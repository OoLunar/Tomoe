using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.Entities;
using Tomoe.Models;
using Tomoe.Utils;

namespace Tomoe.Commands.Common
{
    public class Poll : BaseCommandModule
    {
        public DatabaseList<PollModel, Guid> PollModelList { private get; set; } = null!;

        [Command("poll")]
        [Description("Creates a public message for people to vote on.")]
        public async Task PollAsync(CommandContext context, [Description("The question for people to vote on. Use quotes (\"Like this\") to use multiple words.")] string question, [Description("When to end the poll.")] TimeSpan timeout, [Description("Which channel to post the poll in. Defaults to the current channel.")] DiscordChannel? channel = null, [Description("A list of options to choose from.")] params string[] wordAndEmojiList)
        {
            channel ??= context.Channel;
            if (wordAndEmojiList.Length < 2)
            {
                await context.RespondAsync("You need to provide at least two options to vote on.");
                return;
            }
            else if (wordAndEmojiList.Length > 24)
            {
                await context.RespondAsync("You can only provide up to 24 options to vote on.");
                return;
            }
            else if (!channel.PermissionsFor(context.Member).HasPermission(Permissions.SendMessages))
            {
                await context.RespondAsync($"[Error]: You cannot create a poll in {channel.Mention} due to Discord permissions!");
                return;
            }
            else if (!channel.PermissionsFor(context.Guild.CurrentMember).HasPermission(Permissions.SendMessages))
            {
                await context.RespondAsync($"[Error]: I cannot create a poll in {channel.Mention} due to Discord permissions!");
                return;
            }

            PollModel poll = new()
            {
                GuildId = context.Guild.Id,
                ChannelId = channel.Id,
                UserId = context.User.Id,
                Question = question,
                ExpiresAt = DateTime.UtcNow.Add(timeout),
                Votes = new Dictionary<string, ulong[]>(wordAndEmojiList.Select(x => new KeyValuePair<string, ulong[]>(x, Array.Empty<ulong>())))
            };
            PollModelList.Add(poll);

            List<DiscordActionRowComponent> actionRowComponents = new();
            List<DiscordButtonComponent> buttons = new();
            IArgumentConverter<DiscordEmoji> emojiArgumentConverter = new DiscordEmojiConverter();
            foreach (string wordOrEmoji in wordAndEmojiList)
            {
                if (buttons.Count == 5)
                {
                    actionRowComponents.Add(new DiscordActionRowComponent(buttons));
                    buttons.Clear();
                }

                DiscordButtonComponent button;
                Optional<DiscordEmoji> isEmoji = await emojiArgumentConverter.ConvertAsync(wordOrEmoji, context);
                button = isEmoji.IsDefined(out DiscordEmoji? emoji) && emoji != null
                                                        ? (new(ButtonStyle.Secondary, $"poll\v{poll.Id}\v{wordOrEmoji}", null, false, new DiscordComponentEmoji(emoji)))
                                                        : (new(ButtonStyle.Secondary, $"poll\v{poll.Id}\v{wordOrEmoji}", wordOrEmoji));

                buttons.Add(button);
            }
            buttons.Add(new DiscordButtonComponent(ButtonStyle.Danger, $"poll\v{poll.Id}\v\tcancel", "Remove my vote!"));
            actionRowComponents.Add(new DiscordActionRowComponent(buttons));
            buttons.Clear();

            DiscordMessageBuilder builder = new();
            builder.Content = $"{question}\nThe poll ends {Formatter.Timestamp(timeout)}";
            builder.AddComponents(actionRowComponents);

            DiscordMessage message;
            if (channel == context.Channel)
            {
                message = await context.RespondAsync(builder);
            }
            else
            {
                message = await channel.SendMessageAsync(builder);
                await context.RespondAsync("Poll created!");
            }

            poll.MessageId = message.Id;
            PollModelList.Update(poll);
        }

        public static async Task VoteExpired(object? sender, PollModel pollModel)
        {
            if (!Program.BotReady)
            {
                return;
            }

            DatabaseList<PollModel, Guid> pollModelList = (DatabaseList<PollModel, Guid>)sender!;

            string winner;
            KeyValuePair<string, ulong[]> winnerVote = pollModel.Votes.FirstOrDefault(x => x.Value.Length > 0);
            winner = winnerVote.Key == null ? "Nobody! There weren't any votes..." : $"{winnerVote.Key} (with {winnerVote.Value.Length} vote{(winnerVote.Value.Length == 1 ? "" : "s")}!)";

            pollModelList.Remove(pollModel);

            DiscordClient client = Program.DiscordShardedClient.GetShard(pollModel.GuildId);
            if (client == null)
            {
                // TODO: Should probably log here.
                return;
            }

            if (!client.Guilds.TryGetValue(pollModel.GuildId, out DiscordGuild? guild) || guild == null)
            {
                // TODO: Should probably log here.
                return;
            }

            if (!guild.Channels.TryGetValue(pollModel.ChannelId, out DiscordChannel? channel) || channel == null || !channel.PermissionsFor(guild.CurrentMember).HasPermission(Permissions.SendMessages))
            {
                // TODO: Should probably log here.
                return;
            }

            DiscordMessage message = await channel.GetMessageAsync(pollModel.MessageId);
            List<DiscordActionRowComponent> actionRowComponents = new(5);
            foreach (DiscordActionRowComponent component in message.Components)
            {
                List<DiscordButtonComponent> buttons = new(5);
                foreach (DiscordButtonComponent button in component.Components)
                {
                    buttons.Add(button.Disable());
                }
                actionRowComponents.Add(new DiscordActionRowComponent(buttons));
            }

            DiscordMessageBuilder messageBuilder = new();
            messageBuilder.Content = Formatter.Strike(pollModel.Question) + "\nThe poll has ended! Thanks for submitting your vote!";
            messageBuilder.AddComponents(actionRowComponents);
            await message.ModifyAsync(messageBuilder);

            DiscordMessageBuilder builder = new();
            builder.Content = $"> {pollModel.Question}\n<@{pollModel.UserId}>, the poll has ended! The winner is: {winner}";
            builder.WithAllowedMention(new UserMention(pollModel.UserId));
            builder.WithReply(message.Id);
            await channel.SendMessageAsync(builder);
        }
    }
}