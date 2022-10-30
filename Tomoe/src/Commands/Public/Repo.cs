using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.SlashCommands;

namespace Tomoe.Commands.Common
{
    public sealed class SourceCodeCommand : ApplicationCommandModule
    {
        [SlashCommand("source_code", "Sends the source code for Tomoe.")]
        public static Task SourceCodeAsync(InteractionContext context) => context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new()
        {
            Content = Program.Config.RepositoryLink
        });
    }
}
