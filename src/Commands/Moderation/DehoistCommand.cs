using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using OoLunar.Tomoe.Database.Models;

namespace OoLunar.Tomoe.Commands.Moderation
{
    /// <summary>
    /// Get drop kicked nerd.
    /// </summary>
    public static class DehoistCommand
    {
        /// <summary>
        /// Renames users with hoisted names to prevent them from appearing at the top of the member list.
        /// </summary>
        [Command("dehoist")]
        [RequirePermissions(DiscordPermission.ManageNicknames)]
        public static async ValueTask ExecuteAsync(CommandContext context)
        {
            await context.DeferResponseAsync();

            int dehoistedMemberCount = 0;
            List<DiscordMember> failedMembers = [];
            string format = await GuildSettingsModel.GetAutoDehoistFormatAsync(context.Guild!.Id) ?? "Dehoisted";
            await foreach (DiscordMember member in context.Guild!.GetAllMembersAsync())
            {
                if (!ShouldDehoist(member))
                {
                    continue;
                }

                try
                {
                    await member.ModifyAsync(memberEditModel =>
                    {
                        memberEditModel.AuditLogReason = $"Requested by {context.Member!.GetDisplayName()} ({context.Member!.Id}): Dehoisted.";
                        memberEditModel.Nickname = GetNewDisplayName(member, format);
                    });

                    dehoistedMemberCount++;
                }
                catch (DiscordException)
                {
                    try
                    {
                        await member.ModifyAsync(memberEditModel =>
                        {
                            memberEditModel.AuditLogReason = $"Requested by {context.Member!.GetDisplayName()} ({context.Member!.Id}): Dehoisted.";
                            memberEditModel.Nickname = GetNewDisplayName(member, format, true);
                        });

                        dehoistedMemberCount++;
                    }
                    catch (DiscordException)
                    {
                        failedMembers.Add(member);
                    }
                }
            }

            StringBuilder stringBuilder = new();
            stringBuilder.AppendLine($"Successfully dehoisted {dehoistedMemberCount:N0} members.");
            if (failedMembers.Count > 0)
            {
                stringBuilder.AppendLine("Failed to dehoist the following members:");
                foreach (DiscordMember member in failedMembers)
                {
                    stringBuilder.AppendLine($"- {member.Mention} ({member.Username})");
                }
            }

            await context.RespondAsync(stringBuilder.ToString());
        }

        internal static bool ShouldDehoist(DiscordMember member) => !string.IsNullOrWhiteSpace(member.DisplayName)
            && member.DisplayName[0] < 'A' && !char.IsBetween(member.DisplayName[0], '0', '9');

        internal static string GetNewDisplayName(DiscordMember member, string format, bool returnFormatDirectly = false)
        {
            string newDisplayName = "";
            for (int i = 0; i < member.DisplayName.Length; i++)
            {
                char character = member.DisplayName[i];
                if (character < 'A' && !char.IsBetween(character, '0', '9'))
                {
                    continue;
                }

                newDisplayName = member.DisplayName[i..].Trim();
                break;
            }

            return !returnFormatDirectly
                && !string.IsNullOrWhiteSpace(newDisplayName)
                && newDisplayName.Length > 0
                && newDisplayName.Length <= 32
                && newDisplayName != "everyone"
                && newDisplayName != "here"
                    ? newDisplayName
                    : format
                        .Replace("{display_name}", newDisplayName.ToString())
                        .Replace("{user_name}", member.Username);
        }
    }
}
