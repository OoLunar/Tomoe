using System;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using EdgeDB;
using Microsoft.Extensions.Logging;
using OoLunar.Tomoe.Database.Models;

namespace OoLunar.Tomoe.Events.Handlers
{
    public sealed class GuildDownloadCompletedHandler
    {
        private readonly ILogger<GuildDownloadCompletedHandler> _logger;
        private readonly EdgeDBClient _edgeDBClient;

        public GuildDownloadCompletedHandler(ILogger<GuildDownloadCompletedHandler> logger, EdgeDBClient edgeDBClient)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _edgeDBClient = edgeDBClient ?? throw new ArgumentNullException(nameof(edgeDBClient));
        }

        [DiscordEvent]
        public async Task OnGuildDownloadCompletedAsync(DiscordClient client, GuildDownloadCompletedEventArgs guildDownloadCompletedEventArgs)
        {
            foreach (DiscordGuild discordGuild in guildDownloadCompletedEventArgs.Guilds.Values)
            {
                GuildModel? guildModel = await GuildModel.FindAsync(discordGuild.Id, _edgeDBClient);
                if (guildModel is null)
                {
                    await GuildModel.CreateAsync(new GuildModel(discordGuild), _edgeDBClient);
                }
                else
                {
                    await GuildModel.UpdateAsync(guildModel, _edgeDBClient);
                }
            }

            _logger.LogInformation("Guild download completed, handling a total of {GuildCount:N0} guilds.", guildDownloadCompletedEventArgs.Guilds.Count);
        }
    }
}
