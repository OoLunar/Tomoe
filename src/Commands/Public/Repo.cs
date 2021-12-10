namespace Tomoe.Commands
{
    using DSharpPlus;
    using DSharpPlus.SlashCommands;
    using System.Threading.Tasks;

    public partial class Public : ApplicationCommandModule
    {
        [SlashCommand("source_code", "Sends the source code for Tomoe.")]
        public static async Task SourceCode(InteractionContext context) => await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new()
        {
            Content = Program.Config.RepositoryLink
        });
    }
}
