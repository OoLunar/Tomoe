namespace Tomoe.Commands.Listeners
{
    using DSharpPlus;
    using DSharpPlus.Entities;
    using DSharpPlus.EventArgs;
    using Serilog;
    using System.Linq;
    using System.Threading.Tasks;

    public class GuildDownloadCompleted
    {
        private static readonly ILogger logger = Log.ForContext<GuildDownloadCompleted>();

        public static async Task Handler(DiscordClient discordClient, GuildDownloadCompletedEventArgs guildDownloadCompletedEventArgs)
        {
            int guildCount = Public.TotalMemberCount.Count;
            int memberCount = Public.TotalMemberCount.Values.Sum();
            logger.Information($"Guild download completed! Handling {guildCount} guilds and {memberCount} members!");
            await discordClient.UpdateStatusAsync(new DiscordActivity("for bad things", ActivityType.Watching), UserStatus.Online);
        }
    }
}