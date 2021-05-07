namespace Tomoe.Commands.Public
{
    using DSharpPlus.CommandsNext;
    using DSharpPlus.CommandsNext.Attributes;
    using System.Threading.Tasks;

    public class Ping : BaseCommandModule
    {
        [Command("ping"), Description("Checks the latency between the bot and the Discord API Websocket. Best used to see if the bot is lagging.")]
        public async Task Overload(CommandContext context) => await Program.SendMessage(context, $"Pong! Latency is {context.Client.Ping}ms");
    }
}
