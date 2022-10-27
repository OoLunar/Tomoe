namespace Tomoe.Commands
{
    using System.Threading.Tasks;
    using DSharpPlus;
    using DSharpPlus.SlashCommands;

    public partial class Public : ApplicationCommandModule
    {
        [SlashCommand("source_code", "Sends the source code for Tomoe.")]
        public static async Task SourceCode(InteractionContext context) => await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new()
        {
            Content = Program.Config.RepositoryLink
        });
    }
}
