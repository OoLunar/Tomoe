using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.Entities;
using EdgeDB;
using Microsoft.Extensions.Logging;
using OoLunar.Tomoe.Database;
using OoLunar.Tomoe.Services;

namespace OoLunar.Tomoe.Commands.Common
{
    public sealed class PollCommand : BaseCommandModule
    {
        public ExpirableService<PollModel> PollService { private get; set; } = null!;
        public ILogger<PollCommand> Logger { private get; set; } = null!;
        public CancellationTokenSource CancellationTokenSource { private get; set; } = null!;

        public EdgeDBClient EdgeDBClient { private get; set; } = null!;
        public ILogger<PollModel> PollLogger { private get; set; } = null!;

        [Command("poll"), Description("Creates a fair, democratic poll."), RequireBotPermissions(Permissions.EmbedLinks)]
        public async Task PollAsync(CommandContext context, [Description("The question to ask.")] string question, DateTimeOffset? expiresAt = null, [Description("The options to choose from.")] params string[] options)
        {
            // Pre-execution checks.
            if (string.IsNullOrWhiteSpace(question))
            {
                await context.RespondAsync("Please provide a valid title for the poll. An empty or whitespace title is not allowed.");
                return;
            }

            List<string> choices = new();
            //The foreach is basically: options.Select(choice => choice.Trim()).Where(choice => !string.IsNullOrWhiteSpace(choice)).Distinct()
            foreach (string option in options)
            {
                // Trim and remove duplicates. Extraneous whitespace can occur from quoted parameters.
                string trimmedChoice = option.Trim();
                if (string.IsNullOrWhiteSpace(trimmedChoice) || choices.Contains(trimmedChoice))
                {
                    continue;
                }
                choices.Add(trimmedChoice);
            }

            if (choices.Count is < 2 or > 24)
            {
                await context.RespondAsync("Please provide at least two unique choices and at most 24 unique choices. Extra whitespace (spaces, tabs and newlines at the beginning or end) is stripped from choices.");
                return;
            }

            expiresAt ??= DateTimeOffset.UtcNow.AddMinutes(5);
            if (expiresAt < DateTimeOffset.UtcNow)
            {
                await context.RespondAsync("Please provide a valid date and time in the future.");
                return;
            }

            // Create the poll message.
            DiscordEmbedBuilder embedBuilder = new()
            {
                Title = $"Poll Time!",
                Description = question,
                Color = new DiscordColor(0x6b73db),
            };
            embedBuilder.AddField("Options", string.Join("\n", choices));
            embedBuilder.AddField("Expires at", $"{Formatter.Timestamp(expiresAt.Value, TimestampFormat.ShortDateTime)} ({Formatter.Timestamp(expiresAt.Value, TimestampFormat.RelativeTime)})", true);
            embedBuilder.AddField("Votes", $"0 votes so far. {Formatter.Italic("You")}  could be the change!", true); // Double space is intentional.

            PollModel poll = new(EdgeDBClient, PollLogger, context.User.Id, context.Guild?.Id, context.Channel.Id, null, question.Trim(), expiresAt.Value, choices);
            poll = await PollService.AddAsync(poll);

            IArgumentConverter<DiscordEmoji> emojiArgumentConverter = new DiscordEmojiConverter();
            List<DiscordActionRowComponent> buttonRows = new();
            List<DiscordComponent> buttons = new();
            for (int i = 0; i < choices.Count; i++)
            {
                string choice = choices[i];
                string[] choiceSplit = choice.Split(' ');
                DSharpPlus.Entities.Optional<DiscordEmoji> optionalEmoji = await emojiArgumentConverter.ConvertAsync(choiceSplit[0], context);
                if (optionalEmoji.HasValue)
                {
                    buttons.Add(new DiscordButtonComponent(ButtonStyle.Secondary, $"poll\v{poll.Id}\v{i}", string.Join(' ', choiceSplit.Skip(1)), false, new(optionalEmoji.Value)));
                }
                else
                {
                    buttons.Add(new DiscordButtonComponent(ButtonStyle.Secondary, $"poll\v{poll.Id}\v{i}", choice, false));
                }

                if (buttons.Count == 5)
                {
                    buttonRows.Add(new(buttons));
                    buttons.Clear();
                }
            }
            buttons.Add(new DiscordButtonComponent(ButtonStyle.Danger, $"poll\v{poll.Id}\vremove", "Remove my vote!"));
            buttonRows.Add(new DiscordActionRowComponent(buttons));

            DiscordMessageBuilder messageBuilder = new();
            messageBuilder.AddEmbed(embedBuilder.Build());
            messageBuilder.AddComponents(buttonRows);
            await poll.UpdateMessageIdAsync(await context.RespondAsync(messageBuilder), CancellationTokenSource.Token);

            Logger.LogInformation("Created poll {PollId} on message {MessageId} in channel {ChannelId} by user {UserId}.", poll.Id, poll.MessageId, context.Channel.Id, context.User.Id);
        }
    }
}
