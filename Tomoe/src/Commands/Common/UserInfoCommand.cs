using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using OoLunar.DSharpPlus.CommandAll.Attributes;
using OoLunar.DSharpPlus.CommandAll.Commands;

namespace OoLunar.Tomoe.Commands.Common
{
    public sealed class UserInfoCommand : BaseCommand
    {
        [Command("user_info")]
        public static async Task ExecuteAsync(CommandContext context, DiscordUser? user = null)
        {
            user ??= context.User;

            DiscordEmbedBuilder embedBuilder = new()
            {
                Title = $"Info about {user.Username}",
                Thumbnail = new() { Url = user.AvatarUrl },
                Color = new DiscordColor("#6b73db")
            };
            embedBuilder.AddField("Mention", user.Mention, true);
            embedBuilder.AddField("User Id", Formatter.InlineCode(user.Id.ToString(CultureInfo.InvariantCulture)), true);


            if (user is DiscordMember member)
            {
                if (!member.Color.Equals(default(DiscordColor)))
                {
                    embedBuilder.Color = member.Color;
                }

                // ZWS field for spacing
                embedBuilder.AddField("\u200B", "\u200B", true);
                embedBuilder.AddField("Joined Discord", Formatter.Timestamp(user.CreationTimestamp, TimestampFormat.RelativeTime), true);
                embedBuilder.AddField("Joined the Server", Formatter.Timestamp(member.JoinedAt, TimestampFormat.RelativeTime), true);
                embedBuilder.AddField("Roles", member.Roles.Any() ? string.Join('\n', member.Roles.OrderByDescending(role => role.Position).Select(role => $"- {role.Mention}")) : "None", false);
            }
            else
            {
                // No ZWS field
                embedBuilder.AddField("Joined Discord", Formatter.Timestamp(user.CreationTimestamp, TimestampFormat.RelativeTime), true);
            }

            await context.ReplyAsync(embedBuilder);
        }
    }
}
