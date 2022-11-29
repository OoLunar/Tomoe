using System;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using EdgeDB.DataTypes;
using OoLunar.Tomoe.Interfaces;

namespace OoLunar.Tomoe.Commands.Moderation
{
    /// <summary>
    /// Mutes a user in the guild.
    /// </summary>
    public sealed class MuteCommand : ModerationCommand
    {
        /// <inheritdoc cref="MuteAsync(CommandContext, DiscordMember, TimeSpan?, string?)"/>
        [Command("mute")]
        [Description("Mutes a user.")]
        [RequireGuild, RequirePermissions(Permissions.ModerateMembers)]
        public Task MuteAsync(CommandContext context, [Description("Who is being muted.")] DiscordMember member, [Description("Why they're being muted."), RemainingText] string reason = "No reason provided.")
            => MuteAsync(context, member, null, reason);

        /// <summary>
        /// Mutes a user in the guild.
        /// </summary>
        /// <param name="context">The context that the command was used in.</param>
        /// <param name="member">Who is being muted.</param>
        /// <param name="timeSpan">How long they're being muted for.</param>
        /// <param name="reason">Why they're being muted.</param>
        [Command("mute")]
        public async Task MuteAsync(CommandContext context, [Description("Who is being muted.")] DiscordMember member, [Description("How long they're being muted for.")] TimeSpan? timeSpan = null, [Description("Why they're being muted."), RemainingText] string? reason = null)
        {
            // Check if we can mute the user (hierarchy, server owner, permissions, etc).
            if (!await CheckPermissionsAsync(context, Permissions.ModerateMembers, member))
            {
                return;
            }
            // Check if the user is already muted.
            else if (member.CommunicationDisabledUntil != null && member.CommunicationDisabledUntil >= DateTimeOffset.UtcNow)
            {
                await context.RespondAsync($"They're already muted until {Formatter.Timestamp(member.CommunicationDisabledUntil.Value, TimestampFormat.ShortDateTime)}");
                Audit.AddNote("User was already muted.");
                return;
            }

            // Make sure the time span is valid, otherwise default to 5 minutes.
            if (timeSpan == null || timeSpan.Value.TotalSeconds < 0)
            {
                timeSpan = TimeSpan.FromMinutes(5);
            }
            // Set start and end times in the audit log.
            Audit.Duration = new Range<DateTimeOffset>(DateTimeOffset.UtcNow, DateTimeOffset.UtcNow + timeSpan);
            Audit.AddAffectedUsers(member.Id);
            Audit.Successful = true;
            reason = Audit.SetReason(reason);
            StringBuilder auditLogReason = new(2000);
            StringBuilder response = new(2000);

            if (!await DmMemberAsync(member, $"You have been muted in {context.Guild.Name} for: {reason}."))
            {
                auditLogReason.AppendJoin(' ', Audit.Notes);
                response.Append("I was unable to DM the user, check audit logs for more information. ");
            }

            try
            {
                await member.TimeoutAsync(DateTimeOffset.UtcNow.Add(timeSpan.Value), auditLogReason.ToString());
                auditLogReason.AppendFormat("Muted by {0}#{1}: {2}", context.Member!.Username, context.Member.Discriminator, reason);
                response.AppendFormat("{0}#{1} has been muted until {2}.", member.Username, member.Discriminator, Formatter.Timestamp(timeSpan.Value, TimestampFormat.ShortDateTime));
            }
            catch (DiscordException error)
            {
                Audit.AddNote($"Failed to mute, HTTP Error {error.WebResponse.ResponseCode}: {error.JsonMessage}.");
                auditLogReason.Append("I was unable to mute the user, check audit logs for more information.");
                Audit.Successful = false;
            }

            await context.RespondAsync(response.ToString());
        }
    }
}
