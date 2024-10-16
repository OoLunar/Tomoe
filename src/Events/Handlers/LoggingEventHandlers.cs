using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
    public sealed class LoggingEventHandlers :
        IEventHandler<GuildBanRemovedEventArgs>,
        IEventHandler<GuildBanAddedEventArgs>,
        IEventHandler<GuildMemberRemovedEventArgs>,
        IEventHandler<GuildMemberUpdatedEventArgs>,
        IEventHandler<GuildMemberAddedEventArgs>,
        IEventHandler<MessageUpdatedEventArgs>,
        IEventHandler<MessageDeletedEventArgs>
    {
        [DiscordEvent(DiscordIntents.GuildModeration)]
        public async Task HandleEventAsync(DiscordClient sender, GuildBanRemovedEventArgs eventArgs)
        {
            // Check to see if the guild has logging enabled for the ban event
            if (await GuildLoggingModel.GetLoggingAsync(eventArgs.Guild.Id, GuildLoggingType.MemberUnbanned) is not GuildLoggingModel logging || !logging.Enabled)
            {
                return;
            }

            // Get the channel to log the event in
            DiscordChannel channel = await eventArgs.Guild.GetChannelAsync(logging.ChannelId);

            // Ensure all audit logs are the latest
            DateTimeOffset timestamp = DateTimeOffset.UtcNow.AddSeconds(-3);

            // Figure out who unbanned the user
            await foreach (DiscordAuditLogEntry entry in eventArgs.Guild.GetAuditLogsAsync(100, null, DiscordAuditLogActionType.Unban))
            {
                if (TryFilterAuditLogEntry(entry, DiscordAuditLogActionType.Unban, timestamp, out DiscordAuditLogBanEntry? banEntry) && banEntry.Target.Id == eventArgs.Member.Id)
                {
                    await SendLogMessageAsync(channel, logging, FrozenDictionary<string, string>.Empty, eventArgs.Member, banEntry.UserResponsible, banEntry.Reason);
                    return;
                }
            }

            // No responsible user was found, so we just log the event
            await SendLogMessageAsync(channel, logging, FrozenDictionary<string, string>.Empty, eventArgs.Member);
        }

        [DiscordEvent(DiscordIntents.GuildModeration)]
        public async Task HandleEventAsync(DiscordClient sender, GuildBanAddedEventArgs eventArgs)
        {
            // Check to see if the guild has logging enabled for the ban event
            if (await GuildLoggingModel.GetLoggingAsync(eventArgs.Guild.Id, GuildLoggingType.MemberBanned) is not GuildLoggingModel logging || !logging.Enabled)
            {
                return;
            }

            // Get the channel to log the event in
            DiscordChannel channel = await eventArgs.Guild.GetChannelAsync(logging.ChannelId);

            // Ensure all audit logs are the latest
            DateTimeOffset timestamp = DateTimeOffset.UtcNow.AddSeconds(-3);

            // Figure out who unbanned the user
            await foreach (DiscordAuditLogEntry entry in eventArgs.Guild.GetAuditLogsAsync(100, null, DiscordAuditLogActionType.Ban))
            {
                if (TryFilterAuditLogEntry(entry, DiscordAuditLogActionType.Ban, timestamp, out DiscordAuditLogBanEntry? banEntry) && banEntry.Target.Id == eventArgs.Member.Id)
                {
                    await SendLogMessageAsync(channel, logging, FrozenDictionary<string, string>.Empty, eventArgs.Member, banEntry.UserResponsible, banEntry.Reason);
                    return;
                }
            }

            // No responsible user was found, so we just log the event
            await SendLogMessageAsync(channel, logging, FrozenDictionary<string, string>.Empty, eventArgs.Member);
        }

        public async Task HandleEventAsync(DiscordClient sender, GuildMemberRemovedEventArgs eventArgs)
        {
            GuildLoggingType? type = null;
            string? reason = null;
            DiscordUser? responsible = null;
            Dictionary<string, string> args = [];

            // Ensure all audit logs are the latest
            DateTimeOffset timestamp = DateTimeOffset.UtcNow.AddSeconds(-3);
            await foreach (DiscordAuditLogEntry entry in eventArgs.Guild.GetAuditLogsAsync(100))
            {
                if (TryFilterAuditLogEntry(entry, DiscordAuditLogActionType.Ban, timestamp, out DiscordAuditLogBanEntry? banEntry) && banEntry.Target.Id == eventArgs.Member.Id)
                {
                    // Return immediately because the above event will handle this
                    return;
                }
                else if (TryFilterAuditLogEntry(entry, DiscordAuditLogActionType.Kick, timestamp, out DiscordAuditLogKickEntry? kickEntry) && kickEntry.Target.Id == eventArgs.Member.Id)
                {
                    type = GuildLoggingType.MemberKicked;
                    reason = kickEntry.Reason;
                    responsible = kickEntry.UserResponsible;
                    break;
                }
                else if (TryFilterAuditLogEntry(entry, DiscordAuditLogActionType.Prune, timestamp, out DiscordAuditLogPruneEntry? pruneEntry))
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
                args["{member_count}"] = (await GuildMemberModel.CountMembersAsync(eventArgs.Guild.Id)).ToString("N0", CultureInfo.InvariantCulture);
            }

            // Check to see if the guild has logging enabled for the ban event
            if (await GuildLoggingModel.GetLoggingAsync(eventArgs.Guild.Id, type.Value) is not GuildLoggingModel logging || !logging.Enabled)
            {
                return;
            }

            // Get the channel to log the event in
            DiscordChannel channel = await eventArgs.Guild.GetChannelAsync(logging.ChannelId);

            // Send the log message
            await SendLogMessageAsync(channel, logging, args, eventArgs.Member, responsible, reason);
        }

        public async Task HandleEventAsync(DiscordClient sender, GuildMemberUpdatedEventArgs eventArgs)
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
                args["{user_before_guild_avatar_url}"] = eventArgs.MemberBefore.GuildAvatarUrl;
                args["{user_before_avatar_url}"] = eventArgs.MemberBefore.AvatarUrl;
                args["{user_before_role_count}"] = eventArgs.MemberBefore.Roles.Count().ToString("N0", CultureInfo.InvariantCulture);
                args["{user_before_role_list}"] = string.Join(", ", eventArgs.MemberBefore.Roles.Select(x => x.Mention));

                args["{user_guild_avatar_url}"] = eventArgs.MemberAfter.GuildAvatarUrl;
                args["{user_avatar_url}"] = eventArgs.MemberAfter.AvatarUrl;
                args["{user_role_count}"] = eventArgs.MemberAfter.Roles.Count().ToString("N0", CultureInfo.InvariantCulture);
                args["{user_role_list}"] = string.Join(", ", eventArgs.MemberAfter.Roles.Select(x => x.Mention));

                // Get the channel to log the event in
                DiscordChannel channel = await eventArgs.Guild.GetChannelAsync(logging.ChannelId);

                // Send the log message
                await SendLogMessageAsync(channel, logging, args, eventArgs.MemberAfter);
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
                args["{mute_expires}"] = Formatter.Timestamp(eventArgs.MemberAfter.CommunicationDisabledUntil!.Value);
                args["{mute_duration}"] = (eventArgs.MemberAfter.CommunicationDisabledUntil!.Value.AddSeconds(5) - DateTimeOffset.UtcNow).Humanize();

                // Figure out who muted the user
                DateTimeOffset timestamp = DateTimeOffset.UtcNow.AddSeconds(-3);
                await foreach (DiscordAuditLogEntry entry in eventArgs.Guild.GetAuditLogsAsync(100, null, DiscordAuditLogActionType.MemberUpdate))
                {
                    if (TryFilterAuditLogEntry(entry, DiscordAuditLogActionType.MemberUpdate, timestamp, out DiscordAuditLogMemberUpdateEntry? updateEntry) && updateEntry.Target.Id == eventArgs.Member.Id)
                    {
                        await SendLogMessageAsync(channel, logging, args, eventArgs.Member, updateEntry.UserResponsible, updateEntry.Reason);
                        return;
                    }
                }

                // No responsible user was found, so we just log the event
                await SendLogMessageAsync(channel, logging, args, eventArgs.Member);
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
                    if (TryFilterAuditLogEntry(entry, DiscordAuditLogActionType.MemberUpdate, timestamp, out DiscordAuditLogMemberUpdateEntry? updateEntry) && updateEntry.Target.Id == eventArgs.Member.Id)
                    {
                        await SendLogMessageAsync(channel, logging, args, eventArgs.Member, updateEntry.UserResponsible, updateEntry.Reason);
                        return;
                    }
                }

                // No responsible user was found, so we just log the event
                await SendLogMessageAsync(channel, logging, args, eventArgs.Member);
            }
        }

        public async Task HandleEventAsync(DiscordClient sender, GuildMemberAddedEventArgs eventArgs)
        {
            // Ensure the guild has logging enabled for the member join event
            if (await GuildLoggingModel.GetLoggingAsync(eventArgs.Guild.Id, GuildLoggingType.MemberJoined) is not GuildLoggingModel logging || !logging.Enabled)
            {
                return;
            }

            // Get the channel to log the event in
            DiscordChannel channel = await eventArgs.Guild.GetChannelAsync(logging.ChannelId);

            // Get the new member count
            Dictionary<string, string> args = [];
            args["{member_count}"] = (await GuildMemberModel.CountMembersAsync(eventArgs.Guild.Id)).ToString("N0", CultureInfo.InvariantCulture);

            // Send the log message
            await SendLogMessageAsync(channel, logging, args, eventArgs.Member);
        }

        public async Task HandleEventAsync(DiscordClient sender, MessageUpdatedEventArgs eventArgs)
        {
            // Ensure the guild has logging enabled for the message update event
            if (await GuildLoggingModel.GetLoggingAsync(eventArgs.Guild.Id, GuildLoggingType.MessageUpdated) is not GuildLoggingModel logging || !logging.Enabled)
            {
                return;
            }

            // Get the channel to log the event in
            DiscordChannel channel = await eventArgs.Guild.GetChannelAsync(logging.ChannelId);

            Dictionary<string, string> args = [];
            args["{message_id}"] = eventArgs.Message.Id.ToString(CultureInfo.InvariantCulture);
            args["{message_link}"] = eventArgs.Message.JumpLink.ToString();
            args["{message_channel_mention}"] = eventArgs.Channel.Mention;
            args["{message_before_content}"] = eventArgs.MessageBefore?.Content ?? "<Unknown>";
            args["{message_before_attachment_count}"] = eventArgs.MessageBefore?.Attachments.Count.ToString("N0", CultureInfo.InvariantCulture) ?? "<Unknown>";
            args["{message_before_embed_count}"] = eventArgs.MessageBefore?.Embeds.Count.ToString("N0", CultureInfo.InvariantCulture) ?? "<Unknown>";
            args["{message_content}"] = eventArgs.Message.Content;
            args["{message_attachment_count}"] = eventArgs.Message.Flags.HasValue && eventArgs.Message.Flags.Value.HasFlag(DiscordMessageFlags.SuppressedEmbeds) ? "0" : eventArgs.Message.Attachments.Count.ToString("N0", CultureInfo.InvariantCulture);
            args["{message_embed_count}"] = eventArgs.Message.Embeds.Count.ToString("N0", CultureInfo.InvariantCulture);

            // Send the log message
            await SendLogMessageAsync(channel, logging, args, eventArgs.Author);
        }

        public async Task HandleEventAsync(DiscordClient sender, MessageDeletedEventArgs eventArgs)
        {
            // Ensure the guild has logging enabled for the message delete event
            if (await GuildLoggingModel.GetLoggingAsync(eventArgs.Guild.Id, GuildLoggingType.MessageDeleted) is not GuildLoggingModel logging || !logging.Enabled)
            {
                return;
            }

            // Get the channel to log the event in
            DiscordChannel channel = await eventArgs.Guild.GetChannelAsync(logging.ChannelId);

            Dictionary<string, string> args = [];
            args["{message_id}"] = eventArgs.Message.Id.ToString(CultureInfo.InvariantCulture);
            args["{message_link}"] = eventArgs.Message.JumpLink.ToString();
            args["{message_channel_mention}"] = eventArgs.Channel.Mention;
            args["{message_content}"] = eventArgs.Message.Content;
            args["{message_attachment_count}"] = eventArgs.Message.Flags.HasValue && eventArgs.Message.Flags.Value.HasFlag(DiscordMessageFlags.SuppressedEmbeds) ? "0" : eventArgs.Message.Attachments.Count.ToString("N0", CultureInfo.InvariantCulture);
            args["{message_embed_count}"] = eventArgs.Message.Embeds.Count.ToString("N0", CultureInfo.InvariantCulture);

            // Send the log message
            await SendLogMessageAsync(channel, logging, args, eventArgs.Message.Author!);
        }

        private static async ValueTask SendLogMessageAsync(DiscordChannel channel, GuildLoggingModel logging, IReadOnlyDictionary<string, string> args, DiscordUser user, DiscordUser? moderator = null, string? reason = null)
        {
            // Format the message
            string message = logging.Format;

            // Apply information for the user
            message = message
                .Replace("{user_id}", user.Id.ToString(CultureInfo.InvariantCulture))
                .Replace("{user_mention}", user.Mention)
                .Replace("{user_display_name}", user.GetDisplayName())
                .Replace("{user_global_name}", user.GlobalName)
                .Replace("{user_name}", user.Username + (user.Discriminator == "0" ? "" : $"#{user.Discriminator}"));

            // Apply information for the moderator, if available
            message = moderator is null
                ? message
                    .Replace("{moderator_id}", "<Unknown>")
                    .Replace("{moderator_mention}", "<Unknown>")
                    .Replace("{moderator_display_name}", "<Unknown>")
                    .Replace("{moderator_global_name}", "<Unknown>")
                    .Replace("{moderator_name}", "<Unknown>")
                    .Replace("{reason}", reason ?? "<Unknown Reason>")
                : message
                    .Replace("{moderator_id}", moderator.Id.ToString(CultureInfo.InvariantCulture))
                    .Replace("{moderator_mention}", moderator.Mention)
                    .Replace("{moderator_display_name}", moderator.GetDisplayName())
                    .Replace("{moderator_global_name}", moderator.GlobalName)
                    .Replace("{moderator_name}", moderator.Username + (moderator.Discriminator == "0" ? "" : $"#{moderator.Discriminator}"))
                    .Replace("{reason}", reason ?? "<No Reason Provided>");

            foreach ((string key, string value) in args)
            {
                message = message.Replace(key, value);
            }

            // Send the message
            await channel.SendMessageAsync(new DiscordMessageBuilder().WithContent(message).WithAllowedMentions(Mentions.None));
        }

        private static bool TryFilterAuditLogEntry<T>(DiscordAuditLogEntry entry, DiscordAuditLogActionType actionType, DateTimeOffset timestamp, [NotNullWhen(true)] out T? typedEntry) where T : DiscordAuditLogEntry
        {
            typedEntry = entry as T;
            return typedEntry is not null && typedEntry.ActionType == actionType && typedEntry.CreationTimestamp > timestamp;
        }
    }
}
