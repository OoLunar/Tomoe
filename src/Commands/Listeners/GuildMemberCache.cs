using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.DependencyInjection;
using Tomoe.Db;

namespace Tomoe.Commands
{
    public partial class Listeners
    {
        public static async Task GuildMemberCacheAsync(DiscordClient discordClient, GuildCreateEventArgs guildCreateEventArgs)
        {
            if (guildCreateEventArgs.Guild == null)
            {
                return;
            }

            using IServiceScope scope = Program.ServiceProvider.CreateScope();
            Database database = scope.ServiceProvider.GetService<Database>();

            GuildConfig guildConfig = database.GuildConfigs.FirstOrDefault(databaseGuildConfig => databaseGuildConfig.Id == guildCreateEventArgs.Guild.Id);
            if (guildConfig == null)
            {
                guildConfig = new()
                {
                    Id = guildCreateEventArgs.Guild.Id
                };
                database.GuildConfigs.Add(guildConfig);
            }

            List<DiscordMember> newDiscordMembers = new();
            IEnumerable<ulong> discordMembers = database.GuildMembers.Where(databaseGuildMember => databaseGuildMember.GuildId == guildCreateEventArgs.Guild.Id).Select(databaseGuildMember => databaseGuildMember.UserId);
            foreach (ulong discordMemberId in guildCreateEventArgs.Guild.Members.Keys.Except(discordMembers))
            {
                DiscordMember discordMember = guildCreateEventArgs.Guild.Members[discordMemberId];
                newDiscordMembers.Add(discordMember);
            }

            database.AddGuildMembers(newDiscordMembers);
            await database.SaveChangesAsync();

            if (!Public.TotalMemberCount.TryAdd(guildCreateEventArgs.Guild.Id, guildCreateEventArgs.Guild.MemberCount))
            {
                Public.TotalMemberCount[guildCreateEventArgs.Guild.Id] += guildCreateEventArgs.Guild.MemberCount;
            }
            logger.Information($"{guildCreateEventArgs.Guild.Id}, shard {discordClient.ShardId}, {guildCreateEventArgs.Guild.Name}, {guildCreateEventArgs.Guild.MemberCount} member{(guildCreateEventArgs.Guild.MemberCount == 1 ? "" : "s")}");
        }
    }
}
