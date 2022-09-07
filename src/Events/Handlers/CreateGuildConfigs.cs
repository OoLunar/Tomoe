using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OoLunar.Tomoe.Attributes;
using OoLunar.Tomoe.Database;
using OoLunar.Tomoe.Database.Models;
using OoLunar.Tomoe.Services;
using Serilog;
using Serilog.Extensions.Logging;

namespace OoLunar.Tomoe.Events.Handlers
{
    public sealed class CreateGuildConfigs
    {
        private IConfiguration Configuration { get; init; }
        private ILogger<CreateGuildConfigs> Logger { get; init; }
        private DatabaseTracker DatabaseTracker { get; init; }
        private GuildModelResolverService GuildModelResolver { get; init; }

        public CreateGuildConfigs(IConfiguration configuration, ILogger<CreateGuildConfigs> logger, DatabaseTracker databaseTracker, GuildModelResolverService guildModelResolver)
        {
            ArgumentNullException.ThrowIfNull(configuration, nameof(configuration));
            ArgumentNullException.ThrowIfNull(logger, nameof(logger));
            ArgumentNullException.ThrowIfNull(databaseTracker, nameof(databaseTracker));
            ArgumentNullException.ThrowIfNull(guildModelResolver, nameof(guildModelResolver));

            Configuration = configuration;
            Logger = logger;
            DatabaseTracker = databaseTracker;
            GuildModelResolver = guildModelResolver;
        }

        [DiscordEventHandler(nameof(DiscordShardedClient.GuildDownloadCompleted))]
        public async Task GuildDownloadedCompleteAsync(DiscordClient client, GuildDownloadCompletedEventArgs eventArgs)
        {
            SerilogLoggerFactory loggerFactory = new(Log.Logger);
            foreach (ulong guildId in eventArgs.Guilds.Keys)
            {
                // Adds the guild to the database if it doesn't exist.
                GuildModel? guild = await GuildModelResolver.GetAsync(guildId);
                if (guild == null)
                {
                    GuildModel guildModel = await GuildModelResolver.InsertAsync(new(loggerFactory.CreateLogger<GuildModel>(), Configuration, guildId));
                    DatabaseTracker.StartTracking(guildModel);
                    guildModel.AddMembers(eventArgs.Guilds[guildId].Members.Values.Select(member => new GuildMemberModel(loggerFactory.CreateLogger<GuildMemberModel>(), guildModel, member.Id, member.Roles.Select(role => role.Id))));
                    await DatabaseTracker.UpdateAsync<GuildModel>();
                    Logger.LogInformation("Added guild {GuildId} with {MemberCount:N0} members to the database.", guildId, eventArgs.Guilds[guildId].Members.Count);
                }
                Logger.LogInformation("{GuildId}, shard {ShardId}, {Name}, {MemberCount} member{Pluralized}.", guildId, client.ShardId, eventArgs.Guilds[guildId].Name, eventArgs.Guilds[guildId].MemberCount, eventArgs.Guilds[guildId].MemberCount == 1 ? "" : "s");
            }
        }
    }
}
