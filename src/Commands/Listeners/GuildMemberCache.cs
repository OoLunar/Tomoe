namespace Tomoe.Commands.Listeners
{
    using DSharpPlus;
    using DSharpPlus.Entities;
    using DSharpPlus.EventArgs;
    using Microsoft.Extensions.DependencyInjection;
    using Serilog;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Tomoe.Db;

    public class GuildMemberCache
    {
        private static readonly ILogger logger = Log.ForContext<GuildMemberCache>();

        public static async Task Handler(DiscordClient discordClient, GuildCreateEventArgs guildCreateEventArgs)
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
                GuildMember guildMember = new();
                guildMember.UserId = discordMemberId;
                guildMember.GuildId = guildCreateEventArgs.Guild.Id;

                DiscordMember discordMember = guildCreateEventArgs.Guild.Members[discordMemberId];
                guildMember.Roles.AddRange(discordMember.Roles.Except(new[] { guildCreateEventArgs.Guild.EveryoneRole }).Select(discordRole => discordRole.Id));
                newGuildMembers.Add(guildMember);
            }

            database.GuildMembers.AddRange(newGuildMembers);
            await database.SaveChangesAsync();

            Api.Public.memberCount[guildCreateEventArgs.Guild.Id] += guildCreateEventArgs.Guild.MemberCount;
            logger.Information($"Guild {guildCreateEventArgs.Guild.Name} ({guildCreateEventArgs.Guild.Id}) is ready!");
        }
    }
}
