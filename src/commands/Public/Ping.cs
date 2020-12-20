using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

namespace Tomoe.Commands.Public {
    public class Ping : BaseCommandModule {
        private const string _COMMAND_NAME = "ping";
        private const string _COMMAND_DESC = "Checks the latency between the bot and the Discord API Websocket. Best used to see if the bot is lagging.";

        [Command(_COMMAND_NAME), Description(_COMMAND_DESC)]
        public async Task Pong(CommandContext context) => Program.SendMessage(context, $"Pong! Latency is {context.Client.Ping}ms");
    }
}