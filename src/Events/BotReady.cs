using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.EventArgs;
using Tomoe.Attributes;

namespace Tomoe.Events
{
    public class BotReady
    {
        [SubscribeToEvent(nameof(DiscordShardedClient.Ready))]
        public static Task BotReadyAsync(DiscordClient client, ReadyEventArgs ready)
        {
            Program.BotReady = true;
            return Task.FromResult(0);
        }
    }
}
