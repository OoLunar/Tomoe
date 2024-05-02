using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Entities;
using OoLunar.Tomoe.Database.Models;

namespace OoLunar.Tomoe.Commands.Common
{
    public sealed partial class InfoCommand
    {
        /// <summary>
        /// Sends information about the provided role.
        /// </summary>
        /// <param name="role">Which role to get information about.</param>
        [Command("role"), RequireGuild]
        public static async Task RoleInfoAsync(CommandContext context, DiscordRole role)
        {
            DiscordEmbedBuilder embedBuilder = new()
            {
                Title = $"Role Info for {role.Name}",
                Author = new()
                {
                    Name = context.Member!.DisplayName,
                    IconUrl = context.User.AvatarUrl,
                    Url = context.User.AvatarUrl
                },
                Color = role.Color.Value == 0x000000 ? null : role.Color
            };

            embedBuilder.AddField("Color", role.Color.ToString(), true);
            embedBuilder.AddField("Created At", Formatter.Timestamp(role.CreationTimestamp.UtcDateTime, TimestampFormat.LongDateTime), true);
            embedBuilder.AddField("Hoisted", role.IsHoisted.ToString(), true);
            embedBuilder.AddField("Is Managed", role.IsManaged.ToString(), true);
            embedBuilder.AddField("Is Mentionable", role.IsMentionable.ToString(), true);
            embedBuilder.AddField("Role Id", Formatter.InlineCode(role.Id.ToString(CultureInfo.InvariantCulture)), true);
            embedBuilder.AddField("Role Name", role.Name, true);
            embedBuilder.AddField("Role Position", role.Position.ToString("N0", CultureInfo.InvariantCulture), true);
            embedBuilder.AddField("Permissions", role.Permissions == DiscordPermissions.None ? "No permissions." : role.Permissions.ToPermissionString() + ".", false);

            int fieldCharCount = 0;
            List<string> memberMentions = [];
            await foreach (GuildMemberModel member in GuildMemberModel.GetMembersWithRoleAsync(context.Guild!.Id, role.Id))
            {
                string mention = $"<@{member.UserId.ToString(CultureInfo.InvariantCulture)}>";
                fieldCharCount += mention.Length + 2; // + 2 for the command and space
                if (fieldCharCount >= 1024)
                {
                    break;
                }

                memberMentions.Add(mention);
            }

            memberMentions.Sort(string.CompareOrdinal);
            embedBuilder.AddField($"Members ({await GuildMemberModel.CountMembersWithRoleAsync(context.Guild.Id, role.Id):N0})", string.Join(", ", memberMentions.DefaultIfEmpty("None")), false);
            await context.RespondAsync(embedBuilder);
        }
    }
}
