using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace Tomoe.Commands.Public {
    public class Repo : BaseCommandModule {
        [Command("repo")]
        [Aliases(new string[] { "github", "gh", "gitlab", "repository" })]
        [Description("Sends the source code for Tomoe.")]
        public async Task Get(CommandContext context) => Tomoe.Program.SendMessage(context, "https://github.com/OoLunar/Tomoe");
    }
}