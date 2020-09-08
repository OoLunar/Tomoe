using System.Threading.Tasks;
using Discord.Addons.Interactive;
using Discord.Commands;

namespace Tomoe.Commands.Public {
    public class Repo : InteractiveBase {

        /// <summary>
        /// Sends the repository for Tomoe.
        /// <code>
        /// >>repo
        /// </code>
        /// </summary>
        [Command("repo", RunMode = RunMode.Async)]
        [Alias(new string[] { "github", "repository", "gitlab", "gh", "gl" })]
        public async Task repo() => await ReplyAsync("<https://gitlab.com/OoLunar/Tomoe>");
    }
}