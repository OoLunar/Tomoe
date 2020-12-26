using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

namespace Tomoe.Commands.Public
{
	public class ServerInfo : BaseCommandModule
	{
		[Command("serverinfo"), Description("Gets general info about the server."), Aliases("server_info")]
		public async Task Get(CommandContext context)
		{
			DiscordEmbedBuilder embedBuilder = new();
			embedBuilder.Title = context.Guild.Name;
			embedBuilder.Url = context.Guild.IconUrl;
			embedBuilder.Footer = new()
			{
				IconUrl = context.Guild.BannerUrl
			};
			string guildInfo = string.Empty;
			guildInfo += $"AFK Channel: {context.Guild.AfkChannel?.Mention ?? "Not set"}\n";
			guildInfo += $"AFK Timeout: {TimeSpan.FromSeconds(context.Guild.AfkTimeout).TotalMinutes} minutes\n";
			guildInfo += $"Channel Count: {context.Guild.Channels.Where((channel, _) => !channel.Value.IsCategory).ToArray().Length}\n";
			guildInfo += $"Created At: {context.Guild.CreationTimestamp}\n";
			guildInfo += $"Description: {context.Guild.Description ?? "Not set"}\n";
			guildInfo += $"Emoji Count: {context.Guild.Emojis.Count}\n";
			guildInfo += $"Explicit Content Filter: {context.Guild.ExplicitContentFilter}\n";
			guildInfo += $"Features: {string.Join(", ", context.Guild.Features) ?? "None"}\n";
			guildInfo += $"Icon url: {context.Guild.IconUrl ?? "No icon set"}\n";
			guildInfo += $"Id: {context.Guild.Id}\n";
			guildInfo += $"Is Large: {context.Guild.IsLarge}\n";
			guildInfo += $"Max Members: {context.Guild.MaxMembers}\n";
			guildInfo += $"Member Count: {context.Guild.MemberCount}\n";
			guildInfo += $"MFA Level: {context.Guild.MfaLevel}\n";
			guildInfo += $"Name: {context.Guild.Name}\n";
			guildInfo += $"Owner: {context.Guild.Owner.Mention}\n";
			guildInfo += $"Preferred Locale: {context.Guild.PreferredLocale}\n";
			guildInfo += $"Premium Subscription Count: {context.Guild.PremiumSubscriptionCount}\n";
			guildInfo += $"Premium Tier: {context.Guild.PremiumTier}\n";
			guildInfo += $"Public Updates Channel: {context.Guild.PublicUpdatesChannel?.Mention ?? "Not set"}\n";
			guildInfo += $"Role Count: {context.Guild.Roles.Count}\n";
			guildInfo += $"Rules Channel: {context.Guild.RulesChannel?.Mention ?? "Not set"}\n";
			guildInfo += $"Splash Url: {context.Guild.SplashUrl ?? "Not set"}\n";
			guildInfo += $"Vanity Url: {context.Guild.VanityUrlCode ?? "Not set"}\n";
			guildInfo += $"Verification Level: {context.Guild.VerificationLevel}\n";
			guildInfo += $"Voice Region: {context.Guild.VoiceRegion.Name}\n";

			embedBuilder.Description = guildInfo;
			_ = Program.SendMessage(context, embedBuilder.Build());
		}
	}
}
