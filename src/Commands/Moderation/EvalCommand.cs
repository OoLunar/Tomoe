using System;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ArgumentModifiers;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.Trees;
using DSharpPlus.Entities;
using DSharpPlus.Net.Serialization;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Newtonsoft.Json;

namespace OoLunar.Tomoe.Commands.Moderation
{
    /// <summary>
    /// "dont eval shit" - Jake from Discord
    /// </summary>
    public static class EvalCommand
    {
        /// <summary>
        /// Allows the eval script to access contextual data.
        /// </summary>
        public sealed class EvalContext
        {
            /// <inheritdoc cref="EvalContext" />
            public required CommandContext Context { get; init; }

            /// <inheritdoc cref="EvalContext" />
            public CommandContext context => Context;

            /// <summary>
            /// Whether the returned response was transformed into JSON data or not.
            /// </summary>
            public bool IsJson { get; private set; }

            /// <summary>
            /// The data object to serialize into JSON data.
            /// </summary>
            /// <param name="obj">The object being read.</param>
            /// <returns>The JSON data.</returns>
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

                _discordJson.Serialize(jsonTextWriter, obj, null);
                jsonTextWriter.Flush();
                jsonTextWriter.Close();
                return writer.ToString();
            }
        }

        private static readonly ScriptOptions _evalOptions;
        private static readonly JsonSerializer _discordJson;

        static EvalCommand()
        {
            string[] assemblies = Directory.GetFiles(Path.GetDirectoryName(typeof(EvalCommand).Assembly.Location)!, "*.dll");
            _evalOptions = ScriptOptions.Default
                .WithAllowUnsafe(true)
                .WithEmitDebugInformation(true)
                .WithLanguageVersion(LanguageVersion.Preview)
                .AddReferences(assemblies)
                .AddImports(
                [
                    "System",
                    "System.Threading.Tasks",
                    "System.Text",
                    "System.Reflection",
                    "System.Linq",
                    "System.Globalization",
                    "System.Collections.Generic",
                    "Humanizer",
                    "DSharpPlus",
                    "DSharpPlus.Commands",
                    "DSharpPlus.Net.Serialization",
                    "DSharpPlus.Exceptions",
                    "DSharpPlus.Entities"
                ]);

            _discordJson = (JsonSerializer)typeof(DiscordJson).GetField("serializer", BindingFlags.NonPublic | BindingFlags.Static)!.GetValue(null)!;
        }

        /// <summary>
        /// Runs C# code and returns the output.
        /// </summary>
        /// <remarks>
        /// Yeah you can't run this command.
        /// </remarks>
        /// <param name="context"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        [Command("eval"), Description("Not for you."), RequireApplicationOwner]
        public static async ValueTask ExecuteAsync(CommandContext context, [FromCode] string code)
        {
            await context.DeferResponseAsync();
            Script<object> script = CSharpScript.Create(code, _evalOptions, typeof(EvalContext));
            ImmutableArray<Diagnostic> errors = script.Compile();
            if (errors.Length == 1)
            {
                string errorString = errors[0].ToString();
                await context.EditResponseAsync(errorString.Length switch
                {
                    < 1992 => new DiscordMessageBuilder().WithContent(Formatter.BlockCode(errorString)),
                    _ => new DiscordMessageBuilder().AddFile("errors.log", new MemoryStream(Encoding.UTF8.GetBytes(errorString)))
                });

                return;
            }
            else if (errors.Length > 1)
            {
                await context.EditResponseAsync(new DiscordMessageBuilder().AddFile("errors.log", new MemoryStream(Encoding.UTF8.GetBytes(string.Join("\n", errors.Select(x => x.ToString()))))));
                return;
            }

            EvalContext evalContext = new() { Context = context };
            object output = null!;
            try
            {
                ScriptState<object> state = await script.RunAsync(evalContext);
                output = state.Exception ?? state.ReturnValue;
            }
            catch (Exception error)
            {
                output = error;
            }

            await FinishedAsync(evalContext, output);
        }

        private static async ValueTask FinishedAsync(EvalContext context, object? output)
        {
            string filename = "output.json";
            byte[] utf8Bytes = null!;
            switch (output)
            {
                case null:
                    await context.Context.EditResponseAsync("Eval was successful with nothing returned.");
                    break;
                case TargetInvocationException error when error.InnerException is not null:
                    await FinishedAsync(context, ExceptionDispatchInfo.Capture(error.InnerException).SourceException); // go to Exception error case
                    break;
                case Exception error:
                    string errorString = error.ToString();
                    int indexOfSubmissionCutoff = errorString.IndexOf("--- End of stack trace from previous location ---\n   at Microsoft.CodeAnalysis.Scripting.ScriptExecutionState", StringComparison.Ordinal);
                    if (indexOfSubmissionCutoff != -1)
                    {
                        errorString = errorString[..indexOfSubmissionCutoff].Replace("<<Initialize>>d__0.MoveNext", "MainAsync");
                    }

                    await context.Context.EditResponseAsync(new DiscordMessageBuilder().WithContent(Formatter.BlockCode(errorString, "cs")));
                    break;
                case string outputString when context.IsJson && outputString.Length < 1988:
                    await context.Context.EditResponseAsync(Formatter.BlockCode(outputString, "json"));
                    break;
                case string outputString when context.IsJson:
                    utf8Bytes = Encoding.UTF8.GetBytes(outputString);
                    goto default;
                // < 1992 since Formatter.BlockCode adds a minimum of 8 characters.
                case string outputString when outputString.Contains('\n') && outputString.Length < 1992:
                    await context.Context.EditResponseAsync(Formatter.BlockCode(outputString, "cs"));
                    break;
                case string outputString when outputString.Length >= 1992:
                    filename = "output.txt";
                    utf8Bytes = Encoding.UTF8.GetBytes(outputString);
                    goto default;
                case string outputString:
                    await context.Context.EditResponseAsync(outputString);
                    break;
                case DiscordMessage message:
                    await context.Context.EditResponseAsync(new DiscordMessageBuilder(message));
                    break;
                case DiscordMessageBuilder messageBuilder:
                    await context.Context.EditResponseAsync(messageBuilder);
                    break;
                case DiscordEmbed embed:
                    await context.Context.EditResponseAsync(new DiscordMessageBuilder().AddEmbed(embed));
                    break;
                case DiscordEmbedBuilder embedBuilder:
                    await context.Context.EditResponseAsync(new DiscordMessageBuilder().AddEmbed(embedBuilder));
                    break;
                case DateTime dateTime:
                    await context.Context.EditResponseAsync(Formatter.Timestamp(dateTime, TimestampFormat.RelativeTime));
                    break;
                case DateTimeOffset dateTimeOffset:
                    await context.Context.EditResponseAsync(Formatter.Timestamp(dateTimeOffset, TimestampFormat.RelativeTime));
                    break;
                case TimeSpan timeSpan:
                    await context.Context.EditResponseAsync(Formatter.Timestamp(timeSpan, TimestampFormat.RelativeTime));
                    break;
                case double:
                case decimal:
                case float:
                case ulong:
                case long:
                case uint:
                case int:
                case ushort:
                case short:
                    await context.Context.EditResponseAsync((string)output.GetType().GetMethod("ToString", [typeof(string), typeof(IFormatProvider)])!.Invoke(output, ["N0", await context.Context.GetCultureAsync()])!);
                    break;
                case object when output is not SnowflakeObject:
                    // Check if the ToString method is overridden
                    Type outputType = output.GetType();
                    if (!(outputType.IsClass && outputType.GetMethod("<Clone>$") is not null)
                     && !(outputType.IsValueType && outputType.GetInterfaces().Any(@interface => @interface.IsGenericType && @interface.GetGenericTypeDefinition() == typeof(IEquatable<>)) && outputType.GetMethods().FirstOrDefault(method => method.Name == "op_Equality") is MethodInfo equalityOperator && equalityOperator.GetCustomAttribute<CompilerGeneratedAttribute>() is not null)
                       && outputType.GetMethods(BindingFlags.Public | BindingFlags.Instance).Where(method => method.Name == "ToString").Any(method => method.DeclaringType != typeof(object)))
                    {
                        await context.Context.EditResponseAsync(output.ToString()!);
                        break;
                    }

                    goto default;
                default:
                    utf8Bytes ??= Encoding.UTF8.GetBytes(context.ToJson(output));
                    await context.Context.EditResponseAsync(new DiscordMessageBuilder().AddFile(filename, new MemoryStream(utf8Bytes)));
                    break;
            }
        }
    }
}
