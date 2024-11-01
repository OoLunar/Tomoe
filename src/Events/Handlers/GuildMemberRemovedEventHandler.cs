using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Entities.AuditLogs;
using DSharpPlus.EventArgs;
using OoLunar.Tomoe.Database.Models;

namespace OoLunar.Tomoe.Events.Handlers
{
    public sealed class GuildMemberRemovedEventHandler : IEventHandler<GuildMemberRemovedEventArgs>
    {
        [DiscordEvent(DiscordIntents.GuildMembers)]
        public async Task HandleEventAsync(DiscordClient sender, GuildMemberRemovedEventArgs eventArgs)
        {
            await UpdateMemberDatabaseAsync(eventArgs.Member);
            await LogLeaveAsync(eventArgs.Member);
        }

        private static async ValueTask UpdateMemberDatabaseAsync(DiscordMember member)
        {
            GuildMemberModel? guildMemberModel = await GuildMemberModel.FindMemberAsync(member.Id, member.Guild.Id);
            if (guildMemberModel is null)
            {
                // If the member doesn't exist, create them with the absent state.
                await GuildMemberModel.CreateAsync(member.Id, member.Guild.Id, member.JoinedAt, GuildMemberState.Absent, member.Roles.Select(x => x.Id));
                return;
            }

            // If the member previously existed, set their state to absent.
            guildMemberModel.State |= GuildMemberState.Absent;
            guildMemberModel.RoleIds = member.Roles.Select(x => x.Id).ToList();
            await guildMemberModel.UpdateAsync();
        }

        private static async ValueTask LogLeaveAsync(DiscordMember member)
        {
            GuildLoggingType? type = null;
            string? reason = null;
            DiscordUser? responsible = null;
            Dictionary<string, string> args = [];

            // Ensure all audit logs are the latest
            DateTimeOffset timestamp = DateTimeOffset.UtcNow.AddSeconds(-3);

            // Wait 1 second so the audit logs can catch up
            await Task.Delay(1000);
            await foreach (DiscordAuditLogEntry entry in member.Guild.GetAuditLogsAsync(100))
            {
                if (LoggingEventHandlers.TryFilterAuditLogEntry(entry, DiscordAuditLogActionType.Ban, timestamp, out DiscordAuditLogBanEntry? banEntry) && banEntry.Target.Id == member.Id)
                {
                    // Return immediately because the above event will handle this
                    return;
                }
                else if (LoggingEventHandlers.TryFilterAuditLogEntry(entry, DiscordAuditLogActionType.Kick, timestamp, out DiscordAuditLogKickEntry? kickEntry) && kickEntry.Target.Id == member.Id)
                {
                    type = GuildLoggingType.MemberKicked;
                    reason = kickEntry.Reason;
                    responsible = kickEntry.UserResponsible;
                    break;
                }
                else if (LoggingEventHandlers.TryFilterAuditLogEntry(entry, DiscordAuditLogActionType.Prune, timestamp, out DiscordAuditLogPruneEntry? pruneEntry))
                {
                    // The prune event doesn't tell us who was removed, so we just have to assume it was the member
                    args["{prune_count}"] = pruneEntry.Toll.ToString("N0", CultureInfo.InvariantCulture);
                    args["{prune_days}"] = pruneEntry.Days.ToString("N0", CultureInfo.InvariantCulture);
                    reason = pruneEntry.Reason;
                    responsible = pruneEntry.UserResponsible;
                    type = GuildLoggingType.MemberPruned;
                    break;
                }
            }

            // If we still don't know, then the member left on their own
            if (type is null)
            {
                type = GuildLoggingType.MemberLeft;
                args["{member_count}"] = (await GuildMemberModel.CountMembersAsync(member.Guild.Id)).ToString("N0", CultureInfo.InvariantCulture);
            }

            // Check to see if the guild has logging enabled for the ban event
            if (await GuildLoggingModel.GetLoggingAsync(member.Guild.Id, type.Value) is not GuildLoggingModel logging || !logging.Enabled)
            {
                return;
            }

            // Get the channel to log the event in
            DiscordChannel channel = await member.Guild.GetChannelAsync(logging.ChannelId);

            // Send the log message
            await LoggingEventHandlers.SendLogMessageAsync(channel, logging, args, member, responsible, reason);
        }
    }
}
