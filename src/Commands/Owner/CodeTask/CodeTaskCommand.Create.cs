using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.TextCommands.ContextChecks;
using DSharpPlus.Entities;
using OoLunar.Tomoe.CodeTasks;
using OoLunar.Tomoe.Database.Models;

namespace OoLunar.Tomoe.Commands.Owner
{
    public sealed partial class CodeTaskCommand
    {
        private static readonly string CodeTemplate = CompilerUtilities.GetEmbeddedResource("Tomoe.Templates.CodeTaskTemplate.template")
            .Replace("{{TargetFramework}}", ThisAssembly.Project.TargetFramework)
            .Replace("{{DefaultUsings}}", CompilerUtilities.GetEmbeddedResource("Tomoe.Templates.BenchmarkTemplate.template"))
            .Replace("\n\n", "\nusing OoLunar.Tomoe.CodeTasks;\n\n");

        /// <summary>
        /// Creates a new plugin to be executed on bot startup.
        /// </summary>
        /// <remarks>
        /// Generally used for plugins that don't quite fit into the bot's scope of responsibility but generally isn't desired to create a separate bot for.
        /// </remarks>
        /// <param name="name">The name of the code task.</param>
        /// <param name="code">The file containing the C# code to execute.</param>
        [Command("create")]
        public async ValueTask CreateAsync(CommandContext context, string name, [TextMessageReply] DiscordAttachment code)
        {
            if (string.IsNullOrWhiteSpace(code.FileName) || !code.FileName.EndsWith(".cs", true, await context.GetCultureAsync()))
            {
                await context.RespondAsync("The file must be a C# file.");
                return;
            }

            // Yeah we're gonna be here for a bit.
            await context.DeferResponseAsync();

            // Download the file and read the contents.
            string codeContent = await _httpClient.GetStringAsync(code.Url);

            Ulid id = Ulid.NewUlid();

            // Verify the code compiles
            DiscordMessageBuilder? messageBuilder = await CompileCodeAsync(id, name, codeContent);
            if (messageBuilder is not null)
            {
                await context.RespondAsync(messageBuilder);
                return;
            }

            // Create the code task.
            CodeTaskModel codeTask = await CodeTaskModel.CreateAsync(id, context.Guild!.Id, name, codeContent);
            await CodeTaskRunner.CreateAsync(codeTask);
            CodeTaskRunner.Run(_serviceProvider, id);

            // Respond to the user.
            await context.RespondAsync("Code task created.");
        }

        public static async ValueTask<DiscordMessageBuilder?> CompileCodeAsync(Ulid id, string name, string code)
        {
            string basePath = Path.Combine(Environment.CurrentDirectory, "CodeTasks", id.ToString());

            // Replace `#nuget <package> [version]` with the actual nuget package
            List<string> nugetPackageCommands = [];
            foreach (string line in code.Split('\n'))
            {
                if (line.StartsWith("#nuget"))
                {
                    string[] parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length is 2)
                    {
                        nugetPackageCommands.Add($"add {basePath} package {parts[1]}");
                    }
                    else if (parts.Length is 3)
                    {
                        nugetPackageCommands.Add($"add {basePath} package {parts[1]} --version {parts[2]}");
                    }

                    code = code.Replace(line, string.Empty);
                }
            }

            // Verify the code compiles
            code = CodeTemplate.Replace("{{Code}}", code);

            // Create the base path
            Directory.CreateDirectory(basePath);

            // Normalize the file name
            foreach (char character in Path.GetInvalidFileNameChars())
            {
                name = name.Replace(character, '_');
            }

            // Create the csproj
            await File.WriteAllTextAsync(Path.Combine(basePath, $"{id}.csproj"), CompilerUtilities.CopyProjectCsproj().Replace("Exe", "Library"));

            // Add dependencies
            string output;
            int exitCode;
            foreach (string command in nugetPackageCommands)
            {
                (output, exitCode) = await CompilerUtilities.ExecuteProgramAsync("dotnet", command);
                if (exitCode is not 0)
                {
                    DiscordMessageBuilder messageBuilder = new();
                    messageBuilder.WithContent($"Failed to run `dotnet {command}`. Exit code: {exitCode}");
                    messageBuilder.AddFile($"{id} - {name}.log", new MemoryStream(Encoding.UTF8.GetBytes(output)), AddFileOptions.CloseStream);
                    return messageBuilder;
                }
            }

            // Create the code file
            await File.WriteAllTextAsync(Path.Combine(basePath, $"{name}.cs"), code);

            // Compile the code
            (output, exitCode) = await CompilerUtilities.ExecuteProgramAsync("dotnet", $"publish {basePath} -c Release");
            if (exitCode is not 0)
            {
                DiscordMessageBuilder messageBuilder = new();
                messageBuilder.WithContent($"Failed to compile code task. Exit code: {exitCode}");
                messageBuilder.AddFile($"{id} - {name}.log", new MemoryStream(Encoding.UTF8.GetBytes(output)), AddFileOptions.CloseStream);
                return messageBuilder;
            }

            // Format the code
            (output, exitCode) = await CompilerUtilities.ExecuteProgramAsync("dotnet", $"format {basePath} -vd");
            if (exitCode is not 0)
            {
                DiscordMessageBuilder messageBuilder = new();
                messageBuilder.WithContent($"Failed to format the code. Exit code: {exitCode}");
                messageBuilder.AddFile($"{id} - {name}.log", new MemoryStream(Encoding.UTF8.GetBytes(output)), AddFileOptions.CloseStream);
                return messageBuilder;
            }

            // Load the code
            Assembly assembly = Assembly.LoadFrom(Path.Combine(basePath, "bin", "Release", ThisAssembly.Project.TargetFramework, "publish", $"{id}.dll"));

            // Ensure there's only ONE type that inherits from TaskRunner
            int found = 0;
            foreach (Type type in assembly.GetTypes())
            {
                if (type.IsSubclassOf(typeof(TaskRunner)))
                {
                    found++;
                }
            }

            if (found is not 1)
            {
                DiscordMessageBuilder messageBuilder = new();
                messageBuilder.WithContent($"Failed to compile code task. Found {found} types that inherit from TaskRunner. Please ensure there's only 1.");
                return messageBuilder;
            }

            return null;
        }
    }
}
