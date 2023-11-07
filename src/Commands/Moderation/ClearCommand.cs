using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandAll.Commands;
using DSharpPlus.CommandAll.Commands.Attributes;
using DSharpPlus.CommandAll.Processors.TextCommands.Attributes;
using DSharpPlus.Entities;

namespace OoLunar.Tomoe.Commands.Moderation
{
    public class ClearCommand
    {
        [Command("clear"), Description("Clears messages from chat.")]
        public static async Task ExecuteAsync(CommandContext context, DiscordMessage firstMessage, DiscordMessage? lastMessage = null, [RemainingText] string? reason = null)
        {
            IEnumerable<DiscordMessage> messages = firstMessage.Channel.GetMessagesAfterAsync(firstMessage.Id).ToBlockingEnumerable().Prepend(firstMessage);
            if (lastMessage is not null)
            {
                messages = messages.OrderBy(x => x.CreationTimestamp).TakeWhile(m => m.Id != lastMessage.Id).Append(lastMessage);
            }

            await firstMessage.Channel.DeleteMessagesAsync(messages, reason ?? "No reason provided.");
            await context.RespondAsync($"{messages.Count():N0} deleted.");
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
