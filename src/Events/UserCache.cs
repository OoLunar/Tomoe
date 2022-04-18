using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Tomoe.Attributes;
using Tomoe.Models;

namespace Tomoe.Events
{
    public class UserCache
    {
        [SubscribeToEvent(nameof(DiscordClient.GuildAvailable))]
        public static async Task GuildAvailableAsync(DiscordClient client, GuildCreateEventArgs guildCreateEventArgs)
        {
            DatabaseContext database = Program.ServiceProvider.GetService<DatabaseContext>()!;
            Dictionary<ulong, MemberModel> allMemberModels = database.GuildMembers.AsNoTracking().Where(x => x.GuildId == guildCreateEventArgs.Guild.Id).AsEnumerable().ToDictionary(x => x.UserId, x => x);

            List<MemberModel> updateMembers = new();
            List<MemberModel> addMembers = new();
            foreach (DiscordMember discordMember in guildCreateEventArgs.Guild.Members.Values)
            {
                // Update member data
                if (allMemberModels.TryGetValue(discordMember.Id, out MemberModel? memberModel) && memberModel != null)
                {
                    bool updated = false;

                    // Returning member
                    if (!memberModel.IsInGuild)
                    {
                        // If the guild config says to restore roles, do so. If not, update the roles.
                        GuildConfigModel? guildConfig = database.GuildConfigs.FirstOrDefault(x => x.GuildId == guildCreateEventArgs.Guild.Id);
                        if (guildConfig != null && guildConfig.RestoreRoles)
                        {
                            await discordMember.ReplaceRolesAsync(memberModel.Roles.Select(x => guildCreateEventArgs.Guild.Roles[x]).Concat(discordMember.Roles), "Role restoration");
                        }

                        memberModel.IsInGuild = true;
                        updated = true;
                    }

                    // New or updated member
                    IEnumerable<ulong> memberRoles = guildCreateEventArgs.Guild.Members[memberModel.UserId].Roles.OrderBy(x => x.Position).Select(x => x.Id);
                    if (!memberModel.Roles.SequenceEqual(memberRoles))
                    {
                        memberModel.Roles = new(memberRoles);
                        updated = true;
                    }

                    if (updated)
                    {
                        updateMembers.Add(memberModel);
                    }
                }
                // Add new members
                else
                {
                    addMembers.Add(new()
                    {
                        UserId = discordMember.Id,
                        GuildId = guildCreateEventArgs.Guild.Id,
                        Roles = new(discordMember.Roles.Select(x => x.Id)),
                        JoinedAt = discordMember.JoinedAt.UtcDateTime,
                        IsInGuild = true
                    });
                }
            }

            // Remove members that are no longer in the guild
            List<MemberModel> removeMembers = new();
            foreach (MemberModel memberModel in allMemberModels.Values)
            {
                if (!guildCreateEventArgs.Guild.Members.ContainsKey(memberModel.UserId))
                {
                    memberModel.IsInGuild = false;
                    removeMembers.Add(memberModel);
                }
            }

            database.GuildMembers.UpdateRange(updateMembers);
            database.GuildMembers.AddRange(addMembers);
            database.GuildMembers.RemoveRange(removeMembers);

            ILogger logger = Log.Logger.ForContext<UserCache>();
            if (logger != null)
            {
                logger.Information("{GuildId}, {MemberCount} members, shard {ShardId}, {GuildName}", guildCreateEventArgs.Guild.Id, guildCreateEventArgs.Guild.MemberCount.ToMetric(), client.ShardId, guildCreateEventArgs.Guild.Name);
            }
            Program.Guilds[guildCreateEventArgs.Guild.Id] = guildCreateEventArgs.Guild.MemberCount;
            await database.SaveChangesAsync();
        }

        [SubscribeToEvent(nameof(DiscordClient.GuildMemberAdded))]
        public static async Task GuildMemberAddedAsync(DiscordClient client, GuildMemberAddEventArgs guildMemberAddEventArgs)
        {
            DatabaseContext database = Program.ServiceProvider.GetService<DatabaseContext>()!;
            MemberModel? memberModel = database.GuildMembers.FirstOrDefault(x => x.UserId == guildMemberAddEventArgs.Member.Id && x.GuildId == guildMemberAddEventArgs.Guild.Id);
            if (memberModel == null)
            {
                memberModel = new()
                {
                    UserId = guildMemberAddEventArgs.Member.Id,
                    GuildId = guildMemberAddEventArgs.Guild.Id,
                    Roles = new(guildMemberAddEventArgs.Member.Roles.OrderBy(x => x.Position).Select(x => x.Id)),
                    JoinedAt = guildMemberAddEventArgs.Member.JoinedAt.UtcDateTime,
                    IsInGuild = true
                };
            }
            else
            {
                // If the guild config says to restore roles, do so. If not, update the roles.
                GuildConfigModel? guildConfig = database.GuildConfigs.FirstOrDefault(x => x.GuildId == guildMemberAddEventArgs.Guild.Id);
                if (guildConfig != null && guildConfig.RestoreRoles)
                {
                    await guildMemberAddEventArgs.Member.ReplaceRolesAsync(memberModel.Roles.Select(x => guildMemberAddEventArgs.Guild.Roles[x]).Concat(guildMemberAddEventArgs.Member.Roles), "Role restoration");
                }

                memberModel.Roles = new(guildMemberAddEventArgs.Member.Roles.OrderBy(x => x.Position).Select(x => x.Id));
            }

            database.GuildMembers.Add(memberModel);
            Program.Guilds[guildMemberAddEventArgs.Guild.Id]++;
            await database.SaveChangesAsync();
        }

        [SubscribeToEvent(nameof(DiscordClient.GuildMemberRemoved))]
        public static Task GuildMemberRemovedAsync(DiscordClient client, GuildMemberRemoveEventArgs guildMemberRemoveEventArgs)
        {
            DatabaseContext database = Program.ServiceProvider.GetService<DatabaseContext>()!;
            MemberModel? memberModel = database.GuildMembers.FirstOrDefault(x => x.UserId == guildMemberRemoveEventArgs.Member.Id && x.GuildId == guildMemberRemoveEventArgs.Guild.Id);
            memberModel ??= new() // This shouldn't happen but it's here for edge-cases.
            {
                UserId = guildMemberRemoveEventArgs.Member.Id,
                GuildId = guildMemberRemoveEventArgs.Guild.Id,
                Roles = new(guildMemberRemoveEventArgs.Member.Roles.OrderBy(x => x.Position).Select(x => x.Id)),
                JoinedAt = guildMemberRemoveEventArgs.Member.JoinedAt.UtcDateTime,
            };
            memberModel.IsInGuild = false;

            database.Update(memberModel);
            Program.Guilds[guildMemberRemoveEventArgs.Guild.Id]--;
            return database.SaveChangesAsync();
        }

        [SubscribeToEvent(nameof(DiscordClient.GuildMemberUpdated))]
        public static Task GuildMemberUpdatedAsync(DiscordClient client, GuildMemberUpdateEventArgs guildMemberUpdateEventArgs)
        {
            DatabaseContext database = Program.ServiceProvider.GetService<DatabaseContext>()!;
            if (!guildMemberUpdateEventArgs.RolesBefore.SequenceEqual(guildMemberUpdateEventArgs.RolesAfter))
            {
                MemberModel? memberModel = database.GuildMembers.FirstOrDefault(x => x.UserId == guildMemberUpdateEventArgs.Member.Id && x.GuildId == guildMemberUpdateEventArgs.Guild.Id);
                if (memberModel == null)
                {
                    memberModel = new()
                    {
                        UserId = guildMemberUpdateEventArgs.Member.Id,
                        GuildId = guildMemberUpdateEventArgs.Guild.Id,
                        Roles = new(guildMemberUpdateEventArgs.RolesAfter.OrderBy(x => x.Position).Select(x => x.Id)),
                        JoinedAt = guildMemberUpdateEventArgs.Member.JoinedAt.UtcDateTime,
                        IsInGuild = true
                    };
                }
                else
                {
                    memberModel.Roles = new(guildMemberUpdateEventArgs.RolesAfter.OrderBy(x => x.Position).Select(x => x.Id));
                }

                database.Update(memberModel);
                return database.SaveChangesAsync();
            }
            return Task.CompletedTask;
        }

        [SubscribeToEvent(nameof(DiscordClient.GuildMembersChunked))]
        public static async Task GuildMembersChunkAsync(DiscordClient client, GuildMembersChunkEventArgs guildMembersChunkEventArgs)
        {
            DatabaseContext database = Program.ServiceProvider.GetService<DatabaseContext>()!;

            IEnumerable<ulong> chunkMemberIds = guildMembersChunkEventArgs.Members.Select(x => x.Id);
            DiscordMember[] indexedMembers = guildMembersChunkEventArgs.Members.ToArray();
            List<MemberModel> updateMembers = new();

            foreach (MemberModel memberModel in database.GuildMembers.AsNoTracking().Where(x => chunkMemberIds.Contains(x.UserId) && guildMembersChunkEventArgs.Guild.Id == x.GuildId).AsEnumerable())
            {
                DiscordMember discordMember = indexedMembers[memberModel.UserId];
                bool updated = false;

                // Returning member
                if (!memberModel.IsInGuild)
                {
                    // If the guild config says to restore roles, do so. If not, update the roles.
                    GuildConfigModel? guildConfig = database.GuildConfigs.FirstOrDefault(x => x.GuildId == guildMembersChunkEventArgs.Guild.Id);
                    if (guildConfig != null && guildConfig.RestoreRoles)
                    {
                        await discordMember.ReplaceRolesAsync(memberModel.Roles.Select(x => guildMembersChunkEventArgs.Guild.Roles[x]).Concat(discordMember.Roles), "Role restoration");
                    }

                    memberModel.IsInGuild = true;
                    updated = true;
                }

                // New or updated member
                IEnumerable<ulong> memberRoles = guildMembersChunkEventArgs.Guild.Members[memberModel.UserId].Roles.OrderBy(x => x.Position).Select(x => x.Id);
                if (!memberModel.Roles.SequenceEqual(memberRoles))
                {
                    memberModel.Roles = new(memberRoles);
                    updated = true;
                }

                if (updated)
                {
                    updateMembers.Add(memberModel);
                }
            }

            database.UpdateRange(updateMembers);
            await database.SaveChangesAsync();
        }
    }
}
