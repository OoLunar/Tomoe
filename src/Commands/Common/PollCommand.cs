using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandAll.Commands;
using DSharpPlus.CommandAll.Commands.Attributes;
using DSharpPlus.CommandAll.Converters;
using DSharpPlus.CommandAll.Processors.SlashCommands;
using DSharpPlus.CommandAll.Processors.TextCommands;
using DSharpPlus.CommandAll.Processors.TextCommands.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.DependencyInjection;
using OoLunar.Tomoe.Database;
using OoLunar.Tomoe.Database.Models;
using OoLunar.Tomoe.Events;
using OoLunar.Tomoe.Services;
using OoLunar.Tomoe.Services.Pagination;

namespace OoLunar.Tomoe.Commands.Common
{
    [Command("poll"), TextAlias("democrat", "democratic", "anti-tyrant-deterrent", "vote")]
    public sealed class PollCommand(IServiceProvider serviceProvider, PollService pollService, DatabaseContext databaseContext, PaginatorService paginatorService)
    {
        private static readonly TimeSpanConverter _timeSpanArgumentConverter = new();
        private static readonly DateTimeConverter _dateTimeArgumentConverter = new();
        private readonly PollService _pollService = pollService ?? throw new ArgumentNullException(nameof(pollService));
        private readonly IServiceProvider _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        private readonly DatabaseContext _databaseContext = databaseContext ?? throw new ArgumentNullException(nameof(databaseContext));
        private readonly PaginatorService _paginatorService = paginatorService ?? throw new ArgumentNullException(nameof(paginatorService));

        [Command("create"), TextAlias("new")]
        public async Task CreateAsync(CommandContext context, string question, string timeOrDate, params string[] options)
        {
            MessageCreateEventArgs messageCreateEventArgs = TextCommandUtilities.CreateFakeMessageEventArgs(context, timeOrDate);
            TextConverterContext converterContext = new()
            {
                Channel = context.Channel,
                Command = context.Command,
                Extension = context.Extension,
                RawArguments = timeOrDate,
                ServiceScope = context.ServiceProvider.CreateAsyncScope(),
                Splicer = context.Extension.GetProcessor<TextCommandProcessor>().Configuration.TextArgumentSplicer,
                User = context.User
            };
            converterContext.NextTextArgument();

            if ((await _timeSpanArgumentConverter.ConvertAsync(converterContext, messageCreateEventArgs)).IsDefined(out TimeSpan timeSpan) && timeSpan != TimeSpan.Zero)
            {
                await CreateAsync(context, question, DateTime.UtcNow.Add(timeSpan), options);
            }
            else if ((await _dateTimeArgumentConverter.ConvertAsync(converterContext, messageCreateEventArgs)).IsDefined(out DateTime dateTime))
            {
                await CreateAsync(context, question, dateTime, options);
            }
            else
            {
                await context.RespondAsync("Please provide a valid time or date for the poll to expire.");
            }
        }

        private async Task CreateAsync(CommandContext context, string question, DateTime expiresAt, params string[] options)
        {
            // Pre-execution checks.
            if (string.IsNullOrWhiteSpace(question))
            {
                await context.RespondAsync("Please provide a valid title for the poll. An empty or whitespace title is not allowed.");
                return;
            }
            else if (expiresAt < DateTime.Now.Add(TimeSpan.FromSeconds(25)))
            {
                await context.RespondAsync("Please provide a valid expiration date. The poll must expire at least 30 seconds from now.");
                return;
            }

            // Manually parse the first word of each option to test if they contain an emoji id,
            // then add the emoji to the button.
            Dictionary<string, DiscordComponentEmoji?> choices = [];
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
                await context.RespondAsync("Please provide at least two unique choices and at most 24 unique choices. Extra whitespace (spaces, tabs and newlines at the beginning or end) is stripped from choices.");
                return;
            }

            Guid pollId = Guid.NewGuid();
            List<DiscordActionRowComponent> buttonRows = [];
            List<DiscordComponent> buttons = [];
            for (int i = 0; i < choices.Count; i++)
            {
                (string choice, DiscordComponentEmoji? optionalEmoji) = choices.ElementAt(i);
                buttons.Add(new DiscordButtonComponent(ButtonStyle.Secondary, $"poll:{pollId}:vote:{i}", choice, false, optionalEmoji is not null ? optionalEmoji : null!));

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
            await context.RespondAsync(messageBuilder);
            await _pollService.CreatePollAsync(pollId, question, choices.Keys, expiresAt, context.Guild?.Id, context.Channel.Id, (await context.GetResponseAsync())!.Id);
        }

        [Command("list")]
        public async Task ListAsync(CommandContext context, DiscordChannel? channel = null)
        {
            await context.DelayResponseAsync();

            List<ulong> channelIds = [];
            if (channel is not null)
            {
                if (!context.Member!.PermissionsIn(channel).HasPermission(Permissions.AccessChannels))
                {
                    await context.RespondAsync("You do not have permission to view polls in that channel.");
                    return;
                }

                channelIds.Add(channel.Id);
            }
            else
            {
                foreach (DiscordChannel guildChannel in context.Guild!.Channels.Values)
                {
                    if (context.Member!.PermissionsIn(guildChannel).HasPermission(Permissions.AccessChannels))
                    {
                        channelIds.Add(guildChannel.Id);
                    }
                }
            }

            PollModel[] polls = [.. _databaseContext.Polls.Where(poll => poll.GuildId == context.Guild!.Id && channelIds.Contains(poll.ChannelId))];

            List<Page> pages = [];
            foreach (PollModel poll in polls)
            {
                DiscordMessageBuilder messageBuilder = new();
                messageBuilder.AddFile("poll.png", poll.GenerateBarGraph(), true);

                DiscordEmbedBuilder embedBuilder = new()
                {
                    Title = poll.Question,
                    Color = new DiscordColor("#6b73db"),
                    ImageUrl = "attachment://poll.png",
                    Footer = new() { Text = $"Poll ID: {poll.Id}" }
                };

                embedBuilder.AddField("Expires at", $"{Formatter.Timestamp(poll.ExpiresAt, TimestampFormat.ShortDateTime)} ({Formatter.Timestamp(poll.ExpiresAt, TimestampFormat.RelativeTime)})", true);
                embedBuilder.AddField("Votes", (await _pollService.GetTotalVotesAsync(poll.Id)).ToString(CultureInfo.InvariantCulture), true);
                messageBuilder.AddEmbed(embedBuilder);

                pages.Add(new PageBuilder()
                {
                    Title = poll.Question,
                    Description = $"Poll ID: {poll.Id}",
                    MessageBuilder = messageBuilder
                });
            }

            if (pages.Count == 0)
            {
                await context.EditResponseAsync($"No polls were found{(channel is null ? "." : $" in {channel.Mention}.")}");
            }
            else if (pages.Count == 1)
            {
                await context.EditResponseAsync(pages[0].MessageBuilder);
            }
            else
            {
                Paginator paginator = _paginatorService.CreatePaginator(pages, context.User);
                await context.EditResponseAsync(paginator.GenerateMessage());
                if (context is SlashCommandContext slashCommandContext)
                {
                    paginator.Interaction = slashCommandContext.Interaction;
                }
                else if (context is TextCommandContext textContext)
                {
                    paginator.CurrentMessage = textContext.Response;
                }
            }
        }

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
            if (eventArgs.Message.Author?.Id != client.CurrentUser.Id
                || eventArgs.Message.Components is null
                || eventArgs.Message.Components.Count == 0
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
                    || message.Components.Count == 0
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