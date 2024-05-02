using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Commands;
using DSharpPlus.Entities;
using OoLunar.Tomoe.Database.Models;

namespace OoLunar.Tomoe.Commands.Common
{
    public sealed partial class InfoCommand
    {
        /// <summary>
        /// Sends information about the provided user.
        /// </summary>
        /// <param name="user">Which user to get information about. Leave empty to get information about yourself.</param>
        [Command("user")]
        public static async Task UserInfoAsync(CommandContext context, DiscordUser? user = null)
        {
            user ??= context.User;
            DiscordEmbedBuilder embedBuilder = new()
            {
                Title = $"Info about {user.GetDisplayName()}",
                Thumbnail = new() { Url = user.AvatarUrl },
                Color = new DiscordColor("#6b73db")
            };

            embedBuilder.AddField("Mention", user.Mention, true);
            if (user is DiscordMember member)
            {
                if (!member.Color.Equals(default(DiscordColor)))
                {
                    embedBuilder.Color = member.Color;
                }

                GuildMemberModel? memberModel = await GuildMemberModel.FindMemberAsync(context.User.Id, context.Guild!.Id);
                if (memberModel is not null && memberModel.FirstJoined != member.JoinedAt.UtcDateTime)
                {
                    embedBuilder.AddField("User Id", Formatter.InlineCode(user.Id.ToString(CultureInfo.InvariantCulture)), false);
                    embedBuilder.AddField("Joined Discord", Formatter.Timestamp(user.CreationTimestamp, TimestampFormat.RelativeTime), true);
                    embedBuilder.AddField("First joined the Server", Formatter.Timestamp(memberModel.FirstJoined, TimestampFormat.RelativeTime), true);
                }
                else
                {
                    embedBuilder.AddField("User Id", Formatter.InlineCode(user.Id.ToString(CultureInfo.InvariantCulture)), true);
                    // ZWS field
                    embedBuilder.AddField("\u200B", "\u200B", true);
                    embedBuilder.AddField("Joined Discord", Formatter.Timestamp(user.CreationTimestamp, TimestampFormat.RelativeTime), true);
                }

                embedBuilder.AddField("Recently joined the Server", Formatter.Timestamp(member.JoinedAt, TimestampFormat.RelativeTime), true);
                embedBuilder.AddField("Roles", member.Roles.Any() ? string.Join('\n', member.Roles.OrderByDescending(role => role.Position).Select(role => $"- {role.Mention}")) : "None", false);
            }
            else
            {
                embedBuilder.AddField("User Id", Formatter.InlineCode(user.Id.ToString(CultureInfo.InvariantCulture)), true);
                embedBuilder.AddField("Joined Discord", Formatter.Timestamp(user.CreationTimestamp, TimestampFormat.RelativeTime), true);
            }

            await context.RespondAsync(embedBuilder);
        }
    }
}
