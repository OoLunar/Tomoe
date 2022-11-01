using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.SlashCommands;

namespace Tomoe.Commands.Common
{
    public sealed class PingCommand : ApplicationCommandModule
    {
        [SlashCommand("ping", "Checks the latency between the bot and the Discord API. Best used to see if the bot is lagging.")]
        public static Task PingAsync(InteractionContext context) => context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new()
        {
            Content = $"Pong! Webhook latency is {context.Client.Ping}ms"
        });
    }
}
