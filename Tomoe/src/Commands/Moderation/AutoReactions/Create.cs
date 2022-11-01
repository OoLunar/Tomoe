using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Humanizer;
using Tomoe.Commands.Attributes;
using Tomoe.Db;

namespace Tomoe.Commands.Moderation
{
    [SlashCommandGroup("autoreact", "Adds a new reaction on every message sent in a specified guild channel.")]
    public sealed partial class AutoReactions : ApplicationCommandModule
    {
        private static Regex EmojiRegex { get; } = EmojiRegexMethod();
        public Database Database { private get; set; } = null!;

        [SlashCommand("create", "Creates a new autoreaction on a channel."), Hierarchy(Permissions.ManageChannels | Permissions.ManageMessages)]
        public async Task CreateAsync(InteractionContext context, [Option("channel", "Which guild channel to autoreact too.")] DiscordChannel channel, [Option("emoji", "Which emoji to react with.")] string emojiString)
        {
            if (!DiscordEmoji.TryFromUnicode(context.Client, emojiString, out DiscordEmoji emoji))
            {
                Match match = EmojiRegex.Match(emojiString);
                string emojiIdString = match.Groups["id"].Value;
                if (!ulong.TryParse(emojiIdString, NumberStyles.Integer, CultureInfo.InvariantCulture, out ulong emojiId))
                {
                    await context.EditResponseAsync(new()
                    {
                        Content = $"Error: {emojiString} is not a valid emoji!"
                    });
                    return;
                }

                if (!DiscordEmoji.TryFromGuildEmote(context.Client, emojiId, out emoji))
                {
                    await context.EditResponseAsync(new()
                    {
                        Content = $"Error: {emojiString} is not a valid emoji!"
                    });
                    return;
                }
            }

            if (channel.Type != ChannelType.Text && channel.Type != ChannelType.News && channel.Type != ChannelType.Category)
            {
                await context.EditResponseAsync(new()
                {
                    Content = $"Error: {channel.Mention} is not a text or category channel!"
                });
                return;
            }

            List<string> channelsAffected = new();

            if (channel.Type is ChannelType.Text or ChannelType.News)
            {
                AutoReaction? autoReaction = Database.AutoReactions.FirstOrDefault(databaseAutoReaction => databaseAutoReaction.GuildId == context.Guild.Id && databaseAutoReaction.ChannelId == channel.Id && databaseAutoReaction.EmojiName == emoji.ToString());
                if (autoReaction == null)
                {
                    autoReaction = new()
                    {
                        GuildId = context.Guild.Id,
                        ChannelId = channel.Id,
                        EmojiName = emoji.ToString()
                    };
                    Database.AutoReactions.Add(autoReaction);
                    await Database.SaveChangesAsync();
                    channelsAffected.Add(channel.Mention);
                }
            }
            else
            {
                foreach (DiscordChannel subChannel in channel.Children)
                {
                    AutoReaction? autoReaction = Database.AutoReactions.FirstOrDefault(databaseAutoReaction => databaseAutoReaction.GuildId == context.Guild.Id && databaseAutoReaction.ChannelId == subChannel.Id && databaseAutoReaction.EmojiName == emoji.ToString());
                    if (autoReaction == null)
                    {
                        autoReaction = new()
                        {
                            GuildId = context.Guild.Id,
                            ChannelId = subChannel.Id,
                            EmojiName = emoji.ToString()
                        };
                        Database.AutoReactions.Add(autoReaction);
                        channelsAffected.Add(subChannel.Mention);
                    }
                }
            }

            Dictionary<string, string> keyValuePairs = new()
                {
                    { "guild_name", context.Guild.Name },
                    { "guild_count", Program.TotalMemberCount[context.Guild.Id].ToMetric() },
                    { "moderator_username", context.Member.Username },
                    { "moderator_tag", context.Member.Discriminator },
                    { "moderator_mention", context.Member.Mention },
                    { "moderator_id", context.Member.Id.ToString(CultureInfo.InvariantCulture) },
                    { "moderator_displayname", context.Member.DisplayName },
                    { "channels_affected", channelsAffected.Humanize() },
                    { "channel_emoji", emoji }
                };
            await ModLogAsync(context.Guild, keyValuePairs, CustomEvent.AutoReactionCreate, Database, true);

            await context.EditResponseAsync(new()
            {
                Content = "Channel" + (channelsAffected.Count != 1 ? "s" : "") + $" {channelsAffected.Humanize()} will now have the emoji {emoji} reacted on every new message."
            });
        }

        [GeneratedRegex("^<(?<animated>a)?:(?<name>[a-zA-Z0-9_]+?):(?<id>\\d+?)>$", RegexOptions.Compiled | RegexOptions.ECMAScript)]
        private static partial Regex EmojiRegexMethod();
    }
}
