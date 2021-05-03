using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Humanizer;

namespace Tomoe.Commands.Public
{
	public class ServerInfo : BaseCommandModule
	{
		[Command("serverinfo"), Description("Gets general info about the server."), Aliases("server_info", "guildinfo", "guild_info")]
		public async Task Overload(CommandContext context)
		{
			DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder().GenerateDefaultEmbed(context, null);
			embedBuilder.Title = context.Guild.Name;
			embedBuilder.Url = context.Guild.IconUrl;
			embedBuilder.Footer = new() { IconUrl = context.Guild.BannerUrl };
			_ = embedBuilder.AddField($"AFK Channel", context.Guild.AfkChannel?.Mention ?? "Not set", true);
			_ = embedBuilder.AddField($"AFK Timeout", TimeSpan.FromSeconds(context.Guild.AfkTimeout).TotalMinutes + " minutes", true);
			_ = embedBuilder.AddField($"Channel Count", context.Guild.Channels.Where((channel, _) => !channel.Value.IsCategory).Count().ToMetric(), true);
			_ = embedBuilder.AddField($"Created At", context.Guild.CreationTimestamp.UtcDateTime.ToOrdinalWords(), true);
			_ = embedBuilder.AddField($"Description", context.Guild.Description ?? "Not set", true);
			_ = embedBuilder.AddField($"Emoji Count", context.Guild.Emojis.Count.ToMetric(), true);
			string features = string.Join(", ", context.Guild.Features.Select(feature => feature.ToLowerInvariant().Titleize()));
			_ = embedBuilder.AddField($"Features", string.IsNullOrWhiteSpace(features) ? "None" : features);
			_ = embedBuilder.AddField($"Explicit Content Filter", context.Guild.ExplicitContentFilter.ToString(), true);
			_ = embedBuilder.AddField($"Icon url", context.Guild.IconUrl == null ? "No icon set!" : Formatter.MaskedUrl("Link to image", new(context.Guild.IconUrl.Replace(".jpg", ".png?size=1024"))), true);
			_ = embedBuilder.AddField($"Id", context.Guild.Id.ToString(), true);
			_ = embedBuilder.AddField($"Is Large", context.Guild.IsLarge.ToString(), true);
			_ = embedBuilder.AddField($"Max Members", context.Guild.MaxMembers.HasValue ? context.Guild.MaxMembers.Value.ToMetric() : "Unknown", true);
			_ = embedBuilder.AddField($"Member Count", context.Guild.MemberCount.ToMetric(), true);
			_ = embedBuilder.AddField($"MFA Level", context.Guild.MfaLevel.ToString(), true);
			_ = embedBuilder.AddField($"Name", context.Guild.Name, true);
			_ = embedBuilder.AddField($"Owner", context.Guild.Owner.Mention, true);
			_ = embedBuilder.AddField($"Preferred Locale", context.Guild.PreferredLocale, true);
			_ = embedBuilder.AddField($"Premium Subscription Count", context.Guild.PremiumSubscriptionCount.HasValue ? context.Guild.PremiumSubscriptionCount.Value.ToMetric() : "None", true);
			_ = embedBuilder.AddField($"Premium Tier", context.Guild.PremiumTier.Humanize(), true);
			_ = embedBuilder.AddField($"Role Count", context.Guild.Roles.Count.ToMetric(), true);
			_ = embedBuilder.AddField($"Rules Channel", context.Guild.RulesChannel?.Mention ?? "Not set", true);
			_ = embedBuilder.AddField($"Splash Url", Formatter.MaskedUrl("Link to image", new(context.Guild.SplashUrl.Replace(".jpg", ".png?size=1024"))) ?? "Not set", true);
			_ = embedBuilder.AddField($"Vanity Url", context.Guild.VanityUrlCode ?? "Not set", true);
			_ = embedBuilder.AddField($"Verification Level", context.Guild.VerificationLevel.ToString(), true);
			_ = embedBuilder.AddField($"Voice Region", context.Guild.VoiceRegion.Name, true);
			embedBuilder.Thumbnail = new()
			{
				Url = context.Guild.IconUrl.Replace(".jpg", ".png?&size=1024")
			};

			_ = await Program.SendMessage(context, null, embedBuilder.Build());
		}
	}
}
