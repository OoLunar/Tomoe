using System;
using System.Collections.Generic;
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

namespace OoLunar.Tomoe.Commands.Owner
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

        internal static readonly ScriptOptions _evalOptions;
        internal static readonly JsonSerializer _discordJson;

        static EvalCommand()
        {
            List<string> imports = [
                "System",
                "System.Collections.Generic",
                "System.Globalization",
                "System.Linq",
                "System.Reflection",
                "System.Runtime.CompilerServices",
                "System.Text",
                "System.Threading.Tasks",
                "BenchmarkDotNet.Attributes",
                "DSharpPlus",
                "DSharpPlus.Commands",
                "DSharpPlus.Entities",
                "DSharpPlus.Exceptions",
                "DSharpPlus.Net.Serialization",
                "Humanizer"
            ];

            // Find all namespaces that start with `DSharpPlus.Commands`
            imports.AddRange(typeof(CommandContext).Assembly.GetTypes().Where(type => type.Namespace?.StartsWith("DSharpPlus.Commands", StringComparison.Ordinal) is true).Select(type => type.Namespace!).Distinct());
            imports.Sort();

            string[] assemblies = Directory.GetFiles(Path.GetDirectoryName(typeof(EvalCommand).Assembly.Location)!, "*.dll");
            _evalOptions = ScriptOptions.Default
                .WithAllowUnsafe(true)
                .WithEmitDebugInformation(true)
                .WithLanguageVersion(LanguageVersion.Preview)
                .AddReferences(assemblies)
                .AddImports(imports);

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
        [Command("eval"), Description("Not for you."), RequireApplicationOwner]
        public static async ValueTask ExecuteAsync(CommandContext context, [FromCode] string code)
        {
            // Yeah we're gonna be here for a bit.
            await context.DeferResponseAsync();

            // Verify the code compiles
            if (VerifyCode(code, out Script<object>? script) is DiscordMessageBuilder errorMessage)
            {
                await context.EditResponseAsync(errorMessage);
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

        internal static DiscordMessageBuilder? VerifyCode(string code, out Script<object> script)
        {
            script = CSharpScript.Create(code, _evalOptions, typeof(EvalContext));
            ImmutableArray<Diagnostic> errors = script.Compile();
            if (errors.Length == 1)
            {
                string errorString = errors[0].ToString();
                return errorString.Length switch
                {
                    < 1992 => new DiscordMessageBuilder().WithContent(Formatter.BlockCode(errorString)),
                    _ => new DiscordMessageBuilder().AddFile("errors.log", new MemoryStream(Encoding.UTF8.GetBytes(errorString)))
                };
            }
            else if (errors.Length > 1)
            {
                return new DiscordMessageBuilder().AddFile("errors.log", new MemoryStream(Encoding.UTF8.GetBytes(string.Join("\n", errors.Select(x => x.ToString())))));
            }

            return null;
        }

        private static async ValueTask FinishedAsync(EvalContext context, object? output)
        {
            const int MaxDiscordMessageLength = 2000;
            const int MaxDiscordMessageLengthWithCodeBlock = MaxDiscordMessageLength - 8;
            const int MaxDiscordMessageLengthWithJsonCodeBlock = MaxDiscordMessageLength - 12;
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

                    // Go to the default case if the error string is too long
                    await FinishedAsync(context, Formatter.BlockCode(errorString, "cs"));
                    break;
                case string outputString when context.IsJson:
                    await context.Context.EditResponseAsync(outputString.Length switch
                    {
                        // If the text is in JSON, then it should always be in a code block or json file.
                        < MaxDiscordMessageLengthWithJsonCodeBlock => new DiscordMessageBuilder().WithContent(Formatter.BlockCode(outputString, "json")),
                        < MaxDiscordMessageLengthWithCodeBlock => new DiscordMessageBuilder().WithContent(Formatter.BlockCode(outputString)),
                        _ => new DiscordMessageBuilder().AddFile("output.json", new MemoryStream(Encoding.UTF8.GetBytes(outputString)))
                    });
                    break;
                case string outputString:
                    await context.Context.EditResponseAsync(outputString.Length switch
                    {
                        // If the text is greater than the max length, then it should be in a file.
                        > MaxDiscordMessageLength => new DiscordMessageBuilder().AddFile("output.txt", new MemoryStream(Encoding.UTF8.GetBytes(outputString))),
                        _ => new DiscordMessageBuilder().WithContent(outputString)
                    });
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
                        await FinishedAsync(context, output.ToString());
                        break;
                    }

                    goto default;
                default:
                    await FinishedAsync(context, context.ToJson(output));
                    break;
            }
        }
    }
}
