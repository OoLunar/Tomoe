namespace Tomoe.Commands.Listeners
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

    public class PersistentRoles
    {
        public async Task Handler(DiscordClient discordClient, GuildMemberAddEventArgs guildMemberAddEventArgs)
        {
            using IServiceScope scope = Program.ServiceProvider.CreateScope();
            Database database = scope.ServiceProvider.GetService<Database>();
            GuildConfig guild = database.GuildConfigs.First(guild => guild.Id == guildMemberAddEventArgs.Guild.Id);
            GuildMember guildMember = database.GuildMembers.FirstOrDefault(user => user.UserId == guildMemberAddEventArgs.Member.Id && user.GuildId == guildMemberAddEventArgs.Guild.Id);
            bool saveDatabase = false;

            if (guildMember == null)
            {
                guildMember.UserId = guildMemberAddEventArgs.Member.Id;
                guildMember.GuildId = guildMemberAddEventArgs.Guild.Id;
                IEnumerable<ulong> newRoles = guildMemberAddEventArgs.Member.Roles.Except(new[] { guildMemberAddEventArgs.Guild.EveryoneRole }).Select(discordRole => discordRole.Id).Except(guildMember.Roles);
                guildMember.Roles.AddRange(newRoles);
                saveDatabase = true;
            }
            else
            {
                foreach (ulong discordRoleId in guildMember.Roles)
                {
                    DiscordRole discordRole = guildMemberAddEventArgs.Guild.GetRole(discordRoleId);
                    if (discordRole == null)
                    {
                        guildMember.Roles.Remove(discordRoleId);
                        saveDatabase = true;
                    }
                    else
                    {
                        try
                        {
                            await guildMemberAddEventArgs.Member.GrantRoleAsync(discordRole, "Persistent Roles.");
                        }
                        catch (UnauthorizedAccessException) { }
                    }
                }
            }

            if (saveDatabase)
            {
                await database.SaveChangesAsync();
            }

            Api.Public.memberCount[guildMemberAddEventArgs.Guild.Id]++;
        }

        public async Task Handler(DiscordClient discordClient, GuildMemberRemoveEventArgs guildMemberRemoveEventArgs)
        {
            using IServiceScope scope = Program.ServiceProvider.CreateScope();
            Database database = scope.ServiceProvider.GetService<Database>();
            GuildConfig guild = database.GuildConfigs.First(guild => guild.Id == guildMemberRemoveEventArgs.Guild.Id);
            GuildMember guildMember = database.GuildMembers.FirstOrDefault(user => user.UserId == guildMemberRemoveEventArgs.Member.Id && user.GuildId == guildMemberRemoveEventArgs.Guild.Id);
            bool saveDatabase = false;

            if (guildMember == null)
            {
                guildMember.UserId = guildMemberRemoveEventArgs.Member.Id;
                guildMember.GuildId = guildMemberRemoveEventArgs.Guild.Id;
                saveDatabase = true;
            }

            IEnumerable<ulong> newRoles = guildMemberRemoveEventArgs.Member.Roles.Except(new[] { guildMemberRemoveEventArgs.Guild.EveryoneRole }).Select(discordRole => discordRole.Id).Except(guildMember.Roles);
            if (newRoles.Any())
            {
                guildMember.Roles.AddRange(newRoles);
                saveDatabase = true;
            }

            if (saveDatabase)
            {
                await database.SaveChangesAsync();
            }

            Api.Public.memberCount[guildMemberRemoveEventArgs.Guild.Id]--;
        }
    }
}