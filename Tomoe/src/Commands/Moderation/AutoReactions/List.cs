using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Tomoe.Db;

namespace Tomoe.Commands.Moderation
{
    public sealed partial class AutoReactions : ApplicationCommandModule
    {
        [SlashCommand("list", "Shows all autoreactions on a channel.")]
        public Task ListAsync(InteractionContext context, [Option("channel", "Which channel to view the autoreactions on.")] DiscordChannel? channel = null)
        {
            channel ??= context.Channel;

            if (channel.Type != ChannelType.Text && channel.Type != ChannelType.News && channel.Type != ChannelType.Category)
            {
                return context.EditResponseAsync(new()
                {
                    Content = $"Error: {channel.Mention} is not a text or category channel!"
                });
            }

            IEnumerable<AutoReaction> autoReactions = channel != null
                ? Database.AutoReactions.Where(databaseAutoReaction => databaseAutoReaction.GuildId == context.Guild.Id && databaseAutoReaction.ChannelId == channel.Id)
                : Database.AutoReactions.Where(databaseAutoReaction => databaseAutoReaction.GuildId == context.Guild.Id);

            Dictionary<DiscordChannel, List<DiscordEmoji>> channelsAndEmojis = new();
            List<DiscordEmbed> embeds = new();

            foreach (AutoReaction autoReaction in autoReactions)
            {
                DiscordChannel autoReactionChannel = context.Guild.GetChannel(autoReaction.ChannelId);
                DiscordEmoji emoji = DiscordEmoji.FromName(context.Client, autoReaction.EmojiName, true);
                if (!channelsAndEmojis.TryGetValue(autoReactionChannel, out List<DiscordEmoji>? emojis))
                {
                    channelsAndEmojis.Add(autoReactionChannel, new() { emoji });
                }
                else
                {
                    channelsAndEmojis[autoReactionChannel].Add(emoji);
                }
            }

            DiscordEmbedBuilder embed = new()
            {
                Color = new DiscordColor("#7b84d1"),
            };
            embed.WithThumbnail(context.Guild.IconUrl);

            foreach (DiscordChannel embedChannel in channelsAndEmojis.Keys)
            {
                if (embed.Fields.Count == 25)
                {
                    embeds.Add(embed);
                    embed = new()
                    {
                        Color = new DiscordColor("#7b84d1"),
                    };
                    embed.WithThumbnail(context.Guild.IconUrl);
                }

                StringBuilder stringBuilder = new();

                foreach (DiscordEmoji emoji in channelsAndEmojis[embedChannel])
                {
                    stringBuilder.Append(emoji.ToString() + ", ");
                }
                embed.AddField('#' + embedChannel.Name, stringBuilder.ToString());
            }

            if (!embeds.Contains(embed))
            {
                embeds.Add(embed);
            }

            return context.EditResponseAsync(new DiscordWebhookBuilder().AddEmbeds(embeds));
        }
    }
}
