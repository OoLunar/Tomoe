namespace Tomoe.Commands
{
    using DSharpPlus;
    using DSharpPlus.Entities;
    using DSharpPlus.EventArgs;
    using Microsoft.Extensions.DependencyInjection;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Tomoe.Db;

    public partial class Listeners
    {
        public static async Task GuildMemberCache(DiscordClient discordClient, GuildCreateEventArgs guildCreateEventArgs)
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
                guildConfig = new();
                guildConfig.Id = guildCreateEventArgs.Guild.Id;
                database.GuildConfigs.Add(guildConfig);
            }

            List<GuildMember> newGuildMembers = new();
            IEnumerable<ulong> discordMembers = database.GuildMembers.Where(databaseGuildMember => databaseGuildMember.GuildId == guildCreateEventArgs.Guild.Id).Select(databaseGuildMember => databaseGuildMember.UserId);
            foreach (ulong discordMemberId in guildCreateEventArgs.Guild.Members.Keys.Except(discordMembers))
            {
                DiscordMember discordMember = guildCreateEventArgs.Guild.Members[discordMemberId];
                GuildMember guildMember = new()
                {
                    UserId = discordMemberId,
                    GuildId = guildCreateEventArgs.Guild.Id,
                    Roles = discordMember.Roles.Except(new[] { guildCreateEventArgs.Guild.EveryoneRole }).Select(discordRole => discordRole.Id).ToList(),
                    JoinedAt = DateTime.UtcNow
                };
                newGuildMembers.Add(guildMember);
            }

            database.GuildMembers.AddRange(newGuildMembers);
            await database.SaveChangesAsync();

            if (!Public.TotalMemberCount.TryAdd(guildCreateEventArgs.Guild.Id, guildCreateEventArgs.Guild.MemberCount))
            {
                Public.TotalMemberCount[guildCreateEventArgs.Guild.Id] += guildCreateEventArgs.Guild.MemberCount;
            }
            logger.Information($"{guildCreateEventArgs.Guild.Name} ({guildCreateEventArgs.Guild.Id}) is ready!");
        }
    }
}
