using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.SlashCommands;

namespace Tomoe.Commands.Common
{
    public sealed class SupportCommand : ApplicationCommandModule
    {
        [SlashCommand("support", "Sends the support Discord invite.")]
        public static Task SupportAsync(InteractionContext context) => context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new()
        {
            // TODO: Grab this from config
            Content = "https://discord.gg/Bsv7zSFygc",
            IsEphemeral = true
        });
    }
}
