using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System.Threading.Tasks;

namespace Tomoe.Commands.Public
{
    public class Support : BaseCommandModule
    {
        [Command("support"), Description("Sends the support Discord invite."), Aliases("discord", "guild")]
        public async Task Overload(CommandContext context) => await Program.SendMessage(context, Formatter.EmbedlessUrl(new("https://discord.gg/Bsv7zSFygc")));
    }
}