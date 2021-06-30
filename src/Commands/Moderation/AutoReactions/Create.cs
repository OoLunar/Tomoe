namespace Tomoe.Commands
{
    using DSharpPlus;
    using DSharpPlus.Entities;
    using DSharpPlus.SlashCommands;
    using Humanizer;
    using Microsoft.Extensions.DependencyInjection;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using Tomoe.Commands.Attributes;
    using Tomoe.Db;

    [SlashCommandGroup("temp", "temp")]
    public partial class Moderation : SlashCommandModule
    {
        [SlashCommandGroup("autoreact", "Adds a new reaction on every message sent in a specified guild channel.")]
        public partial class AutoReactions : SlashCommandModule
        {
            private static Regex EmojiRegex { get; } = new("^<(?<animated>a)?:(?<name>[a-zA-Z0-9_]+?):(?<id>\\d+?)>$", RegexOptions.Compiled | RegexOptions.ECMAScript);
            public Database Database { private get; set; }

            [SlashCommand("create", "Creates a new autoreaction on a channel."), Hierarchy(Permissions.ManageChannels | Permissions.ManageMessages)]
            public async Task Create(InteractionContext context, [Option("channel", "Which guild channel to autoreact too.")] DiscordChannel channel, [Option("emoji", "Which emoji to react with.")] string emojiString)
            {

                if (!DiscordEmoji.TryFromUnicode(context.Client, emojiString, out DiscordEmoji emoji))
                {
                    Match match = EmojiRegex.Match(emojiString);
                    string emojiIdString = match.Groups["id"].Value;
                    if (!ulong.TryParse(emojiIdString, NumberStyles.Integer, CultureInfo.InvariantCulture, out ulong emojiId))
                    {
                        await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new()
                        {
                            IsEphemeral = true,
                            Content = $"Error: {emojiString} is not a valid emoji!"
                        });
                        return;
                    }

                    if (!DiscordEmoji.TryFromGuildEmote(context.Client, emojiId, out emoji))
                    {
                        await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new()
                        {
                            IsEphemeral = true,
                            Content = $"Error: {emojiString} is not a valid emoji!"
                        });
                        return;
                    }
                }

#pragma warning disable CS8794
                if (channel.Type is not ChannelType.Text or not ChannelType.News or not ChannelType.Category)
#pragma warning restore CS8794
                {
                    await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new()
                    {
                        IsEphemeral = true,
                        Content = $"Error: {channel.Mention} is not a text or category channel!"
                    });
                    return;
                }

                await context.CreateResponseAsync(InteractionResponseType.DeferredChannelMessageWithSource, new());

                using IServiceScope scope = Program.ServiceProvider.CreateScope();
                Database database = scope.ServiceProvider.GetService<Database>();

                List<string> channelsAffected = new();

                if (channel.Type is ChannelType.Text or ChannelType.News)
                {
                    AutoReaction autoReaction = database.AutoReactions.FirstOrDefault(databaseAutoReaction => databaseAutoReaction.GuildId == context.Guild.Id && databaseAutoReaction.ChannelId == channel.Id && databaseAutoReaction.EmojiName == emoji.ToString());
                    if (autoReaction == null)
                    {
                        autoReaction = new()
                        {
                            GuildId = context.Guild.Id,
                            ChannelId = channel.Id,
                            EmojiName = emoji.ToString()
                        };
                        database.AutoReactions.Add(autoReaction);
                        await database.SaveChangesAsync();
                        channelsAffected.Add(channel.Mention);
                    }
                }
                else
                {
                    foreach (DiscordChannel subChannel in channel.Children)
                    {
                        AutoReaction autoReaction = database.AutoReactions.FirstOrDefault(databaseAutoReaction => databaseAutoReaction.GuildId == context.Guild.Id && databaseAutoReaction.ChannelId == subChannel.Id && databaseAutoReaction.EmojiName == emoji.ToString());
                        if (autoReaction == null)
                        {
                            autoReaction = new()
                            {
                                GuildId = context.Guild.Id,
                                ChannelId = subChannel.Id,
                                EmojiName = emoji.ToString()
                            };
                            database.AutoReactions.Add(autoReaction);
                            channelsAffected.Add(subChannel.Mention);
                        }
                    }
                    if (channelsAffected.Count != 0)
                    {
                        await database.SaveChangesAsync();
                    }
                }

                Dictionary<string, string> keyValuePairs = new();
                keyValuePairs.Add("guild_name", context.Guild.Name);
                keyValuePairs.Add("guild_count", Public.TotalMemberCount[context.Guild.Id].ToMetric());
                keyValuePairs.Add("moderator_username", context.Member.Username);
                keyValuePairs.Add("moderator_tag", context.Member.Discriminator);
                keyValuePairs.Add("moderator_mention", context.Member.Mention);
                keyValuePairs.Add("moderator_id", context.Member.Id.ToString(CultureInfo.InvariantCulture));
                keyValuePairs.Add("moderator_displayname", context.Member.DisplayName);
                keyValuePairs.Add("channels_affected", channelsAffected.Humanize());
                keyValuePairs.Add("channel_emoji", emoji);
                await ModLog(context.Guild, keyValuePairs, CustomEvent.AutoReactionCreate);

                await context.EditResponseAsync(new()
                {
                    Content = "Channel" + (channelsAffected.Count != 1 ? "s" : "") + $" {channelsAffected.Humanize()} will now have the emoji {emoji} reacted on every new message."
                });
            }
        }
    }
}