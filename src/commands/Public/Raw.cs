using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using static Tomoe.ExtensionMethods;

namespace Tomoe.Commands.Public {
    public class Raw : BaseCommandModule {
        [Command("raw")]
        [Description("Gets the raw version of the message provided")]
        [Aliases("source")]
        public async Task Message(CommandContext context, [Description("The message id or jumplink to the message.")] DiscordMessage message) => Tomoe.Program.SendMessage(context, $"\n{message.Content}", (FilteringAction.CodeBlocksEscape | FilteringAction.AllMentions));
    }
}