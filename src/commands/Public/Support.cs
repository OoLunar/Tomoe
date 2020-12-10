using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace Tomoe.Commands.Public {
    public class Support : BaseCommandModule {
        [Command("support")]
        [Aliases(new string[] { "discord", "guild" })]
        [Description("Sends the support Discord invite.")]
        public async Task Get(CommandContext context) => Tomoe.Program.SendMessage(context, "https://discord.gg/Y6JmYTNcGg");
    }
}