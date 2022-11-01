#pragma warning disable CS8019 // Unnecessary using directive.
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.SlashCommands;
using Humanizer;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Tomoe.Db;

namespace Tomoe.Commands
{
    public sealed class GuildDownloadCompletedListener
    {
        private static readonly ILogger logger = Log.ForContext<GuildDownloadCompletedListener>();

        public static async Task GuildDownloadCompletedAsync(DiscordClient discordClient, GuildDownloadCompletedEventArgs guildDownloadCompletedEventArgs)
        {
            int guildCount = Program.TotalMemberCount.Count;
            int memberCount = Program.TotalMemberCount.Values.Sum();
            logger.Information($"Guild download completed! Handling {guildCount} guilds and {memberCount} members, with a total of {discordClient.ShardCount.ToMetric()} shard{(discordClient.ShardCount == 1 ? "" : "s")}!");
            await discordClient.UpdateStatusAsync(new DiscordActivity("for bad things", ActivityType.Watching), UserStatus.Online);

#if !DEBUG
            // Run this in the background so it doesn't block the rest of the bot from starting up
            Task.Run(async () =>
            {
                using IServiceScope scope = Program.ServiceProvider.CreateScope();
                Database database = scope.ServiceProvider.GetRequiredService<Database>();
                IEnumerable<ulong> guildIds = database.GuildConfigs.Select(databaseGuild => databaseGuild.Id);
                foreach (ulong guildId in guildIds)
                {
                    if (guildId != Program.Config.DiscordDebugGuildId)
                    {
                        DiscordClient client = Program.Client.GetShard(guildId);
                        if (client is null || !client.Guilds.TryGetValue(guildId, out DiscordGuild? guild))
                        {
                            // The guild was deleted or the bot was kicked. We're going to assume the bot was kicked and will keep the config in the database.
                            continue;
                        }

                        await guild.BulkOverwriteApplicationCommandsAsync(Array.Empty<DiscordApplicationCommand>());
                        database.MenuRoles.RemoveRange(database.MenuRoles.Where(menuRole => !guild.Roles.Keys.Contains(menuRole.RoleId)));
                        await database.SaveChangesAsync();
                    }
                }
            });
#endif
        }
    }
}
