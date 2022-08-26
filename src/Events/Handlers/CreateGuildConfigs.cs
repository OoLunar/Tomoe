using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.EventArgs;
using EdgeDB;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OoLunar.Tomoe.Attributes;
using OoLunar.Tomoe.Database;
using Serilog;
using Serilog.Extensions.Logging;

namespace OoLunar.Tomoe.Events.Handlers
{
    public sealed class CreateGuildConfigs
    {
        private IConfiguration Configuration { get; init; }
        private ILogger<CreateGuildConfigs> Logger { get; init; }
        private EdgeDBClient EdgeDbClient { get; init; }
        private CancellationToken CancellationToken { get; init; }

        public CreateGuildConfigs(IConfiguration configuration, ILogger<CreateGuildConfigs> logger, EdgeDBClient database, CancellationTokenSource cancellationTokenSource)
        {
            ArgumentNullException.ThrowIfNull(configuration, nameof(configuration));
            ArgumentNullException.ThrowIfNull(logger, nameof(logger));
            ArgumentNullException.ThrowIfNull(database, nameof(database));

            Configuration = configuration;
            Logger = logger;
            EdgeDbClient = database;
            CancellationToken = cancellationTokenSource.Token;
        }

        [DiscordEventHandler(nameof(DiscordShardedClient.GuildDownloadCompleted))]
        public async Task GuildDownloadedCompleteAsync(DiscordClient _, GuildDownloadCompletedEventArgs eventArgs)
        {
            if (EdgeDbClient == null || Configuration == null)
            {
                Logger.LogCritical("GuildDownloadedCompleteAsync: Database or Configuration is null. Cannot create guild configs for the following guild ids: {GuildIds}", eventArgs.Guilds.Keys);
                return;
            }

            foreach (ulong guildId in eventArgs.Guilds.Keys)
            {
                CancellationTokenSource cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(CancellationToken);
                cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(Configuration.GetValue<int>("discord:guild_creation_timeout")));

                // Adds the guild to the database if it doesn't exist.
                GuildModel? guild = (await QueryBuilder.Select<GuildModel>().Filter(x => x.Id == guildId).Limit(1).ExecuteAsync(EdgeDbClient, Capabilities.ReadOnly, cancellationTokenSource.Token)).FirstOrDefault();
                if (guild == null)
                {
                    if (!cancellationTokenSource.TryReset())
                    {
                        Logger.LogError("GuildDownloadedCompleteAsync: CancellationTokenSource.TryReset() failed. Unable to create a guild model for guild {GuildId}", guildId);
                        return;
                    }
                    // TryReset seems to reset the cancel after timer. https://source.dot.net/#System.Private.CoreLib/CancellationTokenSource.cs,68174c69bd7745b1
                    cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(Configuration.GetValue<int>("discord:guild_creation_timeout")));

                    GuildModel guildModel = new(EdgeDbClient, new SerilogLoggerFactory(Log.Logger).CreateLogger<GuildModel>(), Configuration, guildId);
                    await QueryBuilder.Insert(guildModel).ExecuteAsync(EdgeDbClient, Capabilities.Modifications, cancellationTokenSource.Token);

                    if (!cancellationTokenSource.TryReset())
                    {
                        Logger.LogError("GuildDownloadedCompleteAsync: CancellationTokenSource.TryReset() failed. Failed to add members to guild {GuildId}", guildId);
                        return;
                    }
                    cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(Configuration.GetValue<int>("discord:guild_creation_timeout")));

                    await guildModel.AddMembersAsync(eventArgs.Guilds[guildId].Members.Values, cancellationTokenSource.Token);
                }
            }
        }
    }
}
