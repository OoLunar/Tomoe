namespace Tomoe.Commands
{
    using DSharpPlus;
    using DSharpPlus.Entities;
    using DSharpPlus.EventArgs;
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
            logger.Information($"Guild download completed! Handling {guildCount} guilds and {memberCount} members!");
            await discordClient.UpdateStatusAsync(new DiscordActivity("for bad things", ActivityType.Watching), UserStatus.Online);
        }
    }
}