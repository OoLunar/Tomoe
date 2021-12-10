namespace Tomoe.Commands
{
    using DSharpPlus;
    using DSharpPlus.Entities;
    using DSharpPlus.SlashCommands;
    using Humanizer;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public partial class Public : ApplicationCommandModule
    {
        private const string NotSet = "Not set.";
        private const string LinkToImage = "Link to image.";

        internal static readonly Dictionary<ulong, int> TotalMemberCount = new();

        [SlashCommand("guild_info", "Gets general info about the server.")]
        public static async Task GuildInfo(InteractionContext context)
        {
            string features = string.Join(", ", context.Guild.Features.Select(feature => feature.ToLowerInvariant().Titleize()));
            string bannerUrl = context.Guild.BannerUrl?.Replace(".jpg", ".png?size=1024") ?? NotSet;
            string iconUrl = context.Guild.IconUrl?.Replace(".jpg", ".png?size=1024") ?? NotSet;
            string splashUrl = context.Guild.SplashUrl?.Replace(".jpg", ".png?size=1024") ?? NotSet;
            DiscordEmbedBuilder embedBuilder = new()
            {
                Title = context.Guild.Name + " Guild Information",
                Color = new DiscordColor("#7b84d1")
            };
            if (context.Guild.IconUrl != null)
            {
                embedBuilder.WithThumbnail(iconUrl);
            }

            embedBuilder.AddField("AFK Channel", context.Guild.AfkChannel == null ? NotSet : context.Guild.AfkChannel.Mention, true);
            embedBuilder.AddField("AFK Timeout", TimeSpan.FromSeconds(context.Guild.AfkTimeout).Humanize(), true);
            embedBuilder.AddField("Banner Url", context.Guild.BannerUrl == null ? NotSet : Formatter.MaskedUrl(LinkToImage, new(bannerUrl), bannerUrl), true);
            embedBuilder.AddField("Channel Count", context.Guild.Channels.Values.Count(channel => !channel.IsCategory).ToMetric(), true);
            embedBuilder.AddField("Created At", context.Guild.CreationTimestamp.UtcDateTime.ToOrdinalWords(), true);
            embedBuilder.AddField("Description", string.IsNullOrEmpty(context.Guild.Description) ? NotSet : context.Guild.Description, true);
            embedBuilder.AddField("Emoji Count", context.Guild.Emojis.Count.ToMetric(), true);
            embedBuilder.AddField("Explicit Content Filter", context.Guild.ExplicitContentFilter.Humanize(), true);
            embedBuilder.AddField("Icon url", iconUrl == null ? NotSet : Formatter.MaskedUrl(LinkToImage, new(iconUrl), iconUrl), true);
            embedBuilder.AddField("Features", string.IsNullOrEmpty(features) ? "None." : features);
            embedBuilder.AddField("Id", $"`{context.Guild.Id}`", true);
            embedBuilder.AddField("Max Members", context.Guild.MaxMembers.HasValue ? context.Guild.MaxMembers.Value.ToMetric() : "Unknown.", true);
            embedBuilder.AddField("Member Count", TotalMemberCount[context.Guild.Id].ToMetric(), true);
            embedBuilder.AddField("MFA Level", context.Guild.MfaLevel.Humanize(), true);
            embedBuilder.AddField("Name", context.Guild.Name, true);
            embedBuilder.AddField("Owner", context.Guild.Owner.Mention, true);
            embedBuilder.AddField("Preferred Locale", context.Guild.PreferredLocale, true);
            embedBuilder.AddField("Role Count", context.Guild.Roles.Count.ToMetric(), true);
            embedBuilder.AddField("Rules Channel", context.Guild.RulesChannel == null ? NotSet : context.Guild.RulesChannel.Mention, true);
            embedBuilder.AddField("Server Boosts", context.Guild.PremiumSubscriptionCount.HasValue ? context.Guild.PremiumSubscriptionCount.Value.ToMetric() : "0", true);
            embedBuilder.AddField("Server Tier", context.Guild.PremiumTier.Humanize(), true);
            embedBuilder.AddField("Splash Url", context.Guild.SplashUrl == null ? NotSet : Formatter.MaskedUrl(LinkToImage, new(splashUrl), splashUrl), true);
            embedBuilder.AddField("Vanity Url", string.IsNullOrEmpty(context.Guild.VanityUrlCode) ? NotSet : $"discord.gg/{context.Guild.VanityUrlCode}", true);
            embedBuilder.AddField("Verification Level", context.Guild.VerificationLevel.Humanize(), true);
            embedBuilder.AddField("Voice Region", context.Guild.VoiceRegion.Name, true);

            await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embedBuilder));
        }
    }
}
