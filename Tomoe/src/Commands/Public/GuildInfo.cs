using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Humanizer;

namespace Tomoe.Commands.Common
{
    public sealed class GuildInfoCommand : ApplicationCommandModule
    {
        internal static readonly Dictionary<ulong, int> TotalMemberCount = new();

        [SlashCommand("guild_info", "Gets general info about the server.")]
        public static async Task GuildInfoAsync(InteractionContext context)
        {
            DiscordEmbedBuilder embedBuilder = new()
            {
                Title = context.Guild.Name,
                Color = new DiscordColor("#7b84d1"),
                Author = new()
                {
                    Name = context.Member?.DisplayName ?? context.User.Username,
                    IconUrl = context.User.AvatarUrl,
                    Url = context.User.AvatarUrl
                },
                Footer = new() { IconUrl = context.Guild.BannerUrl }
            };

            string features = string.Join(", ", context.Guild.Features.Select(feature => feature.ToLowerInvariant().Titleize()));
            embedBuilder.AddField("Owner", context.Guild.Owner.Mention, true);
            embedBuilder.AddField("Created At", $"{Formatter.Timestamp(context.Guild.CreationTimestamp.UtcDateTime, TimestampFormat.LongDateTime)}, {Formatter.Timestamp(context.Guild.CreationTimestamp.UtcDateTime, TimestampFormat.RelativeTime)}", false);
            embedBuilder.AddField("Currently Scheduled Events", context.Guild.ScheduledEvents.Count.ToMetric(), true);
            embedBuilder.AddField("Emoji Count", context.Guild.Emojis.Count.ToMetric(), true);
            embedBuilder.AddField("Role Count", context.Guild.Roles.Count.ToMetric(), true);
            embedBuilder.AddField("Sticker Count", context.Guild.Stickers.Count.ToMetric(), true);
            embedBuilder.AddField("Features", string.IsNullOrWhiteSpace(features) ? "None" : features, false);

            int textChannelCount = 0;
            int voiceChannelCount = 0;
            int newsChannelCount = 0;
            int stageChannelCount = 0;
            int categoryChannelCount = 0;
            int activeThreadCount = 0;

            foreach (DiscordChannel channel in context.Guild.Channels.Values)
            {
                if (!channel.PermissionsFor(context.Member).HasPermission(Permissions.AccessChannels))
                {
                    continue;
                }

                switch (channel.Type)
                {
                    case ChannelType.Text:
                        textChannelCount++;
                        break;
                    case ChannelType.Voice:
                        voiceChannelCount++;
                        break;
                    case ChannelType.News:
                        newsChannelCount++;
                        break;
                    case ChannelType.Stage:
                        stageChannelCount++;
                        break;
                    case ChannelType.Category:
                        categoryChannelCount++;
                        break;
                }
            }

            // There's a HasMore property which indicates pagination but D#+ doesn't seem to support that... This probably means there's a bug somewhere.
            foreach (DiscordThreadChannel thread in (await context.Guild.ListActiveThreadsAsync()).Threads)
            {
                if (thread.Users.Contains(context.Member))
                {
                    activeThreadCount++;
                }
            }


            embedBuilder.AddField("Channel Stats", @$"Text: {textChannelCount.ToMetric()}
Voice: {voiceChannelCount.ToMetric()}
News/Announcement: {newsChannelCount.ToMetric()}
Stage: {stageChannelCount.ToMetric()}
Categories: {categoryChannelCount.ToMetric()}
Active Threads: {activeThreadCount.ToMetric()}
Total: {(textChannelCount + voiceChannelCount + newsChannelCount + stageChannelCount + categoryChannelCount + activeThreadCount).ToMetric()} ");

            if (context.Guild.IconUrl != null)
            {
                embedBuilder.Url = context.Guild.IconUrl;
                embedBuilder.Thumbnail = new()
                {
                    Url = context.Guild.GetIconUrl(ImageFormat.Png, 4096)
                };
            }

            await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embedBuilder));
        }
    }
}
