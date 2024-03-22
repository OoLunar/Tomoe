using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Commands;
using DSharpPlus.Commands.EventArgs;
using DSharpPlus.Commands.Exceptions;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using Humanizer;

namespace OoLunar.Tomoe.Events.Handlers
{
    public sealed class CommandErroredEventHandlers
    {
        [DiscordEvent]
        public static async Task OnErroredAsync(CommandsExtension _, CommandErroredEventArgs eventArgs)
        {
            if (eventArgs.Exception is CommandNotFoundException commandNotFoundException)
            {
                await eventArgs.Context.RespondAsync($"Unknown command: {commandNotFoundException.CommandName}");
                return;
            }

            DiscordEmbedBuilder embedBuilder = new()
            {
                Title = "Command Error",
                Description = $"{Formatter.InlineCode(eventArgs.Context.Command.FullName)} failed to execute.",
                Color = new DiscordColor("#6b73db")
            };

            switch (eventArgs.Exception)
            {
                case DiscordException discordError:
                    embedBuilder.AddField("HTTP Code", discordError.Response?.StatusCode.ToString() ?? "Not provided.", true);
                    embedBuilder.AddField("Error Message", discordError.JsonMessage ?? "Not provided.", true);
                    await eventArgs.Context.RespondAsync(new DiscordMessageBuilder().AddEmbed(embedBuilder));
                    break;
                default:
                    Exception? innerMostException = eventArgs.Exception;
                    while (innerMostException?.InnerException is not null)
                    {
                        innerMostException = innerMostException.InnerException;
                    }

                    embedBuilder.AddField("Error Message", innerMostException?.Message ?? "No message provided.", true);
                    embedBuilder.AddField("Stack Trace", Formatter.BlockCode(FormatStackTrace(eventArgs.Exception.StackTrace).Truncate(1014, "…"), "cs"), false);
                    await eventArgs.Context.RespondAsync(new DiscordMessageBuilder().AddEmbed(embedBuilder));
                    break;
            }
        }

        private static string FormatStackTrace(string? text) => string.IsNullOrWhiteSpace(text)
            ? "No stack trace available."
            : string.Join('\n', text.Split('\n').Select(line => ReplaceFirst(line.Trim(), "at", "-")));

        private static string ReplaceFirst(string text, string search, string replace)
        {
            ReadOnlySpan<char> textSpan = text.AsSpan();
            int pos = textSpan.IndexOf(search);
            return pos < 0 ? text : string.Concat(textSpan[..pos], replace, textSpan[(pos + search.Length)..]);
        }
    }
}
