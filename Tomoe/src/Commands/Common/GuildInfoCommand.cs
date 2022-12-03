using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using Humanizer;
using OoLunar.DSharpPlus.CommandAll.Attributes;
using OoLunar.DSharpPlus.CommandAll.Commands;

namespace OoLunar.Tomoe.Commands.Common
{
    public sealed class GuildInfoCommand : BaseCommand
    {
        [Command("guild_info")]
        public static Task ExecuteAsync(CommandContext context)
        {
            if (context.Guild is null)
            {
                return context.ReplyAsync($"Command `/{context.CurrentCommand.FullName}` can only be used in a guild.");
            }

            DiscordEmbedBuilder embedBuilder = new()
            {
                Title = context.Guild.Name,
                Color = new DiscordColor("#6b73db"),
                Author = new()
                {
                    Name = context.Member?.DisplayName ?? context.User.Username,
                    IconUrl = context.User.AvatarUrl,
                    Url = context.User.AvatarUrl
                },
                Footer = new() { IconUrl = context.Guild.BannerUrl }
            };

            if (context.Guild.IconUrl is not null)
            {
                embedBuilder.Thumbnail = new() { Url = context.Guild.GetIconUrl(ImageFormat.Auto, 4096) };
            }

            string features = string.Join(", ", context.Guild.Features.Select(feature => feature.ToLowerInvariant().Titleize()));
            embedBuilder.AddField("Owner", context.Guild.Owner.Mention, true);
            embedBuilder.AddField("Created At", Formatter.Timestamp(context.Guild.CreationTimestamp.UtcDateTime, TimestampFormat.RelativeTime), true);
            embedBuilder.AddField("\u200B", "\u200B", true); // ZWS field
            embedBuilder.AddField("Emoji Count", context.Guild.Emojis.Count.ToString("N0", CultureInfo.InvariantCulture), true);
            embedBuilder.AddField("Role Count", context.Guild.Roles.Count.ToString("N0", CultureInfo.InvariantCulture), true);
            embedBuilder.AddField("Sticker Count", context.Guild.Stickers.Count.ToString("N0", CultureInfo.InvariantCulture), true);
            embedBuilder.AddField("Currently Scheduled Events", context.Guild.ScheduledEvents.Count.ToString("N0", CultureInfo.InvariantCulture), true);
            embedBuilder.AddField("Features", string.IsNullOrWhiteSpace(features) ? "None" : features, false);

            return context.ReplyAsync(embedBuilder);
        }
    }
}
