using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandAll.Attributes;
using DSharpPlus.CommandAll.Attributes;
using DSharpPlus.CommandAll.Commands;
using DSharpPlus.CommandAll.Commands;
using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;
using OoLunar.Tomoe.Database;
using OoLunar.Tomoe.Database.Models;

namespace OoLunar.Tomoe.Commands.Common
{
    public sealed class UserInfoCommand : BaseCommand
    {
        private readonly DatabaseContext _databaseContext;

        public UserInfoCommand(DatabaseContext databaseContext) => _databaseContext = databaseContext ?? throw new ArgumentNullException(nameof(databaseContext));

        [Command("user_info", "ui")]
        public async Task ExecuteAsync(CommandContext context, DiscordUser? user = null)
        {
            user ??= context.User;

            DiscordEmbedBuilder embedBuilder = new()
            {
                Title = $"Info about {user.Username}",
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


                GuildMemberModel memberModel = await _databaseContext.Members.FirstAsync(databaseMember => member.Id == databaseMember.UserId && member.Guild.Id == databaseMember.GuildId);
                if (memberModel.JoinedAt != member.JoinedAt.UtcDateTime)
                {
                    embedBuilder.AddField("User Id", Formatter.InlineCode(user.Id.ToString(CultureInfo.InvariantCulture)), false);
                    embedBuilder.AddField("Joined Discord", Formatter.Timestamp(user.CreationTimestamp, TimestampFormat.RelativeTime), true);
                    embedBuilder.AddField("First joined the Server", Formatter.Timestamp(memberModel.JoinedAt, TimestampFormat.RelativeTime), true);
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

            await context.ReplyAsync(embedBuilder);
        }
    }
}
