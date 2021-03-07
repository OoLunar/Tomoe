using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

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
			StringBuilder guildInfo = new();
			_ = guildInfo.Append($"AFK Channel: {context.Guild.AfkChannel?.Mention ?? "Not set"}\n");
			_ = guildInfo.Append($"AFK Timeout: {TimeSpan.FromSeconds(context.Guild.AfkTimeout).TotalMinutes} minutes\n");
			_ = guildInfo.Append($"Channel Count: {context.Guild.Channels.Where((channel, _) => !channel.Value.IsCategory).ToArray().Length}\n");
			_ = guildInfo.Append($"Created At: {context.Guild.CreationTimestamp}\n");
			_ = guildInfo.Append($"Description: {context.Guild.Description ?? "Not set"}\n");
			_ = guildInfo.Append($"Emoji Count: {context.Guild.Emojis.Count}\n");
			_ = guildInfo.Append($"Explicit Content Filter: {context.Guild.ExplicitContentFilter}\n");
			string features = string.Join(", ", context.Guild.Features);
			_ = guildInfo.Append($"Features: {(string.IsNullOrWhiteSpace(features) ? "None" : features)}\n");
			_ = guildInfo.Append($"Icon url: {(context.Guild.IconUrl == null ? "No icon set!" : $"[Link to image]({context.Guild.IconUrl.Replace(".jpg", ".png?size=1024")})")}\n");
			_ = guildInfo.Append($"Id: {context.Guild.Id}\n");
			_ = guildInfo.Append($"Is Large: {context.Guild.IsLarge}\n");
			_ = guildInfo.Append($"Max Members: {context.Guild.MaxMembers}\n");
			_ = guildInfo.Append($"Member Count: {context.Guild.MemberCount}\n");
			_ = guildInfo.Append($"MFA Level: {context.Guild.MfaLevel}\n");
			_ = guildInfo.Append($"Name: {context.Guild.Name}\n");
			_ = guildInfo.Append($"Owner: {context.Guild.Owner.Mention}\n");
			_ = guildInfo.Append($"Preferred Locale: {context.Guild.PreferredLocale}\n");
			_ = guildInfo.Append($"Premium Subscription Count: {context.Guild.PremiumSubscriptionCount}\n");
			_ = guildInfo.Append($"Premium Tier: {context.Guild.PremiumTier}\n");
			_ = guildInfo.Append($"Role Count: {context.Guild.Roles.Count}\n");
			_ = guildInfo.Append($"Rules Channel: {context.Guild.RulesChannel?.Mention ?? "Not set"}\n");
			_ = guildInfo.Append($"Splash Url: {context.Guild.SplashUrl ?? "Not set"}\n");
			_ = guildInfo.Append($"Vanity Url: {context.Guild.VanityUrlCode ?? "Not set"}\n");
			_ = guildInfo.Append($"Verification Level: {context.Guild.VerificationLevel}\n");
			_ = guildInfo.Append($"Voice Region: {context.Guild.VoiceRegion.Name}");
			embedBuilder.Description = guildInfo.ToString();
			embedBuilder.Thumbnail = new()
			{
				Url = context.Guild.IconUrl.Replace(".jpg", ".png?&size=1024")
			};

			_ = await Program.SendMessage(context, null, embedBuilder.Build());
		}
	}
}
