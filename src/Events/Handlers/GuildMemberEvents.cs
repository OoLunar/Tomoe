using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using OoLunar.Tomoe.Database.Models;

namespace OoLunar.Tomoe.Events.Handlers
{
    public sealed class GuildMemberEvents
    {
        private readonly ILogger<GuildMemberEvents> logger;
        public GuildMemberEvents(ILogger<GuildMemberEvents> logger) => this.logger = logger ?? NullLogger<GuildMemberEvents>.Instance;

        [DiscordEvent]
        public async Task OnGuildCreateAsync(DiscordClient _, GuildCreateEventArgs eventArgs)
        {
            List<GuildMemberModel> guildMemberModels = [];
            foreach (DiscordMember member in eventArgs.Guild.Members.Values)
            {
                guildMemberModels.Add(new()
                {
                    GuildId = eventArgs.Guild.Id,
                    UserId = member.Id,
                    FirstJoined = member.JoinedAt,
                    State = GuildMemberState.None,
                    RoleIds = member.Roles.Select(x => x.Id).ToList()
                });
            }

            await GuildMemberModel.BulkUpsertAsync(guildMemberModels);
            logger.LogInformation("Guild {GuildId} is now available with {MemberCount:N0} Members", eventArgs.Guild.Id, eventArgs.Guild.MemberCount);
        }

        [DiscordEvent]
        public async Task OnGuildMemberAddAsync(DiscordClient _, GuildMemberAddEventArgs eventArgs)
        {
            GuildMemberModel? guildMemberModel = await GuildMemberModel.FindMemberAsync(eventArgs.Member.Id, eventArgs.Guild.Id);
            if (guildMemberModel is null)
            {
                guildMemberModel = await GuildMemberModel.CreateAsync(eventArgs.Member.Id, eventArgs.Guild.Id, DateTimeOffset.UtcNow, GuildMemberState.None, eventArgs.Member.Roles.Select(x => (long)x.Id).ToList());
                return;
            }

            List<DiscordRole> assignedRoles = new(eventArgs.Member.Roles);
            foreach (ulong roleId in guildMemberModel.RoleIds)
            {
                DiscordRole? role = eventArgs.Guild.GetRole(roleId);
                if (role is null || role.Position >= eventArgs.Guild.CurrentMember.Hierarchy || assignedRoles.Contains(role))
                {
                    // If the role wasn't found or the bot cannot assign it, skip it.
                    continue;
                }

                assignedRoles.Add(role);
            }

            try
            {
                // Assign roles all at once instead of one at a time for ratelimiting purposes.
                // TODO: Configuration setting
                await eventArgs.Member.ReplaceRolesAsync(assignedRoles);
                guildMemberModel.RoleIds = assignedRoles.Select(x => x.Id).ToList();
            }
            catch (Exception error)
            {
                logger.LogError(error, "Failed to assign roles to {Member} in {Guild}", eventArgs.Member, eventArgs.Guild);
            }

            guildMemberModel.State = GuildMemberState.None;
            await guildMemberModel.UpdateAsync();
        }

        [DiscordEvent]
        public static async Task OnGuildMemberRemoveAsync(DiscordClient _, GuildMemberRemoveEventArgs eventArgs)
        {
            GuildMemberModel? guildMemberModel = await GuildMemberModel.FindMemberAsync(eventArgs.Member.Id, eventArgs.Guild.Id);
            if (guildMemberModel is null)
            {
                // If the member doesn't exist, create them with the absent state.
                await GuildMemberModel.CreateAsync(eventArgs.Member.Id, eventArgs.Guild.Id, eventArgs.Member.JoinedAt, GuildMemberState.Absent, eventArgs.Member.Roles.Select(x => (long)x.Id).ToList());
                return;
            }

            // If the member previously existed, set their state to absent.
            guildMemberModel.State |= GuildMemberState.Absent;
            guildMemberModel.RoleIds = eventArgs.Member.Roles.Select(x => x.Id).ToList();
            await guildMemberModel.UpdateAsync();
        }

        [DiscordEvent]
        public static async Task OnGuildMemberUpdateAsync(DiscordClient _, GuildMemberUpdateEventArgs eventArgs)
        {
            GuildMemberModel? guildMemberModel = await GuildMemberModel.FindMemberAsync(eventArgs.Member.Id, eventArgs.Guild.Id);
            if (guildMemberModel is null)
            {
                // If the member doesn't exist, create them with the none state.
                await GuildMemberModel.CreateAsync(eventArgs.Member.Id, eventArgs.Guild.Id, eventArgs.Member.JoinedAt, GuildMemberState.None, eventArgs.Member.Roles.Select(x => (long)x.Id).ToList());
                return;
            }

            // If the member previously existed, update their state.
            guildMemberModel.State = GuildMemberState.None;
            guildMemberModel.RoleIds = eventArgs.Member.Roles.Select(x => x.Id).ToList();
            await guildMemberModel.UpdateAsync();
        }

        [DiscordEvent]
        public static async Task OnGuildMemberChunkAsync(DiscordClient _, GuildMembersChunkEventArgs eventArgs)
        {
            foreach (DiscordMember member in eventArgs.Members)
            {
                GuildMemberModel? guildMemberModel = await GuildMemberModel.FindMemberAsync(member.Id, eventArgs.Guild.Id);
                if (guildMemberModel is null)
                {
                    // If the member doesn't exist, create them with the none state.
                    await GuildMemberModel.CreateAsync(member.Id, eventArgs.Guild.Id, member.JoinedAt, GuildMemberState.None, member.Roles.Select(x => (long)x.Id).ToList());
                    continue;
                }

                // If the member previously existed, update their state.
                guildMemberModel.State = GuildMemberState.None;
                guildMemberModel.RoleIds = member.Roles.Select(x => x.Id).ToList();
                await guildMemberModel.UpdateAsync();
            }
        }

        [DiscordEvent]
        public static async Task OnGuildMemberAddBanAsync(DiscordClient _, GuildBanAddEventArgs eventArgs)
        {
            GuildMemberModel? guildMemberModel = await GuildMemberModel.FindMemberAsync(eventArgs.Member.Id, eventArgs.Guild.Id);
            if (guildMemberModel is null)
            {
                // If the member doesn't exist, create them with the absent and banned state.
                await GuildMemberModel.CreateAsync(eventArgs.Member.Id, eventArgs.Guild.Id, eventArgs.Member.JoinedAt, GuildMemberState.Absent | GuildMemberState.Banned, eventArgs.Member.Roles.Select(x => (long)x.Id).ToList());
                return;
            }

            // If the member previously existed, update their state.
            guildMemberModel.State |= GuildMemberState.Absent | GuildMemberState.Banned;
            guildMemberModel.RoleIds = eventArgs.Member.Roles.Select(x => x.Id).ToList();
            await guildMemberModel.UpdateAsync();
        }

        [DiscordEvent]
        public static async Task OnGuildMemberRemoveBanAsync(DiscordClient _, GuildBanRemoveEventArgs eventArgs)
        {
            GuildMemberModel? guildMemberModel = await GuildMemberModel.FindMemberAsync(eventArgs.Member.Id, eventArgs.Guild.Id);
            if (guildMemberModel is null)
            {
                // If the member doesn't exist, create them with the absent state.
                await GuildMemberModel.CreateAsync(eventArgs.Member.Id, eventArgs.Guild.Id, eventArgs.Member.JoinedAt, GuildMemberState.Absent, eventArgs.Member.Roles.Select(x => (long)x.Id).ToList());
                return;
            }

            // If the member previously existed, update their state.
            guildMemberModel.State &= GuildMemberState.Banned;
            await guildMemberModel.UpdateAsync();
        }
    }
}
