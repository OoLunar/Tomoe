namespace Tomoe.Commands
{
    using DSharpPlus;
    using DSharpPlus.Entities;
    using DSharpPlus.EventArgs;
    using Humanizer;
    using Serilog;
    using System.Linq;
    using System.Threading.Tasks;

    public partial class Listeners
    {
        private static readonly ILogger logger = Log.ForContext<Listeners>();

        public static async Task GuildDownloadCompleted(DiscordClient discordClient, GuildDownloadCompletedEventArgs guildDownloadCompletedEventArgs)
        {
            int guildCount = Public.TotalMemberCount.Count;
            int memberCount = Public.TotalMemberCount.Values.Sum();
            logger.Information($"Guild download completed! Handling {guildCount} guilds and {memberCount} members, with a total of {discordClient.ShardCount.ToMetric()} shard{(discordClient.ShardCount == 1 ? "" : "s")}!");
            await discordClient.UpdateStatusAsync(new DiscordActivity("for bad things", ActivityType.Watching), UserStatus.Online);
        }
    }
}