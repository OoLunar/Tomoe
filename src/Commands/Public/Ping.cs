using System.Threading.Tasks;
using Discord.Addons.Interactive;
using Discord.Commands;

namespace Tomoe.Commands.Public {
    public class Ping : InteractiveBase {
        [Command("ping")]
        public async Task ping() {
            Discord.IUserMessage pingMessage;
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();
            pingMessage = await ReplyAsync("Pong!");
            stopwatch.Stop();
            await pingMessage.ModifyAsync(m => { m.Content = $"Pong! {stopwatch.ElapsedMilliseconds}ms"; });
        }
    }
}