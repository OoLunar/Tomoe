using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using OoLunar.Tomoe.Interfaces;

namespace OoLunar.Tomoe.Commands.Moderation
{
    public sealed class ClearCommand : AuditableCommand
    {
        [Command("clear")]
        [Description("Clears the specified number of messages.")]
        [RequireGuild, RequirePermissions(Permissions.ManageMessages)]
        public Task ClearAsync(CommandContext context, DiscordMessage firstMessage, [RemainingText] string? reason = null)
            => ClearAsync(context, firstMessage, null, reason);

        [Command("clear")]
        public async Task ClearAsync(CommandContext context, DiscordMessage firstMessage, DiscordMessage? lastMessage = null, [RemainingText] string? reason = null)
        {
            if (lastMessage != null && lastMessage.Channel != firstMessage.Channel)
            {
                await context.RespondAsync("The two message links are not from the same channel!");
                return;
            }

            IEnumerable<DiscordMessage> messages = (await firstMessage.Channel.GetMessagesAfterAsync(firstMessage.Id)).Prepend(firstMessage).OrderBy(x => x.CreationTimestamp);
            if (lastMessage != null || firstMessage.Channel.LastMessageId.HasValue)
            {
                messages = messages.TakeWhile(m => m.Id != (lastMessage?.Id ?? firstMessage.Channel.LastMessageId));
            }

            // Prune out messages that are older than 2 weeks.
            List<DiscordMessage> messagesToDelete = messages.Where(message => message.CreationTimestamp > DateTimeOffset.UtcNow.AddDays(-14)).ToList();
            if (messagesToDelete.Count == 0)
            {
                await context.RespondAsync("All messages are older than 2 weeks, which means I'm unable to delete them.");
                return;
            }

            reason = Audit.SetReason(reason);
            Audit.AffectedUsers = messages.Select(x => x.Author.Id).Distinct();
            Audit.AddNote($"Deleted {messages.Count():N0} messages.");
            if (messagesToDelete.Count != messages.Count())
            {
                Audit.AddNote($"{messages.Count() - messagesToDelete.Count:N0} messages skipped due to being older than 2 weeks.");
            }

            await firstMessage.Channel.DeleteMessagesAsync(messages, reason);
            Audit.Successful = true;
            await context.RespondAsync(string.Join(' ', Audit.Notes!));
        }
    }
}
