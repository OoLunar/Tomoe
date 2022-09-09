using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using OoLunar.Tomoe.Attributes;
using OoLunar.Tomoe.Services;

namespace OoLunar.Tomoe.Events.Handlers
{
    public class CommandErrored
    {
        public AuditService AuditService { private get; set; } = null!;

        [DiscordEventHandler(typeof(CommandsNextExtension), nameof(CommandsNextExtension.CommandErrored))]
        public static Task CommandErroredAsync(CommandsNextExtension _, CommandErrorEventArgs eventArgs)
        {
            if (eventArgs.Exception is CommandNotFoundException commandNotFoundException)
            {
                return eventArgs.Context.RespondAsync($"Unknown command: {commandNotFoundException.CommandName}");
            }

            DiscordEmbedBuilder embedBuilder = new()
            {
                Title = "Command Error",
                Description = $"{Formatter.InlineCode(eventArgs.Command!.Name)} failed to execute.",
                Color = new DiscordColor("#6b73db")
            };

            switch (eventArgs.Exception)
            {
                case DiscordException discordError:
                    embedBuilder.AddField("HTTP Code", discordError.WebResponse.ResponseCode.ToString(), true);
                    embedBuilder.AddField("Error Message", discordError.JsonMessage, true);
                    return eventArgs.Context.RespondAsync(embedBuilder.Build());
                case TargetInvocationException:
                    embedBuilder.AddField("Error Message", eventArgs.Exception.InnerException!.Message, true);
                    embedBuilder.AddField("Stack Trace", Formatter.BlockCode(FormatStackTrace(eventArgs.Exception.InnerException.StackTrace), "csharp"), false);
                    return eventArgs.Context.RespondAsync(embedBuilder.Build());
                default:
                    embedBuilder.AddField("Error Message", eventArgs.Exception.Message, true);
                    embedBuilder.AddField("Stack Trace", Formatter.BlockCode(FormatStackTrace(eventArgs.Exception.StackTrace), "csharp"), false);
                    return eventArgs.Context.RespondAsync(embedBuilder.Build());
            }
        }

        public static string FormatStackTrace(string? text) => text == null
            ? "No stack trace available."
            : string.Join('\n', text.Split('\n').Select(line => ReplaceFirst(line.Trim(), "at", "-")));

        public static string ReplaceFirst(string text, string search, string replace)
        {
            int pos = text.IndexOf(search);
            return pos < 0 ? text : string.Concat(text.AsSpan(0, pos), replace, text.AsSpan(pos + search.Length));
        }
    }
}
