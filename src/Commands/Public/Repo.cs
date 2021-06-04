namespace Tomoe.Commands.Public
{
    using DSharpPlus;
    using DSharpPlus.SlashCommands;
    using System.Threading.Tasks;

    public class Repository : SlashCommandModule
    {
        [SlashCommand("repository", "Sends the source code for Tomoe.")]
        public async Task Overload(InteractionContext context) => await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new()
        {
            Content = Api.Public.RepositoryLink()
        });
    }
}
