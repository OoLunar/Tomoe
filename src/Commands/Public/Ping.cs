namespace Tomoe.Commands
{
    using System.Threading.Tasks;
    using DSharpPlus;
    using DSharpPlus.SlashCommands;

    public partial class Public : ApplicationCommandModule
    {
        [SlashCommand("ping", "Checks the latency between the bot and the Discord API. Best used to see if the bot is lagging.")]
        public static async Task Ping(InteractionContext context) => await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new()
        {
            Content = $"Pong! Webhook latency is {context.Client.Ping}ms"
        });
    }
}
