using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ArgumentModifiers;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Entities;

namespace OoLunar.Tomoe.Commands.Owner
{
    /// <summary>
    /// Man, that turtle sure is slow! - Bugs Bunny, probably.
    /// </summary>
    public static class BenchmarkCommand
    {
        private static readonly string _csprojTemplate;
        private static readonly string _benchmarkTemplate;
        private static readonly string _programTemplate;

        static BenchmarkCommand()
        {
            // Grab the benchmark template from the embedded resource
            foreach (string resourceFile in typeof(BenchmarkCommand).Assembly.GetManifestResourceNames())
            {
                using Stream manifestStream = typeof(BenchmarkCommand).Assembly.GetManifestResourceStream(resourceFile) ?? throw new InvalidOperationException($"Failed to get the embedded resource {resourceFile}.");
                using StreamReader streamReader = new(manifestStream);
                if (resourceFile == "Tomoe.Tomoe.csproj")
                {
                    _csprojTemplate = streamReader.ReadToEnd();
                }
                else if (resourceFile == "Tomoe.Benchmarks.BenchmarkTemplate.template")
                {
                    _benchmarkTemplate = streamReader.ReadToEnd();
                }
                else if (resourceFile == "Tomoe.Benchmarks.Program.template")
                {
                    _programTemplate = streamReader.ReadToEnd();
                }
            }

            StringBuilder stringBuilder = new();
            using XmlReader xmlReader = XmlReader.Create(new StringReader(_csprojTemplate!));
            while (xmlReader.Read())
            {
                if (xmlReader.NodeType == XmlNodeType.Element && xmlReader.Name == "PackageReference")
                {
                    string name = xmlReader.GetAttribute("Include")!;
                    string version = xmlReader.GetAttribute("Version")!;
                    stringBuilder.AppendLine($"        <PackageReference Include=\"{name}\" Version=\"{version}\" />");
                }
                else if (xmlReader.NodeType == XmlNodeType.Element && xmlReader.Name == "ProjectReference")
                {
                    string include = xmlReader.GetAttribute("Include")!.Replace("$(ProjectRoot)", ThisAssembly.Project.ProjectRoot);
                    stringBuilder.AppendLine($"        <ProjectReference Include=\"{include}\" />");
                }
            }

            _csprojTemplate = $"""
<Project Sdk="Microsoft.NET.Sdk">
    <PropertyGroup>
        <AllowUnsafeBlocks>true</AllowUnsafeBlocks>
        <Configuration>{ThisAssembly.Project.Configuration}"</Configuration>
        <LangVersion>preview</LangVersion>
        <Nullable>enable</Nullable>
        <OutputType>Exe</OutputType>
        <SuppressNETCoreSdkPreviewMessage>true</SuppressNETCoreSdkPreviewMessage>
        <TargetFramework>{ThisAssembly.Project.TargetFramework}</TargetFramework>
    </PropertyGroup>
    <ItemGroup>
        {stringBuilder.ToString().Trim()}
    </ItemGroup>
</Project>
""";
        }

        /// <summary>
        /// Benchmarks the provided code.
        /// </summary>
        /// <param name="code">The code to benchmark.</param>
        [Command("benchmark"), RequireApplicationOwner]
        public static async ValueTask ExecuteAsync(CommandContext context, [FromCode] string code)
        {
            // Yeah we're gonna be here for a bit.
            await context.DeferResponseAsync();

            // Create the assembly name
            Ulid id = Ulid.NewUlid();

            // Create the base path
            string basePath = Path.Combine(Path.GetTempPath(), "Benchmarks", id.ToString());
            Directory.CreateDirectory(basePath);

            // Create the csproj
            await File.WriteAllTextAsync(Path.Combine(basePath, $"{id}.csproj"), _csprojTemplate);

            // Create the code file
            code = _benchmarkTemplate + code;
            await File.WriteAllTextAsync(Path.Combine(basePath, $"{id}.cs"), code);

            // Create the Program file
            await File.WriteAllTextAsync(Path.Combine(basePath, "Program.cs"), _programTemplate);

            // Run the benchmarks
            ProcessStartInfo startInfo = new()
            {
                FileName = "dotnet",
                Arguments = $"run --project {id}.csproj -c Release",
                WorkingDirectory = basePath,
                UseShellExecute = true,
                CreateNoWindow = false,
                WindowStyle = ProcessWindowStyle.Normal
            };

            Process? process = Process.Start(startInfo);
            if (process is null)
            {
                await context.RespondAsync("Failed to start the benchmark process.");
                return;
            }

            // Periodically update Discord with the program title
            CancellationTokenSource cancellationTokenSource = new();
            Task task = Task.Run(async () =>
            {
                await context.EditResponseAsync("Running benchmarks...");

                string? title = null;
                PeriodicTimer timer = new(TimeSpan.FromSeconds(5));
                while (await timer.WaitForNextTickAsync() && !cancellationTokenSource.Token.IsCancellationRequested)
                {
                    if (string.IsNullOrWhiteSpace(process.MainWindowTitle))
                    {
                        continue;
                    }
                    else if (title != process.MainWindowTitle)
                    {
                        title = process.MainWindowTitle;
                        await context.EditResponseAsync(title);
                    }
                }
            });

            await process.WaitForExitAsync();
            await cancellationTokenSource.CancelAsync();
            await task;

            // Send the output
            if (process.ExitCode != 0)
            {
                StringBuilder output = new();
                output.Append(await process.StandardOutput.ReadToEndAsync());
                output.Append(await process.StandardError.ReadToEndAsync());
                DiscordMessageBuilder messageBuilder = new();
                if (output.Length > 1956)
                {
                    messageBuilder.WithContent($"BenchmarkDotNet failed with exit code {process.ExitCode}.");
                    messageBuilder.AddFile("output.log", new MemoryStream(Encoding.UTF8.GetBytes(output.ToString())));
                }
                else
                {
                    messageBuilder.WithContent($"Benchmark failed with exit code {process.ExitCode}.\n```{output}```");
                }

                await context.RespondAsync(messageBuilder);
                return;
            }

            // Dispose of the process as we don't need it anymore
            process.Dispose();

            // Read the header
            string resultsPath = Path.Combine(basePath, "BenchmarkDotNet.Artifacts/results");
            await context.RespondAsync(await File.ReadAllTextAsync(Path.Combine(basePath, "header.md")));

            // Send the results
            foreach (string file in Directory.EnumerateFiles(resultsPath))
            {
                string content = (await File.ReadAllTextAsync(file)).Split("```")[2];
                if (content.Length > 1992)
                {
                    await context.RespondAsync(new DiscordMessageBuilder().AddFile(Path.GetFileName(file), new MemoryStream(Encoding.UTF8.GetBytes(content))));
                }
                else
                {
                    await context.RespondAsync($"```\n{content}\n```");
                }
            }
        }
    }
}
