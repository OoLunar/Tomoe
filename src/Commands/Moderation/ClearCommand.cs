using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.Processors.TextCommands.Attributes;
using DSharpPlus.Commands.Trees;
using DSharpPlus.Commands.Trees.Attributes;
using DSharpPlus.Entities;

namespace OoLunar.Tomoe.Commands.Moderation
{
    public class ClearCommand
    {
        [Command("clear"), Description("Clears messages from chat."), RequirePermissions(Permissions.ManageMessages)]
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

            await firstMessage.Channel.DeleteMessagesAsync(messages, reason ?? "No reason provided.");
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
