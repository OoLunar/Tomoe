using System;
using System.Collections.Generic;
using System.Linq;
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
    /// Clears a range of messages from a channel.
    /// </summary>
    public sealed class ClearCommand : AuditableCommand
    {

        /// <inheritdoc cref="ClearAsync(CommandContext, DiscordMessage, DiscordMessage?, string?)"/>
        [Command("clear")]
        [Description("Clears a range of messages from a channel.")]
        [RequireGuild, RequirePermissions(Permissions.ManageMessages)]
        public Task ClearAsync(CommandContext context, [Description("The message to start clearing messages at.")] DiscordMessage startMessage, [Description("Why the messages are being deleted."), RemainingText] string? reason = null)
            => ClearAsync(context, startMessage, null, reason);

        /// <summary>
        /// Clears a range of messages from a channel.
        /// </summary>
        /// <param name="context">The context that the command was used in.</param>
        /// <param name="startMessage">The message to start clearing messages at (inclusive).</param>
        /// <param name="endMessage">The message to stop clearing messages at (inclusive).</param>
        /// <param name="reason">Why the messages are being deleted.</param>
        [Command("clear")]
        public async Task ClearAsync(CommandContext context, DiscordMessage startMessage, DiscordMessage? endMessage = null, [RemainingText] string? reason = null)
        {
            if (endMessage != null && endMessage.Channel != startMessage.Channel)
            {
                await context.RespondAsync("The two message links are not from the same channel!");
                return;
            }
            else if (endMessage != null && startMessage.CreationTimestamp >= endMessage.CreationTimestamp)
            {
                await context.RespondAsync("The start message is newer than the end message! Please make sure you sent the links in the correct order.");
                return;
            }

            List<DiscordMessage> validMessages = new();
            List<DiscordMessage> skippedMessages = new();
            IEnumerator<DiscordMessage> enumerator = (await startMessage.Channel.GetMessagesAfterAsync(startMessage.Id)).Prepend(startMessage).GetEnumerator();
            while (enumerator.MoveNext())
            {
                DiscordMessage message = enumerator.Current;
                // 30 second leeway due to latency and Discord API being unstable.
                if (message.CreationTimestamp <= DateTimeOffset.UtcNow.AddDays(-14).AddSeconds(30))
                {
                    // Older than two weeks
                    skippedMessages.Add(message);
                    continue;
                }
                else if (message.CreationTimestamp > (endMessage?.Id ?? startMessage.Channel.LastMessageId!.Value).GetSnowflakeTime())
                {
                    // Past the end message
                    continue;
                }

                // We don't need to check if they're already in there because concurrent hash set will just ignore duplicates.
                Audit.AddAffectedUsers(message.Author.Id);
                validMessages.Add(message);
            }

            if (validMessages.Count == 0)
            {
                await context.RespondAsync("There were not any valid messages found. Make sure that the start and end messages are in the same channel, and that none of the messages are older than two weeks.");
            }
            else
            {
                Audit.AddNote($"Skipped {skippedMessages.Count:N0}/{skippedMessages.Count + validMessages.Count:N0} messages due to being older than two weeks.");
                await ClearAsync(context, validMessages, reason);
            }
        }

        [Command("clear")]
        public async Task ClearAsync(CommandContext context, int count, [RemainingText] string? reason = null)
        {
            IReadOnlyList<DiscordMessage> messages = await context.Channel.GetMessagesAsync(count);
            List<DiscordMessage> validMessages = new();
            List<DiscordMessage> skippedMessages = new();

            for (int i = 0; i < messages.Count; i++)
            {
                // 30 second leeway due to latency and Discord API being unstable.
                if (messages[i].CreationTimestamp <= DateTimeOffset.UtcNow.AddDays(-14).AddSeconds(30))
                {
                    // Older than two weeks
                    skippedMessages.Add(messages[i]);
                    continue;
                }

                // We don't need to check if they're already in there because concurrent hash set will just ignore duplicates.
                Audit.AddAffectedUsers(messages[i].Author.Id);
                validMessages.Add(messages[i]);
            }

            Audit.AddNote($"Skipped {skippedMessages.Count:N0}/{messages.Count:N0} messages due to being older than two weeks.");
            await ClearAsync(context, validMessages, reason);
        }

        public async Task ClearAsync(CommandContext context, List<DiscordMessage> validMessages, string? reason = null)
        {
            if (validMessages.Count == 0)
            {
                await context.RespondAsync("There were not any valid messages found. Make sure that the start and end messages are in the same channel, and that none of the messages are older than two weeks.");
                return;
            }

            reason = Audit.SetReason(reason);

            try
            {
                await validMessages[0].Channel.DeleteMessagesAsync(validMessages, reason);
                Audit.Successful = true; // Defaults to false, so we only need to set it to true.
                Audit.AddNote($"Deleted {validMessages.Count:N0} valid messages.");
            }
            catch (DiscordException error)
            {
                Audit.AddNote($"Failed to delete {validMessages.Count:N0} messages, HTTP Error {error.WebResponse.ResponseCode}: {error.JsonMessage}.");
            }

            await context.RespondAsync(string.Join(' ', Audit.Notes));
        }
    }
}
