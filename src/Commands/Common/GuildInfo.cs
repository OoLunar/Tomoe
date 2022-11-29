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
		public Task GuildInfoAsync(CommandContext context)
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
			embedBuilder.AddField("Member Count", Program.MemberCounts[context.Guild.Id].ToMetric(), true);
			embedBuilder.AddField("Role Count", context.Guild.Roles.Count.ToMetric(), true);
			embedBuilder.AddField("Sticker Count", context.Guild.Stickers.Count.ToMetric(), true);
			embedBuilder.AddField("Your Channel Count", context.Guild.Channels.Where((channel, _) => !channel.Value.IsCategory && channel.Value.PermissionsFor(context.Member).HasPermission(Permissions.AccessChannels)).Count().ToMetric(), true);
			embedBuilder.AddField("Features", string.IsNullOrWhiteSpace(features) ? "None" : features, false);

			if (context.Guild.IconUrl != null)
			{
				embedBuilder.Url = context.Guild.IconUrl;
				embedBuilder.Thumbnail = new()
				{
					Url = context.Guild.GetIconUrl(ImageFormat.Png, 4096)
				};
			}

			return context.RespondAsync(embedBuilder.Build());
		}
	}
}
