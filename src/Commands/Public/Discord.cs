using System.Threading.Tasks;
using Discord.Addons.Interactive;
using Discord.Commands;

namespace Tomoe.Commands.Public {
    public class Discord : InteractiveBase {

        /// <summary>
        /// Sends a Discord message, then edits the message with how long it took to send in milliseconds.
        /// <code>
        /// >>ping
        /// </code>
        /// </summary>
        [Command("discord", RunMode = RunMode.Async)]
        [Alias(new string[] { "guild", "server" })]
        [Summary("[Sends the Discord invite for the testing/support server.](https://github.com/OoLunar/Tomoe/blob/master/docs/public/ping.md)")]
        [Remarks("Public")]
        public async Task discord() => await ReplyAsync("discord.gg/hJX2Nsq");

    }
}