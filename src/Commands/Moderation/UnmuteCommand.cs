using System;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using OoLunar.Tomoe.Interfaces;

namespace OoLunar.Tomoe.Commands.Moderation
{
    /// <summary>
    /// Unmutes the user from the guild.
    /// </summary>
    public sealed class UnmuteCommand : ModerationCommand
    {
        /// <summary>
        /// Unmutes a user.
        /// </summary>
        /// <param name="context">The context that the command was used in.</param>
        /// <param name="member">The user to unmute.</param>
        /// <param name="reason">Why the user is being unmuted.</param>
        [Command("unmute")]
        [Description("Unmutes a user.")]
        [RequireGuild, RequirePermissions(Permissions.ModerateMembers)]
        public async Task UnmuteAsync(CommandContext context, [Description("The user to unmute.")] DiscordMember member, [Description("Why the user is being unmuted."), RemainingText] string? reason = null)
        {
            // Ensure the executor and the bot have the ability to unmute the user.
            if (!await CheckPermissionsAsync(context, Permissions.BanMembers, member))
            {
                return;
            }
            // Check if the user is muted.
            else if (member.CommunicationDisabledUntil == null || member.CommunicationDisabledUntil < DateTimeOffset.UtcNow)
            {
                await context.RespondAsync("They're not muted!");
                Audit.AddNote("User wasn't muted.");
                return;
            }

            reason = Audit.SetReason(reason);
            Audit.AddAffectedUsers(member.Id);
            Audit.Successful = true;

            StringBuilder auditLogReason = new(2000);
            StringBuilder response = new(2000);
            if (!await DmMemberAsync(member, $"You have been unmuted from {context.Guild.Name} for: {reason}."))
            {
                auditLogReason = auditLogReason.AppendJoin(' ', Audit.Notes);
                response.Append("I was unable to DM the user, check audit logs for more information. ");
            }

            try
            {
                await member.TimeoutAsync(null, auditLogReason.ToString());
                auditLogReason.AppendFormat("Unmuted by {0}#{1}: {2}", context.User.Username, context.User.Discriminator, reason);
                response.AppendFormat("{0}#{1} has been unmuted.", member.Username, member.Discriminator);
            }
            catch (DiscordException error)
            {
                Audit.AddNote($"Failed to unmute, HTTP Error {error.WebResponse.ResponseCode}: {error.JsonMessage}.");
                response.AppendFormat("I was unable to unmute the user, HTTP Error {0}: {1}.", error.WebResponse.ResponseCode, error.JsonMessage);
                Audit.Successful = false;
            }

            await context.RespondAsync(response.ToString());
        }
    }
}
