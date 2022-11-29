using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Humanizer;

namespace Tomoe.Commands.Common
{
	public class UserInfo : BaseCommandModule
	{
		[Command("user_info"), Description("Get information about a user.")]
		public Task UserInfoAsync(CommandContext context, [Description("The user to get information about.")] DiscordMember member)
		{
			DiscordEmbedBuilder embedBuilder = new()
			{
				Author = new()
				{
					Name = context.Member?.DisplayName ?? context.User.Username,
					IconUrl = context.User.AvatarUrl,
					Url = context.User.AvatarUrl
				},
				Color = new DiscordColor("#7b84d1"),
				Description = $"General information about {member.Username}#{member.Discriminator} ({member.Id})",
				Thumbnail = new()
				{
					Url = member.AvatarUrl
				},
			};
			embedBuilder.AddField("Created At", Formatter.Timestamp(member.CreationTimestamp.UtcDateTime, TimestampFormat.LongDateTime), true);
			embedBuilder.AddField("Joined At", Formatter.Timestamp(member.JoinedAt.UtcDateTime, TimestampFormat.LongDateTime), true);

			StringBuilder stringBuilder = new(2000);
			stringBuilder.AppendLine(Formatter.MaskedUrl("Avatar", new(member.AvatarUrl), "User's avatar."));
			stringBuilder.AppendLine(Formatter.MaskedUrl("Avatar (Large)", new(member.GetAvatarUrl(ImageFormat.Png, 4096)), "User's avatar (Large)."));
			stringBuilder.AppendLine(Formatter.MaskedUrl("Default Avatar", new(member.DefaultAvatarUrl.Replace(".png?size=1024", ".png?size=4096")), "User's default avatar."));
			embedBuilder.AddField("Urls", stringBuilder.ToString(), true);

			if (member.Flags != null)
			{
				embedBuilder.AddField("Flags", member.Flags.Value.Humanize());
			}

			return context.RespondAsync(embedBuilder.Build());
		}
	}
}
