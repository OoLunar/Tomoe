using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace Tomoe.Utils {
    public class Events {
        public static async Task OnReady(DiscordClient client, ReadyEventArgs eventArgs) => await client.UpdateStatusAsync(new DiscordActivity("for bad things", ActivityType.Watching), UserStatus.Online);
    }
}