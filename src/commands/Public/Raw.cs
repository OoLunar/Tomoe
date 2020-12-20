using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

namespace Tomoe.Commands.Public {
    public class Raw : BaseCommandModule {
        private const string _COMMAND_NAME = "raw";
        private const string _COMMAND_DESC = "Gets the raw version of the message provided.";
        private const string _ARG_MESSAGE_DESC = "The message id or jumplink to the message.";

        [Command(_COMMAND_NAME), Description(_COMMAND_DESC)]
        [Aliases("source")]
        public async Task Message(CommandContext context, [Description(_ARG_MESSAGE_DESC)] DiscordMessage message) => Program.SendMessage(context, $"\n{message.Content}");
    }
}