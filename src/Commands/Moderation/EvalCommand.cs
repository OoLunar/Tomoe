using System;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandAll.Attributes;
using DSharpPlus.CommandAll.Commands;
using DSharpPlus.CommandAll.Commands.Checks;
using DSharpPlus.Entities;
using DSharpPlus.Net.Serialization;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Newtonsoft.Json;

namespace OoLunar.Tomoe.Commands.Moderation
{
    public class EvalCommand : BaseCommand
    {
        private static readonly ScriptOptions Options;
        internal static readonly JsonSerializer DiscordJson;

        static EvalCommand()
        {
            string[] assemblies = Directory.GetFiles(Path.GetDirectoryName(typeof(EvalCommand).Assembly.Location)!, "*.dll");
            Options = ScriptOptions.Default
                .WithEmitDebugInformation(true)
                .WithLanguageVersion(LanguageVersion.Latest)
                .AddReferences(assemblies)
                .AddImports(new string[]
                {
                    "System",
                    "System.Threading.Tasks",
                    "System.Text",
                    "System.Reflection",
                    "System.Linq",
                    "System.Globalization",
                    "System.Collections.Generic",
                    "Humanizer",
                    "DSharpPlus",
                    "DSharpPlus.Net.Serialization",
                    "DSharpPlus.Exceptions",
                    "DSharpPlus.Entities",
                    "DSharpPlus.CommandAll"
                });

            DiscordJson = (JsonSerializer)typeof(DiscordJson).GetField("_serializer", BindingFlags.NonPublic | BindingFlags.Static)!.GetValue(null)!;
        }

        public override Task BeforeExecutionAsync(CommandContext context) => context.DelayAsync();

        [Command("eval"), Description("Not for you."), RequireOwnerCheck, SuppressMessage("Roslyn", "CA1822", Justification = "We need the BeforeExecutionAsync method to be ran.")]
        public Task ExecuteAsync(CommandContext context, [RemainingText] string code)
        {
            Script<object> script = CSharpScript.Create(code, Options, typeof(EvalContext));
            ImmutableArray<Diagnostic> errors = script.Compile();

            if (errors.Length == 1)
            {
                string errorString = errors[0].ToString();
                return context.EditAsync(errorString.Length switch
                {
                    < 1992 => new DiscordMessageBuilder().WithContent(Formatter.BlockCode(errorString)),
                    _ => new DiscordMessageBuilder().AddFile("errors.log", new MemoryStream(Encoding.UTF8.GetBytes(errorString)))
                });
            }
            else if (errors.Length > 1)
            {
                return context.EditAsync(new DiscordMessageBuilder().AddFile("errors.log", new MemoryStream(Encoding.UTF8.GetBytes(string.Join("\n", errors.Select(x => x.ToString()))))));
            }

            EvalContext evalContext = new() { Context = context };
            return script.RunAsync(evalContext).ContinueWith((Task<ScriptState<object>> task) => FinishedAsync(evalContext, task.Exception ?? task.Result.Exception ?? task.Result.ReturnValue));
        }

        public static Task FinishedAsync(EvalContext context, object? output)
        {
            string filename = "output.json";
            byte[] utf8Bytes = null!;
            switch (output)
            {
                case null:
                    return context.Context.EditAsync("Eval was successful with nothing returned.");
                case Exception error:
                    throw error;
                case string outputString when context.IsJson && outputString.Length < 1988:
                    return context.Context.EditAsync(Formatter.BlockCode(outputString, "json"));
                case string outputString when context.IsJson:
                    utf8Bytes = Encoding.UTF8.GetBytes(outputString);
                    goto default;
                // < 1992 since Formatter.BlockCode adds a minimum of 8 characters.
                case string outputString when outputString.Contains('\n') && outputString.Length < 1992:
                    return context.Context.EditAsync(Formatter.BlockCode(outputString, "cs"));
                case string outputString when outputString.Length >= 1992:
                    filename = "output.txt";
                    utf8Bytes = Encoding.UTF8.GetBytes(outputString);
                    goto default;
                case string outputString:
                    return context.Context.EditAsync(outputString);
                case DiscordMessage message:
                    return context.Context.EditAsync(new DiscordMessageBuilder(message));
                case DiscordMessageBuilder messageBuilder:
                    return context.Context.EditAsync(messageBuilder);
                case DiscordEmbed embed:
                    return context.Context.EditAsync(new DiscordMessageBuilder().WithEmbed(embed));
                case DiscordEmbedBuilder embedBuilder:
                    return context.Context.EditAsync(new DiscordMessageBuilder().WithEmbed(embedBuilder));
                case object:
                    // Check if the ToString method is overridden
                    if (output.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance).Where(method => method.Name == "ToString").Any(method => method.DeclaringType != typeof(object)))
                    {
                        return context.Context.EditAsync(output.ToString()!);
                    }
                    goto default;
                default:
                    utf8Bytes ??= Encoding.UTF8.GetBytes(context.ToJson(output));
                    return context.Context.EditAsync(new DiscordMessageBuilder().AddFile(filename, new MemoryStream(utf8Bytes)));
            }
        }
    }

    public sealed class EvalContext
    {
        public CommandContext Context { get; init; } = null!;
        public bool IsJson { get; private set; }

        [return: NotNullIfNotNull(nameof(obj))]
        public string? ToJson(object? obj)
        {
            IsJson = true;
            StringWriter writer = new();
            JsonTextWriter jsonTextWriter = new(writer)
            {
                Formatting = Formatting.Indented,
                Indentation = 4,
                StringEscapeHandling = StringEscapeHandling.EscapeNonAscii
            };

            EvalCommand.DiscordJson.Serialize(jsonTextWriter, obj, null);
            jsonTextWriter.Flush();
            jsonTextWriter.Close();
            return writer.ToString();
        }
    }
}
