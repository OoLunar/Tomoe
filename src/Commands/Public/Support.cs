namespace Tomoe.Commands
{
    using DSharpPlus;
    using DSharpPlus.SlashCommands;
    using System.Threading.Tasks;

    public partial class Public : SlashCommandModule
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
