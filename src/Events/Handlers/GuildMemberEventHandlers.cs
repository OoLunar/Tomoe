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
        IEventHandler<GuildAvailableEventArgs>,
        IEventHandler<GuildCreatedEventArgs>,
        IEventHandler<GuildMembersChunkedEventArgs>
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
    }
}
