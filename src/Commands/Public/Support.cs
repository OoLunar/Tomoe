namespace Tomoe.Commands
{
    using System.Threading.Tasks;
    using DSharpPlus;
    using DSharpPlus.SlashCommands;

    public partial class Public : ApplicationCommandModule
    {
        [SlashCommand("support", "Sends the support Discord invite.")]
        public static async Task Support(InteractionContext context) => await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new()
        {
            // TODO: Grab this from config
            Content = "https://discord.gg/Bsv7zSFygc",
            IsEphemeral = true
        });
    }
}
