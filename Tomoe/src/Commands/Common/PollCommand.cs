using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using OoLunar.DSharpPlus.CommandAll.Attributes;
using OoLunar.DSharpPlus.CommandAll.Commands;
using OoLunar.DSharpPlus.CommandAll.Converters;
using OoLunar.Tomoe.Database.Models;
using OoLunar.Tomoe.Services;

namespace OoLunar.Tomoe.Commands.Common
{
    [Command("poll", "democratic")]
    public sealed class PollCommand : BaseCommand
    {
        private static readonly DiscordEmojiArgumentConverter _emojiArgumentConverter = new();
        private readonly PollService _pollService;

        public PollCommand(PollService pollService) => _pollService = pollService ?? throw new ArgumentNullException(nameof(pollService));

        [Command("create", "new")]
        public Task CreateAsync(CommandContext context, string question, params string[] options) => CreateAsync(context, question, DateTime.UtcNow.AddMinutes(5), options);

        [Command("create"), CommandOverloadPriority(0, true)]
        public async Task CreateAsync(CommandContext context, string question, TimeSpan expiresAt, params string[] options) => await CreateAsync(context, question, DateTime.UtcNow.Add(expiresAt), options);

        [Command("create")]
        public async Task CreateAsync(CommandContext context, string question, DateTime expiresAt, params string[] options)
        {
            // Pre-execution checks.
            if (string.IsNullOrWhiteSpace(question))
            {
                await context.ReplyAsync("Please provide a valid title for the poll. An empty or whitespace title is not allowed.");
                return;
            }
            else if (expiresAt < DateTime.UtcNow.Add(TimeSpan.FromSeconds(30)))
            {
                await context.ReplyAsync("Please provide a valid expiration date. The poll must expire at least 30 seconds from now.");
                return;
            }

            Dictionary<string, DiscordEmoji?> choices = new();
            foreach (string option in options)
            {
                // Trim and remove duplicates. Extraneous whitespace can occur from quoted parameters.
                string trimmedChoice = option.Trim();
                if (!string.IsNullOrWhiteSpace(trimmedChoice) && !choices.ContainsKey(trimmedChoice))
                {
                    // Test if the first word is an emoji.
                    string[] splitChoice = trimmedChoice.Split(' ');
                    Optional<DiscordEmoji> emoji = await _emojiArgumentConverter.ConvertAsync(context, null!, splitChoice[0]);
                    choices.Add(string.Join(' ', emoji.HasValue ? splitChoice[1..] : splitChoice), emoji.HasValue ? emoji.Value : null);
                }
            }

            if (choices.Count is < 2 or > 24)
            {
                await context.ReplyAsync("Please provide at least two unique choices and at most 24 unique choices. Extra whitespace (spaces, tabs and newlines at the beginning or end) is stripped from choices.");
                return;
            }

            PollModel poll = _pollService.CreatePoll(question, choices.ToDictionary(option => option.Key, option => option.Value?.ToString()), expiresAt.ToUniversalTime());
            List<DiscordActionRowComponent> buttonRows = new();
            List<DiscordComponent> buttons = new();
            for (int i = 0; i < choices.Count; i++)
            {
                (string choice, DiscordEmoji? optionalEmoji) = choices.ElementAt(i);
                buttons.Add(new DiscordButtonComponent(ButtonStyle.Secondary, $"poll:{poll.Id}:{i}", choice, false, optionalEmoji is not null ? new(optionalEmoji) : null));

                if (buttons.Count == 5)
                {
                    buttonRows.Add(new(buttons));
                    buttons.Clear();
                }
            }

            buttons.Add(new DiscordButtonComponent(ButtonStyle.Danger, $"poll:{poll.Id}:remove", "Remove my vote!"));
            buttonRows.Add(new DiscordActionRowComponent(buttons));

            DiscordEmbedBuilder embedBuilder = new()
            {
                Title = $"Poll Time!",
                Description = question,
                Color = new DiscordColor("#6b73db"),
            };
            embedBuilder.AddField("Expires at", $"{Formatter.Timestamp(expiresAt, TimestampFormat.ShortDateTime)} ({Formatter.Timestamp(expiresAt, TimestampFormat.RelativeTime)})", true);
            embedBuilder.AddField("Votes", $"0 votes so far. {Formatter.Italic("You")}  could be the change!", true); // Double space is intentional.

            DiscordMessageBuilder messageBuilder = new();
            messageBuilder.AddEmbed(embedBuilder);
            messageBuilder.AddComponents(buttonRows);
            await context.ReplyAsync(messageBuilder);
        }

        [Command("list")]
        public static Task ListAsync(CommandContext context) => Task.FromException<NotImplementedException>(new NotImplementedException());
    }
}
