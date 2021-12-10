namespace Tomoe.Commands
{
    using DSharpPlus;
    using DSharpPlus.Entities;
    using DSharpPlus.EventArgs;
    using Humanizer;
    using Microsoft.Extensions.DependencyInjection;
    using Serilog;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Tomoe.Db;

    public partial class Listeners
    {
        private static readonly ILogger logger = Log.ForContext<Listeners>();

        public static async Task GuildDownloadCompleted(DiscordClient discordClient, GuildDownloadCompletedEventArgs guildDownloadCompletedEventArgs)
        {
            int guildCount = Public.TotalMemberCount.Count;
            int memberCount = Public.TotalMemberCount.Values.Sum();
            logger.Information($"Guild download completed! Handling {guildCount} guilds and {memberCount} members, with a total of {discordClient.ShardCount.ToMetric()} shard{(discordClient.ShardCount == 1 ? "" : "s")}!");
            await discordClient.UpdateStatusAsync(new DiscordActivity("for bad things", ActivityType.Watching), UserStatus.Online);

#if !DEBUG
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            Task.Run(async () =>
            {
                using IServiceScope scope = Program.ServiceProvider.CreateScope();
                Database database = scope.ServiceProvider.GetService<Database>();
                IEnumerable<ulong> guildIds = database.GuildConfigs.Select(databaseGuild => databaseGuild.Id);
                foreach (ulong guildId in guildIds)
                {
                    if (guildId != Program.Config.DiscordDebugGuildId)
                    {
                        await Program.Client.GetShard(guildId).Guilds[guildId].BulkOverwriteApplicationCommandsAsync(Array.Empty<DiscordApplicationCommand>());
                    }
                }
            });
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
#endif
        }
    }
}