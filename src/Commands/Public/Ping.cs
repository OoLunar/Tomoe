namespace Tomoe.Commands.Public
{
    using DSharpPlus.SlashCommands;
    using System.Threading.Tasks;

    public class Ping : SlashCommandModule
    {
        [SlashCommand("ping", "Checks the latency between the bot and the Discord API. Best used to see if the bot is lagging.")]
        public async Task Overload(InteractionContext context) => await Program.SendMessage(context, $"Pong! Latency is {context.Client.Ping}ms");
    }
}
