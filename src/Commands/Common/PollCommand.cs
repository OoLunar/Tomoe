using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.Trees;
using DSharpPlus.Commands.Trees.Attributes;
using DSharpPlus.Entities;
using OoLunar.Tomoe.Database;
using OoLunar.Tomoe.Database.Models;

namespace OoLunar.Tomoe.Commands.Common
{
    public sealed class PollCommand
    {
        private readonly DatabaseExpirableManager<PollModel, Ulid> _pollManager;
        public PollCommand(DatabaseExpirableManager<PollModel, Ulid> pollManager) => _pollManager = pollManager ?? throw new ArgumentNullException(nameof(pollManager));

        [Command("poll"), Description("Create a poll."), RequireGuild]
        public async ValueTask ExecuteAsync(CommandContext context, string question, TimeSpan expiresAt, params string[] options)
        {
            DateTimeOffset now = DateTimeOffset.UtcNow;
            if (now > (now + expiresAt))
            {
                await context.RespondAsync("Please set a reasonable expiration date.");
                return;
            }
            else if (now.AddDays(7) < now.Add(expiresAt))
            {
                await context.RespondAsync("Polls can only last up to 7 days.");
                return;
            }
            else if (options.Length < 2)
            {
                await context.RespondAsync("Polls must have at least 2 options.");
                return;
            }
            else if (options.Length > 24)
            {
                await context.RespondAsync("Polls can have at most 24 options.");
                return;
            }

            for (int i = 0; i < options.Length; i++)
            {
                string option = options[i];
                option = option.Trim();
                if (option.Length > 80)
                {
                    await context.RespondAsync($"Options must be at most 80 characters long. Option \"{option}\" is too long.");
                    return;
                }

                options[i] = option;
            }

            Ulid pollId = Ulid.NewUlid();
            DiscordMessageBuilder messageBuilder = new()
            {
                Content = FormatQuestion(question, expiresAt)
            };

            List<DiscordButtonComponent> buttons = new(5);
            List<DiscordActionRowComponent> buttonRows = new(5);
            for (int i = 0; i < options.Length; i++)
            {
                string option = options[i];
                buttons.Add(new(ButtonStyle.Primary, $"poll:{pollId}:{i.ToString(CultureInfo.InvariantCulture)}", option));
                if ((i + 1) % 5 == 0 && i != 0)
                {
                    buttonRows.Add(new(buttons));
                    buttons.Clear();
                }
            }

            buttons.Add(new(ButtonStyle.Danger, $"poll:{pollId}", "Remove my vote!"));
            buttonRows.Add(new(buttons));
            buttons.Clear();
            messageBuilder.AddComponents(buttonRows);

            await context.RespondAsync(messageBuilder);
            DiscordMessage message = await context.GetResponseAsync() ?? throw new InvalidOperationException("How did we get here?");
            PollModel poll = await PollModel.CreatePollAsync(pollId, context.User.Id, context.Guild!.Id, context.Channel.Id, message.Id, question, now + expiresAt, options);
            _pollManager.AddToCache(poll.Id, poll.ExpiresAt);
        }

        private static string FormatQuestion(string question, TimeSpan expiresAt)
        {
            ReadOnlySpan<char> questionFormatted = question.AsSpan().Trim();
            return !questionFormatted.Contains('\n') && questionFormatted[^1] is '?' or '!' or '.'
                ? $"{questionFormatted} Poll ends {Formatter.Timestamp(expiresAt)}"
                : $"{questionFormatted}\n\nPoll ends {Formatter.Timestamp(expiresAt)}";
        }
    }
}
