using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Entities.AuditLogs;
using DSharpPlus.EventArgs;
using OoLunar.Tomoe.Database.Models;

namespace OoLunar.Tomoe.Events.Handlers
{
    public sealed class LoggingEventHandlers : IEventHandler<MessageUpdatedEventArgs>, IEventHandler<MessageDeletedEventArgs>
    {
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
            if (eventArgs.Message.Author is null || await GuildLoggingModel.GetLoggingAsync(eventArgs.Guild.Id, GuildLoggingType.MessageDeleted) is not GuildLoggingModel logging || !logging.Enabled)
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
            await SendLogMessageAsync(channel, logging, args, eventArgs.Message.Author);
        }

        internal static async ValueTask SendLogMessageAsync(DiscordChannel channel, GuildLoggingModel logging, IReadOnlyDictionary<string, string> args, DiscordUser user, DiscordUser? moderator = null, string? reason = null)
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

        internal static bool TryFilterAuditLogEntry<T>(DiscordAuditLogEntry entry, DiscordAuditLogActionType actionType, DateTimeOffset timestamp, [NotNullWhen(true)] out T? typedEntry) where T : DiscordAuditLogEntry
        {
            typedEntry = entry as T;
            return typedEntry is not null && typedEntry.ActionType == actionType && typedEntry.CreationTimestamp > timestamp;
        }
    }
}
