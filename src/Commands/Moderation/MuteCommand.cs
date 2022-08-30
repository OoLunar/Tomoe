using System;
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
    public sealed class MuteCommand : ModerationCommand
    {
        [Command("mute")]
        [Description("Mutes a user.")]
        [RequireGuild, RequirePermissions(Permissions.ModerateMembers)]
        public async Task MuteAsync(CommandContext context, DiscordMember member, TimeSpan? timeSpan = null, [RemainingText] string? reason = null)
        {
            if (!await CheckPermissionsAsync(context, Permissions.ModerateMembers, member))
            {
                return;
            }
            else if (member.CommunicationDisabledUntil != null && member.CommunicationDisabledUntil >= DateTimeOffset.UtcNow)
            {
                await context.RespondAsync($"They're already muted until {Formatter.Timestamp(member.CommunicationDisabledUntil.Value, TimestampFormat.ShortDateTime)}");
                Audit.AddNote("User was already muted.");
                return;
            }

            reason = Audit.SetReason(reason);
            Audit.AffectedUsers = new[] { member.Id };
            timeSpan ??= TimeSpan.FromMinutes(5);
            Audit.Duration = new Range<DateTimeOffset>(DateTimeOffset.UtcNow, DateTimeOffset.UtcNow + timeSpan);

            Audit.Successful = true;
            string auditLogReason = "";
            string response = "";

            if (!await DmMemberAsync(member, $"You have been muted in {context.Guild.Name} for: {reason}."))
            {
                auditLogReason = string.Join(' ', Audit.Notes!);
                response = "I was unable to DM the user, check audit logs for more information. ";
            }

            try
            {
                await member.TimeoutAsync(DateTimeOffset.UtcNow.Add(timeSpan.Value), auditLogReason);
                auditLogReason += $"Muted by {context.Member!.Username}#{context.Member.Discriminator}: {reason}";
                response = $"{member.Username}#{member.Discriminator} has been muted until {Formatter.Timestamp(timeSpan.Value, TimestampFormat.ShortDateTime)}. " + response;
            }
            catch (DiscordException error)
            {
                Audit.AddNote($"Failed to mute, HTTP Error {error.WebResponse.ResponseCode}: {error.JsonMessage}.");
                response = "I was unable to mute the user, check audit logs for more information.";
                Audit.Successful = false;
            }

            await context.RespondAsync(response);
        }
    }
}
