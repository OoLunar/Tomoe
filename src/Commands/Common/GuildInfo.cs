using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Humanizer;

namespace Tomoe.Commands.Common
{
    public class GuildInfo : BaseCommandModule
    {
        [Command("guild_info"), Description("Gets general info about the guild."), Aliases("guild_stats", "server_info", "server_stats")]
        public async Task GuildInfoAsync(CommandContext context)
        {
            DiscordEmbedBuilder embedBuilder = new();
            embedBuilder.Color = new DiscordColor("#7b84d1");
            embedBuilder.Author = new()
            {
                Name = context.Member.DisplayName,
                IconUrl = context.User.AvatarUrl,
                Url = context.User.AvatarUrl
            };

            embedBuilder.Title = context.Guild.Name;
            embedBuilder.Footer = new() { IconUrl = context.Guild.BannerUrl };
            embedBuilder.AddField($"AFK Channel", context.Guild.AfkChannel?.Mention ?? "Not set", true);
            embedBuilder.AddField($"AFK Timeout", TimeSpan.FromSeconds(context.Guild.AfkTimeout).TotalMinutes + " minutes", true);
            embedBuilder.AddField($"Channel Count", context.Guild.Channels.Where((channel, _) => !channel.Value.IsCategory).Count().ToMetric(), true);
            embedBuilder.AddField($"Created At", context.Guild.CreationTimestamp.UtcDateTime.ToOrdinalWords(), true);
            embedBuilder.AddField($"Description", context.Guild.Description ?? "Not set", true);
            embedBuilder.AddField($"Emoji Count", context.Guild.Emojis.Count.ToMetric(), true);
            string features = string.Join(", ", context.Guild.Features.Select(feature => feature.ToLowerInvariant().Titleize()));
            embedBuilder.AddField($"Features", string.IsNullOrWhiteSpace(features) ? "None" : features);
            embedBuilder.AddField($"Explicit Content Filter", context.Guild.ExplicitContentFilter.ToString(), true);
            embedBuilder.AddField($"Icon url", context.Guild.IconUrl == null ? "No icon set!" : Formatter.MaskedUrl("Link to image", new(context.Guild.IconUrl.Replace(".jpg", ".png?size=1024"))), true);
            embedBuilder.AddField($"Id", context.Guild.Id.ToString(CultureInfo.InvariantCulture), true);
            embedBuilder.AddField($"Is Large", context.Guild.IsLarge.ToString(), true);
            embedBuilder.AddField($"Max Members", context.Guild.MaxMembers.HasValue ? context.Guild.MaxMembers.Value.ToMetric() : "Unknown", true);
            embedBuilder.AddField($"Member Count", context.Guild.MemberCount.ToMetric(), true);
            embedBuilder.AddField($"MFA Level", context.Guild.MfaLevel.ToString(), true);
            embedBuilder.AddField($"Name", context.Guild.Name, true);
            embedBuilder.AddField($"Owner", context.Guild.Owner.Mention, true);
            embedBuilder.AddField($"Preferred Locale", context.Guild.PreferredLocale, true);
            embedBuilder.AddField($"Premium Subscription Count", context.Guild.PremiumSubscriptionCount.HasValue ? context.Guild.PremiumSubscriptionCount.Value.ToMetric() : "None", true);
            embedBuilder.AddField($"Premium Tier", context.Guild.PremiumTier.Humanize(), true);
            embedBuilder.AddField($"Role Count", context.Guild.Roles.Count.ToMetric(), true);
            embedBuilder.AddField($"Rules Channel", context.Guild.RulesChannel?.Mention ?? "Not set", true);
            embedBuilder.AddField($"Splash Url", context.Guild.SplashUrl == null ? "Not set" : Formatter.MaskedUrl("Link to image", new(context.Guild.SplashUrl.Replace(".jpg", ".png?size=1024"))), true);
            embedBuilder.AddField($"Vanity Url", context.Guild.VanityUrlCode ?? "Not set", true);
            embedBuilder.AddField($"Verification Level", context.Guild.VerificationLevel.ToString(), true);

            if (context.Guild.IconUrl != null)
            {
                embedBuilder.Url = context.Guild.IconUrl;
                embedBuilder.Thumbnail = new()
                {
                    Url = context.Guild.IconUrl.Replace(".jpg", ".png?&size=4096")
                };
            }

            await context.RespondAsync(null, embedBuilder.Build());
        }
    }
}