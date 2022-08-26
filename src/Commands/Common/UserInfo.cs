using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Humanizer;

namespace OoLunar.Tomoe.Commands.Common
{
    public sealed class UserInfo : BaseCommandModule
    {
        [Command("user_info"), Description("Get information about a user.")]
        public Task UserInfoAsync(CommandContext context, [Description("The user to get information about.")] DiscordUser? user = null)
        {
            user ??= context.Member ?? context.User;

            DiscordEmbedBuilder embedBuilder = new()
            {
                Author = new()
                {
                    Name = context.Member?.DisplayName ?? context.User.Username,
                    IconUrl = context.User.AvatarUrl,
                    Url = context.User.AvatarUrl
                },
                Color = new DiscordColor("#7b84d1"),
                Description = $"General information about {user.Username}#{user.Discriminator} ({user.Id})",
                Thumbnail = new()
                {
                    Url = user.AvatarUrl
                },
            };

            if (user.Flags != null)
            {
                embedBuilder.AddField("Flags", user.Flags.Value.Humanize(), true);
            }

            embedBuilder.AddField("Created At", Formatter.Timestamp(user.CreationTimestamp.UtcDateTime, TimestampFormat.LongDateTime), true);
            if (user is DiscordMember member)
            {
                embedBuilder.AddField("Joined At", Formatter.Timestamp(member.JoinedAt.UtcDateTime, TimestampFormat.LongDateTime), true);
                embedBuilder.AddField("Roles", !member.Roles.Any() ? "None" : string.Join(", ", member.Roles.OrderByDescending(role => role.Position).Select(role => role.Mention)), false);
            }

            StringBuilder stringBuilder = new(2000);
            stringBuilder.AppendLine(Formatter.MaskedUrl("Avatar", new(user.AvatarUrl), "User's avatar."));
            stringBuilder.AppendLine(Formatter.MaskedUrl("Avatar (Large)", new(user.GetAvatarUrl(ImageFormat.Png, 4096)), "User's avatar (Large)."));
            stringBuilder.AppendLine(Formatter.MaskedUrl("Default Avatar", new(user.DefaultAvatarUrl.Replace(".png?size=1024", ".png?size=4096")), "User's default avatar."));
            embedBuilder.AddField("Urls", stringBuilder.ToString(), true);

            return context.RespondAsync(embedBuilder.Build());
        }
    }
}
