using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using OoLunar.Tomoe.Interfaces;

namespace OoLunar.Tomoe.Commands.Moderation
{
    public sealed class BanCommand : ModerationCommand
    {
        [Command("ban")]
        [Description("Bans a user.")]
        [RequireGuild, RequirePermissions(Permissions.BanMembers)]
        public async Task BanAsync(CommandContext context, DiscordMember member, int dayPurgeCount = 1, [RemainingText] string? reason = null)
        {
            if (!await CheckPermissionsAsync(context, Permissions.BanMembers, member))
            {
                return;
            }

            reason = Audit.SetReason(reason);
            Audit.AffectedUsers = new[] { member.Id };
            Audit.Successful = true;

            string auditLogReason = "";
            string response = "";
            if (!await DmMemberAsync(member, $"You have been banned from {context.Guild.Name} for: {reason}."))
            {
                auditLogReason = string.Join(' ', Audit.Notes!);
                response = "I was unable to DM the user, check audit logs for more information. ";
            }

            try
            {
                await member.BanAsync(0, auditLogReason);
                auditLogReason += $"Banned by {context.Member!.Username}#{context.Member.Discriminator}: {reason}";
                response = $"{member.Username}#{member.Discriminator} has been banned. " + response;
            }
            catch (DiscordException error)
            {
                Audit.AddNote($"Failed to ban, HTTP Error {error.WebResponse.ResponseCode}: {error.JsonMessage}.");
                response = "I was unable to ban the user, check audit logs for more information. ";
                Audit.Successful = false;
            }

            await context.RespondAsync(response);
        }

        [Command("ban")]
        public Task BanAsync(CommandContext context, DiscordMember member, [RemainingText] string? reason = null)
            => BanAsync(context, member, 1, reason);
    }
}
