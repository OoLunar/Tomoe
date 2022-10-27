namespace Tomoe.Commands
{
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

    public partial class Moderation : ApplicationCommandModule
    {
        public partial class AutoReactions : ApplicationCommandModule
        {
            [SlashCommand("delete", "Deletes an autoreaction from a specified channel."), Hierarchy(Permissions.ManageChannels | Permissions.ManageMessages)]
            public async Task Delete(InteractionContext context, [Option("channel", "Which guild channel to remove the autoreaction from.")] DiscordChannel channel, [Option("emoji", "Which emoji to react with.")] string emojiString)
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
                    AutoReaction autoReaction = Database.AutoReactions.FirstOrDefault(databaseAutoReaction => databaseAutoReaction.GuildId == context.Guild.Id && databaseAutoReaction.ChannelId == channel.Id && databaseAutoReaction.EmojiName == emoji.ToString());
                    if (autoReaction != null)
                    {
                        Database.AutoReactions.Remove(autoReaction);
                        await Database.SaveChangesAsync();
                        channelsAffected.Add(channel.Mention);
                    }
                }
                else
                {
                    foreach (DiscordChannel subChannel in channel.Children)
                    {
                        AutoReaction autoReaction = Database.AutoReactions.FirstOrDefault(databaseAutoReaction => databaseAutoReaction.GuildId == context.Guild.Id && databaseAutoReaction.ChannelId == subChannel.Id && databaseAutoReaction.EmojiName == emoji.ToString());
                        if (autoReaction != null)
                        {
                            Database.AutoReactions.Remove(autoReaction);
                            channelsAffected.Add(subChannel.Mention);
                        }
                    }
                    if (channelsAffected.Count != 0)
                    {
                        await Database.SaveChangesAsync();
                    }
                }

                Dictionary<string, string> keyValuePairs = new()
                {
                    { "guild_name", context.Guild.Name },
                    { "guild_count", Public.TotalMemberCount[context.Guild.Id].ToMetric() },
                    { "moderator_username", context.Member.Username },
                    { "moderator_tag", context.Member.Discriminator },
                    { "moderator_mention", context.Member.Mention },
                    { "moderator_id", context.Member.Id.ToString(CultureInfo.InvariantCulture) },
                    { "moderator_displayname", context.Member.DisplayName },
                    { "channels_affected", channelsAffected.Humanize() },
                    { "channel_emoji", emoji }
                };
                await ModLog(context.Guild, keyValuePairs, CustomEvent.AutoReactionDelete);

                await context.EditResponseAsync(new()
                {
                    Content = "Channel" + (channelsAffected.Count != 1 ? "s" : "") + $" {channelsAffected.Humanize()} had the autoreaction {emoji} removed."
                });
            }
        }
    }
}
