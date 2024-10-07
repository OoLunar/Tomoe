using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;
using OoLunar.Tomoe.Database.Models;

namespace OoLunar.Tomoe.Events.Handlers
{
    public sealed class GuildMemberEventHandlers :
        IEventHandler<GuildMemberAddedEventArgs>,
        IEventHandler<GuildAvailableEventArgs>,
        IEventHandler<GuildCreatedEventArgs>,
        IEventHandler<GuildMemberRemovedEventArgs>,
        IEventHandler<GuildMemberUpdatedEventArgs>,
        IEventHandler<GuildMembersChunkedEventArgs>,
        IEventHandler<GuildBanAddedEventArgs>,
        IEventHandler<GuildBanRemovedEventArgs>
    {
        private readonly ILogger<GuildMemberEventHandlers> _logger;

        public GuildMemberEventHandlers(ILogger<GuildMemberEventHandlers> logger) => _logger = logger;

        [DiscordEvent(DiscordIntents.Guilds | DiscordIntents.GuildPresences)]
        public Task HandleEventAsync(DiscordClient sender, GuildAvailableEventArgs eventArgs) => HandleEventAsync(sender, (GuildCreatedEventArgs)eventArgs);

        [DiscordEvent(DiscordIntents.Guilds | DiscordIntents.GuildPresences)]
        public async Task HandleEventAsync(DiscordClient sender, GuildCreatedEventArgs eventArgs)
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
            _logger.LogInformation("Guild {GuildId} is now available with {MemberCount:N0} Members", eventArgs.Guild.Id, eventArgs.Guild.MemberCount);
        }

        [DiscordEvent(DiscordIntents.GuildMembers)]
        public async Task HandleEventAsync(DiscordClient sender, GuildMemberAddedEventArgs eventArgs)
        {
            GuildMemberModel? guildMemberModel = await GuildMemberModel.FindMemberAsync(eventArgs.Member.Id, eventArgs.Guild.Id);
            if (guildMemberModel is null)
            {
                guildMemberModel = await GuildMemberModel.CreateAsync(eventArgs.Member.Id, eventArgs.Guild.Id, DateTimeOffset.UtcNow, GuildMemberState.None, eventArgs.Member.Roles.Select(x => x.Id));
                return;
            }

            List<DiscordRole> assignedRoles = new(eventArgs.Member.Roles);
            foreach (ulong roleId in guildMemberModel.RoleIds)
            {
                DiscordRole? role = eventArgs.Guild.Roles.GetValueOrDefault(roleId);
                if (role is null || role.Position >= eventArgs.Guild.CurrentMember.Hierarchy || role.IsManaged || assignedRoles.Contains(role))
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
                _logger.LogError(error, "Failed to assign roles to {Member} in {Guild}", eventArgs.Member, eventArgs.Guild);
            }

            guildMemberModel.State = GuildMemberState.None;
            await guildMemberModel.UpdateAsync();
        }

        [DiscordEvent(DiscordIntents.GuildMembers)]
        public async Task HandleEventAsync(DiscordClient sender, GuildMemberRemovedEventArgs eventArgs)
        {
            GuildMemberModel? guildMemberModel = await GuildMemberModel.FindMemberAsync(eventArgs.Member.Id, eventArgs.Guild.Id);
            if (guildMemberModel is null)
            {
                // If the member doesn't exist, create them with the absent state.
                await GuildMemberModel.CreateAsync(eventArgs.Member.Id, eventArgs.Guild.Id, eventArgs.Member.JoinedAt, GuildMemberState.Absent, eventArgs.Member.Roles.Select(x => x.Id));
                return;
            }

            // If the member previously existed, set their state to absent.
            guildMemberModel.State |= GuildMemberState.Absent;
            guildMemberModel.RoleIds = eventArgs.Member.Roles.Select(x => x.Id).ToList();
            await guildMemberModel.UpdateAsync();
        }

        [DiscordEvent(DiscordIntents.GuildMembers)]
        public async Task HandleEventAsync(DiscordClient sender, GuildMemberUpdatedEventArgs eventArgs)
        {
            GuildMemberModel? guildMemberModel = await GuildMemberModel.FindMemberAsync(eventArgs.Member.Id, eventArgs.Guild.Id);
            if (guildMemberModel is null)
            {
                // If the member doesn't exist, create them with the none state.
                await GuildMemberModel.CreateAsync(eventArgs.Member.Id, eventArgs.Guild.Id, eventArgs.Member.JoinedAt, GuildMemberState.None, eventArgs.Member.Roles.Select(x => x.Id));
                return;
            }

            // If the member previously existed, update their state.
            guildMemberModel.State = GuildMemberState.None;
            guildMemberModel.RoleIds = eventArgs.Member.Roles.Select(x => x.Id).ToList();
            await guildMemberModel.UpdateAsync();
        }

        [DiscordEvent(DiscordIntents.GuildMembers)]
        public async Task HandleEventAsync(DiscordClient sender, GuildMembersChunkedEventArgs eventArgs)
        {
            foreach (DiscordMember member in eventArgs.Members)
            {
                GuildMemberModel? guildMemberModel = await GuildMemberModel.FindMemberAsync(member.Id, eventArgs.Guild.Id);
                if (guildMemberModel is null)
                {
                    // If the member doesn't exist, create them with the none state.
                    await GuildMemberModel.CreateAsync(member.Id, eventArgs.Guild.Id, member.JoinedAt, GuildMemberState.None, member.Roles.Select(x => x.Id));
                    continue;
                }

                // If the member previously existed, update their state.
                guildMemberModel.State = GuildMemberState.None;
                guildMemberModel.RoleIds = member.Roles.Select(x => x.Id).ToList();
                await guildMemberModel.UpdateAsync();
            }
        }

        [DiscordEvent(DiscordIntents.GuildModeration)]
        public async Task HandleEventAsync(DiscordClient sender, GuildBanAddedEventArgs eventArgs)
        {
            GuildMemberModel? guildMemberModel = await GuildMemberModel.FindMemberAsync(eventArgs.Member.Id, eventArgs.Guild.Id);
            if (guildMemberModel is null)
            {
                // If the member doesn't exist, create them with the absent and banned state.
                await GuildMemberModel.CreateAsync(eventArgs.Member.Id, eventArgs.Guild.Id, eventArgs.Member.JoinedAt, GuildMemberState.Absent | GuildMemberState.Banned, eventArgs.Member.Roles.Select(x => x.Id));
                return;
            }

            // If the member previously existed, update their state.
            guildMemberModel.State |= GuildMemberState.Absent | GuildMemberState.Banned;
            guildMemberModel.RoleIds = eventArgs.Member.Roles.Select(x => x.Id).ToList();
            await guildMemberModel.UpdateAsync();
        }

        [DiscordEvent(DiscordIntents.GuildModeration)]
        public async Task HandleEventAsync(DiscordClient sender, GuildBanRemovedEventArgs eventArgs)
        {
            GuildMemberModel? guildMemberModel = await GuildMemberModel.FindMemberAsync(eventArgs.Member.Id, eventArgs.Guild.Id);
            if (guildMemberModel is null)
            {
                // If the member doesn't exist, create them with the absent state.
                await GuildMemberModel.CreateAsync(eventArgs.Member.Id, eventArgs.Guild.Id, eventArgs.Member.JoinedAt, GuildMemberState.Absent, eventArgs.Member.Roles.Select(x => x.Id));
                return;
            }

            // If the member previously existed, update their state.
            guildMemberModel.State &= GuildMemberState.Banned;
            await guildMemberModel.UpdateAsync();
        }
    }
}
