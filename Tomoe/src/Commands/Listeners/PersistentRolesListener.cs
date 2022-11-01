using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Humanizer;
using Microsoft.Extensions.DependencyInjection;
using Tomoe.Commands.Moderation;
using Tomoe.Models;

namespace Tomoe.Commands
{
    public sealed class PersistentRolesListener
    {
        public static async Task PersistentRolesAsync(DiscordClient discordClient, GuildMemberAddEventArgs guildMemberAddEventArgs)
        {
            Program.TotalMemberCount[guildMemberAddEventArgs.Guild.Id]++;

            using IServiceScope scope = Program.ServiceProvider.CreateScope();
            Database database = scope.ServiceProvider.GetRequiredService<Database>();
            GuildConfig guild = database.GuildConfigs.First(guild => guild.Id == guildMemberAddEventArgs.Guild.Id);
            if (guild.PersistentRoles)
            {

                GuildMember? guildMember = database.GuildMembers.FirstOrDefault(user => user.UserId == guildMemberAddEventArgs.Member.Id && user.GuildId == guildMemberAddEventArgs.Guild.Id);

                if (guildMember is null)
                {
                    database.AddGuildMember(guildMemberAddEventArgs.Member);
                }
                else
                {
                    foreach (ulong discordRoleId in guildMember.Roles)
                    {
                        DiscordRole discordRole = guildMemberAddEventArgs.Guild.GetRole(discordRoleId);
                        if (discordRole is null)
                        {
                            guildMember.Roles.Remove(discordRoleId);
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
            }

            Dictionary<string, string> keyValuePairs = new()
            {
                { "guild_name", guildMemberAddEventArgs.Guild.Name },
                { "guild_count", Program.TotalMemberCount[guildMemberAddEventArgs.Guild.Id].ToMetric() },
                { "person_username", guildMemberAddEventArgs.Member.Username },
                { "person_tag", guildMemberAddEventArgs.Member.Discriminator },
                { "person_mention", $"<@{guildMemberAddEventArgs.Member.Id}>" },
                { "person_id", guildMemberAddEventArgs.Member.Id.ToString(CultureInfo.InvariantCulture) }
            };

            await ModLogCommand.ModLogAsync(guildMemberAddEventArgs.Guild, keyValuePairs, Moderation.DiscordEvent.MemberJoined, database);
            await database.SaveChangesAsync();
        }

        public static async Task PersistentRolesAsync(DiscordClient discordClient, GuildMemberRemoveEventArgs guildMemberRemoveEventArgs)
        {
            Program.TotalMemberCount[guildMemberRemoveEventArgs.Guild.Id]--;

            using IServiceScope scope = Program.ServiceProvider.CreateScope();
            Database database = scope.ServiceProvider.GetRequiredService<Database>();
            GuildConfig guild = database.GuildConfigs.First(guild => guild.Id == guildMemberRemoveEventArgs.Guild.Id);
            GuildMember? guildMember = database.GuildMembers.FirstOrDefault(user => user.UserId == guildMemberRemoveEventArgs.Member.Id && user.GuildId == guildMemberRemoveEventArgs.Guild.Id);

            if (guildMember is null)
            {
                database.AddGuildMember(guildMemberRemoveEventArgs.Member);
            }
            else
            {
                IEnumerable<ulong> newRoles = guildMemberRemoveEventArgs.Member.Roles.Except(new[] { guildMemberRemoveEventArgs.Guild.EveryoneRole }).Select(discordRole => discordRole.Id).Except(guildMember.Roles);
                if (newRoles.Any())
                {
                    guildMember.Roles.AddRange(newRoles);
                }
            }

            if (database.ModLogs.Any(x => x.GuildId == guildMemberRemoveEventArgs.Guild.Id && x.DiscordEvent == Moderation.DiscordEvent.MemberLeft))
            {
                Dictionary<string, string> keyValuePairs = new()
                {
                    { "guild_name", guildMemberRemoveEventArgs.Guild.Name },
                    { "guild_count", Program.TotalMemberCount[guildMemberRemoveEventArgs.Guild.Id].ToMetric() },
                    { "person_username", guildMemberRemoveEventArgs.Member.Username },
                    { "person_tag", guildMemberRemoveEventArgs.Member.Discriminator },
                    { "person_mention", guildMemberRemoveEventArgs.Member.Mention },
                    { "person_id", guildMemberRemoveEventArgs.Member.Id.ToString(CultureInfo.InvariantCulture) }
                };

                await ModLogCommand.ModLogAsync(guildMemberRemoveEventArgs.Guild, keyValuePairs, Moderation.DiscordEvent.MemberLeft, database);
            }

            await database.SaveChangesAsync();
        }
    }
}