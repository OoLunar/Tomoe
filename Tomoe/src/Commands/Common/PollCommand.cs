using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.DependencyInjection;
using OoLunar.DSharpPlus.CommandAll.Attributes;
using OoLunar.DSharpPlus.CommandAll.Commands;
using OoLunar.DSharpPlus.CommandAll.Converters;
using OoLunar.Tomoe.Database.Models;
using OoLunar.Tomoe.Events;
using OoLunar.Tomoe.Services;

namespace OoLunar.Tomoe.Commands.Common
{
    [Command("poll", "democrat", "democratic", "anti-tyrant-deterrent")]
    public sealed class PollCommand : BaseCommand
    {
        private static readonly TimeSpanArgumentConverter _timeSpanArgumentConverter = new();
        private static readonly DateTimeArgumentConverter _dateTimeArgumentConverter = new();
        private readonly PollService _pollService;
        private readonly IServiceProvider _serviceProvider;

        public PollCommand(PollService pollService, IServiceProvider serviceProvider)
        {
            _pollService = pollService ?? throw new ArgumentNullException(nameof(pollService));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        [Command("create", "new"), CommandOverloadPriority(0, true)]
        public async Task CreateAsync(CommandContext context, string question, string timeOrDate, params string[] options)
        {
            if ((await _timeSpanArgumentConverter.ConvertAsync(context, null!, timeOrDate)).IsDefined(out TimeSpan timeSpan) && timeSpan != TimeSpan.Zero)
            {
                await CreateAsync(context, question, DateTime.UtcNow.Add(timeSpan), options);
            }
            else if ((await _dateTimeArgumentConverter.ConvertAsync(context, null!, timeOrDate)).IsDefined(out DateTime dateTime))
            {
                await CreateAsync(context, question, dateTime, options);
            }
            else
            {
                await context.ReplyAsync("Please provide a valid time or date for the poll to expire.");
            }
        }

        private async Task CreateAsync(CommandContext context, string question, DateTime expiresAt, params string[] options)
        {
            // Pre-execution checks.
            if (string.IsNullOrWhiteSpace(question))
            {
                await context.ReplyAsync("Please provide a valid title for the poll. An empty or whitespace title is not allowed.");
                return;
            }
            else if (expiresAt < DateTime.Now.Add(TimeSpan.FromSeconds(25)))
            {
                await context.ReplyAsync("Please provide a valid expiration date. The poll must expire at least 30 seconds from now.");
                return;
            }

            // Manually parse the first word of each option to test if they contain an emoji id,
            // then add the emoji to the button.
            Dictionary<string, DiscordComponentEmoji?> choices = new();
            foreach (string option in options)
            {
                // Skip null or empty strings and duplicates.
                if (choices.ContainsKey(option))
                {
                    continue;
                }

                // Split the option into an array of words.
                string[] words = option.Split(' ');

                // Check if the first word is an emoji.
                if (words.Length > 0)
                {
                    if (!words[0].Contains(':') && DiscordEmoji.IsValidUnicode(words[0]))
                    {
                        // Add the emoji to the dictionary.
                        choices.Add(string.Join(' ', words[1..]), new DiscordComponentEmoji(words[0]));
                        continue;
                    }
                    else
                    {
                        // Split the emoji into an array of parts.
                        string[] parts = words[0].Split(':');

                        // Try to parse the emoji ID.
                        if (parts.Length > 1 && ulong.TryParse(parts[1][..(parts[1].Length - 1)], out ulong emojiId))
                        {
                            // Add the emoji to the dictionary.
                            choices.Add(option, new DiscordComponentEmoji(emojiId));
                            continue;
                        }
                    }
                }

                // Add the option as a regular choice.
                choices.Add(option, null);
            }

            if (choices.Count is < 2 or > 24)
            {
                await context.ReplyAsync("Please provide at least two unique choices and at most 24 unique choices. Extra whitespace (spaces, tabs and newlines at the beginning or end) is stripped from choices.");
                return;
            }

            Guid pollId = Guid.NewGuid();
            List<DiscordActionRowComponent> buttonRows = new();
            List<DiscordComponent> buttons = new();
            for (int i = 0; i < choices.Count; i++)
            {
                (string choice, DiscordComponentEmoji? optionalEmoji) = choices.ElementAt(i);
                buttons.Add(new DiscordButtonComponent(ButtonStyle.Secondary, $"poll:{pollId}:vote:{i}", choice, false, optionalEmoji is not null ? optionalEmoji : null));

                if (buttons.Count == 5)
                {
                    buttonRows.Add(new(buttons));
                    buttons.Clear();
                }
            }

            buttons.Add(new DiscordButtonComponent(ButtonStyle.Danger, $"poll:{pollId}:remove", "Remove my vote!"));
            buttonRows.Add(new DiscordActionRowComponent(buttons));

            DiscordEmbedBuilder embedBuilder = new()
            {
                Title = $"Poll Time!",
                Description = question,
                Color = new DiscordColor("#6b73db"),
                Footer = new() { Text = $"Poll ID: {pollId}" }
            };
            embedBuilder.AddField("Expires at", $"{Formatter.Timestamp(expiresAt, TimestampFormat.ShortDateTime)} ({Formatter.Timestamp(expiresAt, TimestampFormat.RelativeTime)})", true);
            embedBuilder.AddField("Votes", $"0 votes so far. {Formatter.Italic("You")}  could be the change!", true); // Double space is intentional.

            DiscordMessageBuilder messageBuilder = new();
            messageBuilder.AddEmbed(embedBuilder);
            messageBuilder.AddComponents(buttonRows);
            messageBuilder.AddMentions(Mentions.All);
            await context.ReplyAsync(messageBuilder);
            await _pollService.CreatePollAsync(pollId, question, choices.Keys, expiresAt, context.Guild?.Id, context.Channel.Id, (await context.GetOriginalResponse()).Id);
        }

        [Command("list")]
        public static Task ListAsync(CommandContext context) => Task.FromException<NotImplementedException>(new NotImplementedException());

        [DiscordEvent]
        public async Task PollSubmittedAsync(DiscordClient client, ComponentInteractionCreateEventArgs eventArgs)
        {
            string[] splitCustomId = eventArgs.Id.Split(':');
            if (splitCustomId.Length < 3 || splitCustomId[0] != "poll" || !Guid.TryParse(splitCustomId[1], out Guid pollId))
            {
                return;
            }

            PollService pollService = _serviceProvider.CreateScope().ServiceProvider.GetRequiredService<PollService>();
            PollModel? poll = await pollService.GetPollAsync(pollId);
            if (poll is null)
            {
                await eventArgs.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AsEphemeral().WithContent("I'm sorry, this poll has ended!"));
                return;
            }
            else if (poll.ExpiresAt <= DateTime.UtcNow)
            {
                await eventArgs.Interaction.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AsEphemeral().WithContent("The results of the poll are currently being tallied!"));
                return;
            }

            DiscordMessageBuilder messageBuilder = new(eventArgs.Message);

            switch (splitCustomId[2])
            {
                case "remove":
                    DiscordFollowupMessageBuilder responseBuilder = new DiscordFollowupMessageBuilder()
                        .AsEphemeral()
                        .WithContent(await pollService.RemoveVoteAsync(poll.Id, eventArgs.User.Id) ? "Your vote has been removed!" : "You haven't voted yet!");

                    messageBuilder.Embeds[0].Fields[1].Value = (await pollService.GetTotalVotesAsync(poll.Id)).ToString("N0");
                    await eventArgs.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage, new DiscordInteractionResponseBuilder(messageBuilder));
                    await eventArgs.Interaction.CreateFollowupMessageAsync(responseBuilder);
                    return;
                case "vote":
                    if (!int.TryParse(splitCustomId[3], out int choiceIndex) || choiceIndex < 0 || choiceIndex >= poll.Options.Length)
                    {
                        return;
                    }

                    await pollService.SetVoteAsync(poll.Id, eventArgs.User.Id, choiceIndex);
                    messageBuilder.Embeds[0].Fields[1].Value = (await pollService.GetTotalVotesAsync(poll.Id)).ToString("N0");
                    await eventArgs.Interaction.CreateResponseAsync(InteractionResponseType.UpdateMessage, new DiscordInteractionResponseBuilder(messageBuilder));
                    await eventArgs.Interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder().AsEphemeral().WithContent($"You successfully voted for option \"{poll.Options[choiceIndex]}\""));
                    return;
                default:
                    return;
            }
        }

        [DiscordEvent]
        public async Task PollDeletedAsync(DiscordClient client, MessageDeleteEventArgs eventArgs)
        {
            if (eventArgs.Message.Author.Id != client.CurrentUser.Id
                || eventArgs.Message.Components is null
                || !eventArgs.Message.Components.Any()
                || eventArgs.Message.Components.FirstOrDefault()?.Components?.FirstOrDefault() is not DiscordButtonComponent button)
            {
                return;
            }

            string[] splitCustomId = button.CustomId.Split(':');
            if (splitCustomId.Length < 3 || splitCustomId[0] != "poll" || !Guid.TryParse(splitCustomId[1], out Guid pollId))
            {
                return;
            }

            PollService pollService = _serviceProvider.CreateScope().ServiceProvider.GetRequiredService<PollService>();
            PollModel? poll = await pollService.GetPollAsync(pollId);
            if (poll is not null)
            {
                await pollService.RemovePollAsync(poll.Id);
            }
        }

        [DiscordEvent]
        public async Task PollDeletedAsync(DiscordClient client, MessageBulkDeleteEventArgs eventArgs)
        {
            PollService pollService = _serviceProvider.CreateScope().ServiceProvider.GetRequiredService<PollService>();
            foreach (DiscordMessage message in eventArgs.Messages)
            {
                if (message.Author.Id != client.CurrentUser.Id
                    || message.Components is null
                    || !message.Components.Any()
                    || message.Components.FirstOrDefault()?.Components?.FirstOrDefault() is not DiscordButtonComponent button)
                {
                    return;
                }

                string[] splitCustomId = button.CustomId.Split(':');
                if (splitCustomId.Length < 3 || splitCustomId[0] != "poll" || !Guid.TryParse(splitCustomId[1], out Guid pollId))
                {
                    return;
                }

                PollModel? poll = await pollService.GetPollAsync(pollId);
                if (poll is not null)
                {
                    await pollService.RemovePollAsync(poll.Id);
                }
            }
        }
    }
}
