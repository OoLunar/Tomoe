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
    /// Kicks a user from the guild.
    /// </summary>
    public sealed class KickCommand : ModerationCommand
    {
        /// <summary>
        /// Kicks a user from the guild.
        /// </summary>
        /// <param name="context">The context that the command was used in.</param>
        /// <param name="member">Who is being kicked.</param>
        /// <param name="reason">Why they're being kicked.</param>
        [Command("kick")]
        [Description("Kicks a user from the guild.")]
        [RequireGuild, RequirePermissions(Permissions.KickMembers)]
        public async Task KickAsync(CommandContext context, [Description("Who is being kicked.")] DiscordMember member, [Description("Why they're being kicked"), RemainingText] string? reason = null)
        {
            // Check if we can kick the user (hierarchy, server owner, permissions, etc).
            if (!await CheckPermissionsAsync(context, Permissions.KickMembers, member))
            {
                return;
            }

            reason = Audit.SetReason(reason);
            Audit.AddAffectedUsers(member.Id);
            Audit.Successful = true;

            StringBuilder auditLogReason = new(2000);
            StringBuilder response = new(2000);
            if (!await DmMemberAsync(member, $"You have been kicked from {context.Guild.Name} for: {reason}."))
            {
                auditLogReason.Append(Audit.Notes[0]);
                response.Append("I was unable to DM the user. ");
            }

            try
            {
                await member.RemoveAsync(auditLogReason.ToString());
                auditLogReason.AppendFormat("Kicked by {0}#{1}: {2}", context.User.Username, context.User.Discriminator, reason);
                response.Insert(0, $"{member.Username}#{member.Discriminator} has been kicked. ");
            }
            catch (DiscordException error)
            {
                Audit.AddNote($"Failed to kick, HTTP Error {error.WebResponse.ResponseCode}: {error.JsonMessage}.");
                response.Insert(0, "I was unable to kick the user, check audit logs for more information. ");
                Audit.Successful = false;
            }

            await context.RespondAsync(response.ToString());
        }
    }
}
