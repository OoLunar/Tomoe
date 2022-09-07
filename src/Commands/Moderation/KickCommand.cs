using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using OoLunar.Tomoe.Interfaces;

namespace OoLunar.Tomoe.Commands.Moderation
{
    public sealed class KickCommand : ModerationCommand
    {
        [Command("kick")]
        [Description("Kicks a user.")]
        [RequireGuild, RequirePermissions(Permissions.KickMembers)]
        public async Task KickAsync(CommandContext context, DiscordMember member, [RemainingText] string? reason = null)
        {
            await CheckPermissionsAsync(context, Permissions.KickMembers, member);

            reason = Audit.SetReason(reason);
            Audit.AffectedUsers = new[] { member.Id };
            Audit.Successful = true;

            string auditLogReason = "";
            string response = "";
            if (!await DmMemberAsync(member, $"You have been kicked from {context.Guild.Name} for: {reason}."))
            {
                auditLogReason = Audit.Notes![0];
                response = "I was unable to DM the user. ";
            }

            try
            {
                await member.RemoveAsync(auditLogReason);
                auditLogReason += $"Kicked by {context.Member!.Username}#{context.Member.Discriminator}: {reason}";
                response = $"{member.Username}#{member.Discriminator} has been kicked. " + response;
            }
            catch (DiscordException error)
            {
                Audit.AddNote($"Failed to kick, HTTP Error {error.WebResponse.ResponseCode}: {error.JsonMessage}.");
                response = "I was unable to kick the user, check audit logs for more information. ";
                Audit.Successful = false;
            }

            await context.RespondAsync(response);
        }
    }
}
