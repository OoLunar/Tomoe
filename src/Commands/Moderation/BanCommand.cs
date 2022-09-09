using System.Globalization;
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
    /// Bans a user from the guild.
    /// </summary>
    public sealed class BanCommand : ModerationCommand
    {
        /// <inheritdoc cref="BanAsync(CommandContext, DiscordMember, int, string?)"/>
        [Command("ban")]
        [Description("Bans a user from the guild.")]
        [RequireGuild, RequirePermissions(Permissions.BanMembers)]
        public Task BanAsync(CommandContext context, DiscordMember member, [RemainingText] string? reason = null)
            => BanAsync(context, member, 1, reason);

        /// <summary>
        /// Bans a user from the guild.
        /// </summary>
        /// <param name="context">The context that the command was used in.</param>
        /// <param name="member">Who's being banned.</param>
        /// <param name="dayPurgeCount">Delete their messages from the past X days.</param>
        /// <param name="reason">Why the user is being banned.</param>
        [Command("ban")]
        public async Task BanAsync(CommandContext context, DiscordMember member, int dayPurgeCount = 1, [RemainingText] string? reason = null)
        {
            // Ensure the executor and the bot have the ability to ban the user.
            if (!await CheckPermissionsAsync(context, Permissions.BanMembers, member))
            {
                return;
            }

            reason = Audit.SetReason(reason);
            Audit.AddAffectedUsers(member.Id);

            StringBuilder auditLogReason = new(2000);
            StringBuilder response = new(2000);
            if (!await DmMemberAsync(member, $"You have been banned from {context.Guild.Name} for: {reason}."))
            {
                auditLogReason.AppendJoin(' ', Audit.Notes);
                response.Append("I was unable to DM the user, check audit logs for more information. ");
            }

            try
            {
                await member.BanAsync(dayPurgeCount, auditLogReason.ToString());
                auditLogReason.AppendFormat("Banned by {0}#{1}: {2}", context.User.Username, context.User.Discriminator, reason);
                if (dayPurgeCount > 0)
                {
                    response.AppendFormat(CultureInfo.InvariantCulture, "{0}#{1} has been banned and their messages from the past {2} days have been removed.", member.Username, member.Discriminator, dayPurgeCount);
                }
                else
                {
                    response.AppendFormat(CultureInfo.InvariantCulture, "{0}#{1} has been banned.", member.Username, member.Discriminator);
                }
                Audit.Successful = true;
            }
            catch (DiscordException error)
            {
                Audit.AddNote($"Failed to ban, HTTP Error {error.WebResponse.ResponseCode}: {error.JsonMessage}.");
                response.Insert(0, "I was unable to ban the user, check audit logs for more information. ");
            }

            await context.RespondAsync(response.ToString());
        }
    }
}
