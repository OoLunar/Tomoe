using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Serilog;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Tomoe.Commands.Listeners
{
    public class GuildDownloadCompleted
    {
        private static readonly ILogger _logger = Log.ForContext<GuildDownloadCompleted>();
        internal static Dictionary<ulong, int> MemberCount = new();

        /// <summary>
        /// Changes the bot status whenever everything is ready.
        /// </summary>
        /// <param name="client">Used to change the bot's status.</param>
        /// <param name="_eventArgs">Unused <see cref="ReadyEventArgs"/>.</param>
        public static async Task Handler(DiscordClient client, GuildDownloadCompletedEventArgs eventArgs)
        {
            int totalMemberCount = 0;
            foreach (int i in MemberCount.Values)
            {
                totalMemberCount += i;
            }

            _logger.Information($"Ready! Handling {eventArgs.Guilds.Count} guilds and {totalMemberCount} guild members");
            await client.UpdateStatusAsync(new DiscordActivity("for bad things", ActivityType.Watching), UserStatus.Online);
        }
    }
}