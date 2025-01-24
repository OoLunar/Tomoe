using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ArgumentModifiers;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.Trees;
using DSharpPlus.Entities;

namespace OoLunar.Tomoe.Commands.Moderation
{
    /// <summary>
    /// Clear as day.
    /// </summary>
    public static class ClearCommand
    {
        /// <summary>
        /// Removes a range of messages from chat.
        /// </summary>
        /// <remarks>
        /// Cannot delete messages older than 2 weeks.
        /// </remarks>
        /// <param name="firstMessage">Removes any messages after this message.</param>
        /// <param name="lastMessage">Removes any messages before this message.</param>
        /// <param name="reason">Why the messages are being deleted.</param>
        [Command("clear"), Description("Clears messages from chat."), RequirePermissions(DiscordPermission.ManageMessages, DiscordPermission.ReadMessageHistory)]
        public static async ValueTask ExecuteAsync(CommandContext context, DiscordMessage firstMessage, DiscordMessage? lastMessage = null, [RemainingText] string? reason = null)
        {
            List<DiscordMessage> messages = [];
            await foreach (DiscordMessage message in firstMessage.Channel!.GetMessagesAfterAsync(firstMessage.Id))
            {
                messages.Add(message);
                if (message.Id == lastMessage?.Id)
                {
                    break;
                }
            }

            await firstMessage.Channel.DeleteMessagesAsync(messages, $"Requested by {context.Member!.GetDisplayName()} ({context.Member!.Id}): {reason ?? "No reason provided."}");
            await context.RespondAsync($"{messages.Count:N0} messages deleted.");
        }

        //[Command("clear"), Description("Removes messages by links.")]
        //public static async Task ExecuteAsync(CommandContext context, params DiscordMessage[] messages)
        //{
        //    if (messages.DistinctBy(message => message.ChannelId).Count() == 1)
        //    {
        //        await messages[0].Channel.DeleteMessagesAsync(messages);
        //    }
        //    else
        //    {
        //        foreach (DiscordMessage message in messages)
        //        {
        //            await message.DeleteAsync();
        //        }
        //    }
        //
        //    await context.RespondAsync($"{messages.Length:N0} messages deleted.");
        //}
    }
}
