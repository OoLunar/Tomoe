namespace Tomoe.Commands.Public
{
    using DSharpPlus;
    using DSharpPlus.SlashCommands;
    using System.Threading.Tasks;

    public class Support : SlashCommandModule
    {
        [SlashCommand("support", "Sends the support Discord invite.")]
        public async Task Overload(InteractionContext context) => await Program.SendMessage(context, Formatter.EmbedlessUrl(new("https://discord.gg/Bsv7zSFygc")));
    }
}
