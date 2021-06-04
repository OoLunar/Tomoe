namespace Tomoe.Commands.Public
{
    using DSharpPlus;
    using DSharpPlus.SlashCommands;
    using System.Threading.Tasks;

    public class Support : SlashCommandModule
    {
        [SlashCommand("support", "Sends the support Discord invite.")]
        public async Task Overload(InteractionContext context) => await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new()
        {
            Content = Api.Public.Support()
        });
    }
}
