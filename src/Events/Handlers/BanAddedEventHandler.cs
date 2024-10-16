using System;
using System.Collections.Frozen;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Entities.AuditLogs;
using DSharpPlus.EventArgs;
using OoLunar.Tomoe.Database.Models;

namespace OoLunar.Tomoe.Events.Handlers
{
    public class BanAddedEventHandler : IEventHandler<GuildBanAddedEventArgs>
    {
        [DiscordEvent(DiscordIntents.GuildModeration)]
        public async Task HandleEventAsync(DiscordClient sender, GuildBanAddedEventArgs eventArgs)
        {
            await UpdateMemberDatabaseAsync(eventArgs.Member);
            await LogBanAsync(eventArgs.Member);
        }

        private static async ValueTask UpdateMemberDatabaseAsync(DiscordMember member)
        {
            GuildMemberModel? guildMemberModel = await GuildMemberModel.FindMemberAsync(member.Id, member.Guild.Id);
            if (guildMemberModel is null)
            {
                // If the member doesn't exist, create them with the absent and banned state.
                await GuildMemberModel.CreateAsync(member.Id, member.Guild.Id, member.JoinedAt, GuildMemberState.Absent | GuildMemberState.Banned, member.Roles.Select(x => x.Id));
                return;
            }

            // If the member previously existed, update their state.
            guildMemberModel.State |= GuildMemberState.Absent | GuildMemberState.Banned;
            guildMemberModel.RoleIds = member.Roles.Select(x => x.Id).ToList();
            await guildMemberModel.UpdateAsync();
        }

        private static async ValueTask LogBanAsync(DiscordMember member)
        {
            // Check to see if the guild has logging enabled for the ban event
            if (await GuildLoggingModel.GetLoggingAsync(member.Guild.Id, GuildLoggingType.MemberBanned) is not GuildLoggingModel logging || !logging.Enabled)
            {
                return;
            }

            // Get the channel to log the event in
            DiscordChannel channel = await member.Guild.GetChannelAsync(logging.ChannelId);

            // Ensure all audit logs are the latest
            DateTimeOffset timestamp = DateTimeOffset.UtcNow.AddSeconds(-3);

            // Figure out who unbanned the user
            await foreach (DiscordAuditLogEntry entry in member.Guild.GetAuditLogsAsync(100, null, DiscordAuditLogActionType.Ban))
            {
                if (LoggingEventHandlers.TryFilterAuditLogEntry(entry, DiscordAuditLogActionType.Ban, timestamp, out DiscordAuditLogBanEntry? banEntry) && banEntry.Target.Id == member.Id)
                {
                    await LoggingEventHandlers.SendLogMessageAsync(channel, logging, FrozenDictionary<string, string>.Empty, member, banEntry.UserResponsible, banEntry.Reason);
                    return;
                }
            }

            // No responsible user was found, so we just log the event
            await LoggingEventHandlers.SendLogMessageAsync(channel, logging, FrozenDictionary<string, string>.Empty, member);
        }
    }
}
