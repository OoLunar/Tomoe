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
        private static readonly DatabaseContext Database = Program.ServiceProvider.GetService<DatabaseContext>()!;

        [SubscribeToEvent(nameof(DiscordClient.GuildAvailable))]
        public static Task GuildAvailableAsync(DiscordClient client, GuildCreateEventArgs guildCreateEventArgs)
        {
            Dictionary<ulong, MemberModel> allMemberModels = Database.GuildMembers.AsNoTracking().Where(x => x.GuildId == guildCreateEventArgs.Guild.Id).AsEnumerable().ToDictionary(x => x.UserId, x => x);

            List<MemberModel> updateMembers = new();
            List<MemberModel> addMembers = new();
            // Sync member data
            foreach (DiscordMember discordMember in guildCreateEventArgs.Guild.Members.Values)
            {
                // Update member data
                if (allMemberModels.TryGetValue(discordMember.Id, out MemberModel? memberModel) && memberModel != null)
                {
                    bool updated = false;
                    if (!memberModel.IsInGuild)
                    {
                        memberModel.IsInGuild = true;
                        updated = true;
                    }

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

            List<MemberModel> removeMembers = new();
            foreach (MemberModel memberModel in allMemberModels.Values)
            {
                if (!guildCreateEventArgs.Guild.Members.ContainsKey(memberModel.UserId))
                {
                    memberModel.IsInGuild = false;
                    removeMembers.Add(memberModel);
                }
            }

            Database.GuildMembers.UpdateRange(updateMembers);
            Database.GuildMembers.AddRange(addMembers);
            Database.GuildMembers.RemoveRange(removeMembers);

            ILogger logger = Log.Logger.ForContext<UserCache>();
            if (logger != null)
            {
                logger.Information("{GuildId}, {MemberCount} members, shard {ShardId}, {GuildName}", guildCreateEventArgs.Guild.Id, guildCreateEventArgs.Guild.MemberCount.ToMetric(), client.ShardId, guildCreateEventArgs.Guild.Name);
            }
            Program.Guilds[guildCreateEventArgs.Guild.Id] = guildCreateEventArgs.Guild.MemberCount;
            return Database.SaveChangesAsync();
        }
    }
}
