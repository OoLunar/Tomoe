using System;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using OoLunar.Tomoe.Interfaces;

namespace OoLunar.Tomoe.Commands.Moderation
{
    public sealed class UnmuteCommand : ModerationCommand
    {
        [Command("unmute")]
        [Description("Unmutes a user.")]
        [RequireGuild, RequirePermissions(Permissions.ModerateMembers)]
        public async Task UnmuteAsync(CommandContext context, [Description("The user to unmute.")] DiscordMember member, [RemainingText] string? reason = null)
        {
            if (!await CheckPermissionsAsync(context, Permissions.BanMembers, member))
            {
                return;
            }
            else if (member.CommunicationDisabledUntil == null || member.CommunicationDisabledUntil < DateTimeOffset.UtcNow)
            {
                await context.RespondAsync($"They're not muted!");
                Audit.AddNote("User wasn't muted.");
                return;
            }

            reason = Audit.SetReason(reason);
            Audit.AffectedUsers = new[] { member.Id };
            Audit.Successful = true;

            string auditLogReason = "";
            string response = "";
            if (!await DmMemberAsync(member, $"You have been unmuted from {context.Guild.Name} for: {reason}."))
            {
                auditLogReason = string.Join(' ', Audit.Notes!);
                response = "I was unable to DM the user, check audit logs for more information. ";
            }

            try
            {
                await member.TimeoutAsync(null, auditLogReason);
                auditLogReason += $"Unmuted by {context.Member!.Username}#{context.Member.Discriminator}: {reason}";
                response = $"{member.Username}#{member.Discriminator} has been unmuted. " + response;
            }
            catch (DiscordException error)
            {
                Audit.AddNote($"Failed to unmute, HTTP Error {error.WebResponse.ResponseCode}: {error.JsonMessage}.");
                response = $"I was unable to unmute the user, HTTP Error {error.WebResponse.ResponseCode}: {error.JsonMessage}.";
                Audit.Successful = false;
            }

            await context.RespondAsync(response);
        }
    }
}
