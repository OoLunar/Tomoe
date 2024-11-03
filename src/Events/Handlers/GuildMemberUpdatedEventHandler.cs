using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Entities.AuditLogs;
using DSharpPlus.EventArgs;
using Humanizer;
using OoLunar.Tomoe.Database.Models;

namespace OoLunar.Tomoe.Events.Handlers
{
    public sealed class GuildMemberUpdatedEventHandler : IEventHandler<GuildMemberUpdatedEventArgs>
    {
        [DiscordEvent(DiscordIntents.GuildMembers)]
        public async Task HandleEventAsync(DiscordClient sender, GuildMemberUpdatedEventArgs eventArgs)
        {
            await UpdateMemberDatabaseAsync(eventArgs.Member);
            await LogMemberUpdateAsync(eventArgs);
        }

        private static async ValueTask UpdateMemberDatabaseAsync(DiscordMember member)
        {
            GuildMemberModel? guildMemberModel = await GuildMemberModel.FindMemberAsync(member.Id, member.Guild.Id);
            if (guildMemberModel is null)
            {
                // If the member doesn't exist, create them with the none state.
                await GuildMemberModel.CreateAsync(member.Id, member.Guild.Id, member.JoinedAt, GuildMemberState.None, member.Roles.Select(x => x.Id));
                return;
            }

            // If the member previously existed, update their state.
            guildMemberModel.State = GuildMemberState.None;
            guildMemberModel.RoleIds = member.Roles.Select(x => x.Id).ToList();
            await guildMemberModel.UpdateAsync();
        }

        private static async ValueTask LogMemberUpdateAsync(GuildMemberUpdatedEventArgs eventArgs)
        {
            Dictionary<string, string> args = [];

            // If the timeout status has not changed, then the user was updated
            if (eventArgs.MemberBefore.IsTimedOut == eventArgs.MemberAfter.IsTimedOut)
            {
                // Ensure the guild has logging enabled for the member update event
                if (await GuildLoggingModel.GetLoggingAsync(eventArgs.Guild.Id, GuildLoggingType.MemberUpdated) is not GuildLoggingModel logging || !logging.Enabled)
                {
                    return;
                }

                // Set the args
                args["{user_before_display_name}"] = eventArgs.MemberBefore.GetDisplayName();
                args["{user_before_global_name}"] = eventArgs.MemberBefore.GlobalName!;
                args["{user_before_name}"] = eventArgs.MemberBefore.Username + (eventArgs.MemberBefore.Discriminator == "0" ? "" : $"#{eventArgs.MemberBefore.Discriminator}");
                args["{user_before_guild_avatar_url}"] = eventArgs.MemberBefore.DisplayAvatarUrl;
                args["{user_before_avatar_url}"] = eventArgs.MemberBefore.AvatarUrl;
                args["{user_before_role_count}"] = eventArgs.MemberBefore.Roles.Count().ToString("N0", CultureInfo.InvariantCulture);
                args["{user_before_role_list}"] = string.Join(", ", eventArgs.MemberBefore.Roles.Select(x => x.Mention));

                args["{user_guild_avatar_url}"] = eventArgs.MemberAfter.DisplayAvatarUrl;
                args["{user_avatar_url}"] = eventArgs.MemberAfter.AvatarUrl;
                args["{user_role_count}"] = eventArgs.MemberAfter.Roles.Count().ToString("N0", CultureInfo.InvariantCulture);
                args["{user_role_list}"] = string.Join(", ", eventArgs.MemberAfter.Roles.Select(x => x.Mention));

                // Get the channel to log the event in
                DiscordChannel channel = await eventArgs.Guild.GetChannelAsync(logging.ChannelId);

                // Send the log message
                await LoggingEventHandlers.SendLogMessageAsync(channel, logging, args, eventArgs.MemberAfter);
            }
            else if (eventArgs.MemberAfter.CommunicationDisabledUntil is not null)
            {
                // Ensure the guild has logging enabled for the mute event
                if (await GuildLoggingModel.GetLoggingAsync(eventArgs.Guild.Id, GuildLoggingType.MemberMuted) is not GuildLoggingModel logging || !logging.Enabled)
                {
                    return;
                }

                // Get the channel to log the event in
                DiscordChannel channel = await eventArgs.Guild.GetChannelAsync(logging.ChannelId);

                // Apply the mute duration
                args["{mute_expires}"] = Formatter.Timestamp(eventArgs.MemberAfter.CommunicationDisabledUntil.Value);

                // Figure out who muted the user
                DateTimeOffset timestamp = DateTimeOffset.UtcNow.AddSeconds(-3);

                // Wait 1 second so the audit logs can catch up
                await Task.Delay(1000);
                await foreach (DiscordAuditLogEntry entry in eventArgs.Guild.GetAuditLogsAsync(100, null, DiscordAuditLogActionType.MemberUpdate))
                {
                    if (LoggingEventHandlers.TryFilterAuditLogEntry(entry, DiscordAuditLogActionType.MemberUpdate, timestamp, out DiscordAuditLogMemberUpdateEntry? updateEntry) && updateEntry.Target.Id == eventArgs.Member.Id)
                    {
                        args["{mute_duration}"] = (eventArgs.MemberAfter.CommunicationDisabledUntil.Value - updateEntry.CreationTimestamp).Humanize();
                        await LoggingEventHandlers.SendLogMessageAsync(channel, logging, args, eventArgs.Member, updateEntry.UserResponsible, updateEntry.Reason);
                        return;
                    }
                }

                // Guesstimate the mute duration
                args["{mute_duration}"] = (eventArgs.MemberAfter.CommunicationDisabledUntil.Value.AddSeconds(5) - DateTimeOffset.UtcNow).Humanize();

                // No responsible user was found, so we just log the event
                await LoggingEventHandlers.SendLogMessageAsync(channel, logging, args, eventArgs.Member);
            }
            else
            {
                // Ensure the guild has logging enabled for the unmute event
                if (await GuildLoggingModel.GetLoggingAsync(eventArgs.Guild.Id, GuildLoggingType.MemberUnmuted) is not GuildLoggingModel logging || !logging.Enabled)
                {
                    return;
                }

                // Get the channel to log the event in
                DiscordChannel channel = await eventArgs.Guild.GetChannelAsync(logging.ChannelId);

                // Figure out who unmuted the user
                DateTimeOffset timestamp = DateTimeOffset.UtcNow.AddSeconds(-3);
                await foreach (DiscordAuditLogEntry entry in eventArgs.Guild.GetAuditLogsAsync(100, null, DiscordAuditLogActionType.MemberUpdate))
                {
                    if (LoggingEventHandlers.TryFilterAuditLogEntry(entry, DiscordAuditLogActionType.MemberUpdate, timestamp, out DiscordAuditLogMemberUpdateEntry? updateEntry) && updateEntry.Target.Id == eventArgs.Member.Id)
                    {
                        await LoggingEventHandlers.SendLogMessageAsync(channel, logging, args, eventArgs.Member, updateEntry.UserResponsible, updateEntry.Reason);
                        return;
                    }
                }

                // No responsible user was found, so we just log the event
                await LoggingEventHandlers.SendLogMessageAsync(channel, logging, args, eventArgs.Member);
            }
        }
    }
}
