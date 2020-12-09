using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using static Tomoe.ExtensionMethods;

namespace Tomoe.Commands.Public {
    public class Raw : BaseCommandModule {
        [Command("raw")]
        [Description("Gets the raw version of the message provided")]
        public async Task Message(CommandContext context, DiscordMessage message) => Tomoe.Program.SendMessage(context, $"\n{message.Content}", (FilteringAction.CodeBlocksEscape | FilteringAction.AllMentions));
    }
}