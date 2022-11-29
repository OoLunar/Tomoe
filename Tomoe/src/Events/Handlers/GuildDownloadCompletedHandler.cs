using System;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;

namespace OoLunar.Tomoe.Events.Handler
{
    public sealed class GuildDownloadCompletedHandler
    {
        private readonly ILogger<GuildDownloadCompletedHandler> Logger;

        public GuildDownloadCompletedHandler(ILogger<GuildDownloadCompletedHandler> logger) => Logger = logger ?? throw new ArgumentNullException(nameof(logger));

        [DiscordEvent]
        public Task OnGuildDownloadCompletedAsync(DiscordClient client, GuildDownloadCompletedEventArgs guildDownloadCompletedEventArgs)
        {
            Logger.LogInformation("Guild download completed, handling a total of {GuildCount:N0} guilds.", guildDownloadCompletedEventArgs.Guilds.Count);
            return Task.CompletedTask;
        }
    }
}
