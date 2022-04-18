using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.EventArgs;
using Humanizer;
using Serilog;
using Tomoe.Attributes;

namespace Tomoe.Events
{
    public class GuildDownloadComplete
    {
        [SubscribeToEvent(nameof(DiscordShardedClient.GuildDownloadCompleted))]
        public static Task GuildDownloadCompleteAsync(DiscordClient client, GuildDownloadCompletedEventArgs ready)
        {
            ILogger logger = Log.Logger.ForContext<UserCache>();
            logger.Information("Guild download completed. Handling {GuildCount} guilds and {MemberCount} members.", Program.Guilds.Count, Program.Guilds.Values.Sum().ToMetric());
            return Task.CompletedTask;
        }
    }
}
