using System;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.EventArgs;
using EdgeDB;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OoLunar.Tomoe.Attributes;
using OoLunar.Tomoe.Database;
using OoLunar.Tomoe.Services;
using Serilog;
using Serilog.Extensions.Logging;

namespace OoLunar.Tomoe.Events.Handlers
{
    public sealed class CreateGuildConfigs
    {
        private IConfiguration Configuration { get; init; }
        private ILogger<CreateGuildConfigs> Logger { get; init; }
        private GuildModelResolverService GuildModelResolver { get; init; }
        private EdgeDBClient EdgeDBClient { get; init; }
        private CancellationToken CancellationToken { get; init; }

        public CreateGuildConfigs(IConfiguration configuration, ILogger<CreateGuildConfigs> logger, GuildModelResolverService guildModelResolver, EdgeDBClient edgeDBClient, CancellationTokenSource cancellationTokenSource)
        {
            ArgumentNullException.ThrowIfNull(configuration, nameof(configuration));
            ArgumentNullException.ThrowIfNull(logger, nameof(logger));
            ArgumentNullException.ThrowIfNull(guildModelResolver, nameof(guildModelResolver));
            ArgumentNullException.ThrowIfNull(edgeDBClient, nameof(edgeDBClient));

            Configuration = configuration;
            Logger = logger;
            GuildModelResolver = guildModelResolver;
            EdgeDBClient = edgeDBClient;
            CancellationToken = cancellationTokenSource.Token;
        }

        [DiscordEventHandler(nameof(DiscordShardedClient.GuildDownloadCompleted))]
        public async Task GuildDownloadedCompleteAsync(DiscordClient client, GuildDownloadCompletedEventArgs eventArgs)
        {
            foreach (ulong guildId in eventArgs.Guilds.Keys)
            {
                // Adds the guild to the database if it doesn't exist.
                GuildModel? guild = await GuildModelResolver.GetAsync(guildId);
                if (guild == null)
                {
                    GuildModel guildModel = await GuildModelResolver.InsertAsync(new(EdgeDBClient, new SerilogLoggerFactory(Log.Logger).CreateLogger<GuildModel>(), Configuration, guildId));
                    await guildModel.AddMembersAsync(eventArgs.Guilds[guildId].Members.Values, CancellationToken);
                    Logger.LogInformation("Added guild {GuildId} with {MemberCount:N0} members to the database.", guildId, eventArgs.Guilds[guildId].Members.Count);
                }
                Logger.LogInformation("{GuildId}, shard {ShardId}, {Name}, {MemberCount} member{Pluralized}.", guildId, client.ShardId, eventArgs.Guilds[guildId].Name, eventArgs.Guilds[guildId].MemberCount, eventArgs.Guilds[guildId].MemberCount == 1 ? "" : "s");
            }
        }
    }
}
