using System.Threading.Tasks;
using Discord.Addons.Interactive;
using Discord.Commands;

namespace Tomoe.Commands.Public {
    public class Ping : InteractiveBase {

        /// <summary>
        /// Sends a Discord message, then edits the message with how long it took to send in milliseconds.
        /// <code>
        /// >>ping
        /// </code>
        /// </summary>
        [Command("ping", RunMode = RunMode.Async)]
        [Summary("[Sends a Discord message, then edits the message with how long the original message took to send in milliseconds.](https://github.com/OoLunar/Tomoe/blob/master/docs/public/ping.md)")]
        public async Task ping() {
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();
            Discord.IUserMessage pingMessage = await ReplyAsync("Pong!");
            stopwatch.Stop();
            await pingMessage.ModifyAsync(m => { m.Content = $"Pong! {stopwatch.ElapsedMilliseconds}ms"; });
        }
    }
}