using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;
using OoLunar.Tomoe.Database.Models;

namespace OoLunar.Tomoe.Events.Handlers
{
    public sealed class GuildMemberAddedEventHandler : IEventHandler<GuildMemberAddedEventArgs>
    {
        private readonly ILogger<GuildMemberAddedEventHandler> _logger;

        public GuildMemberAddedEventHandler(ILogger<GuildMemberAddedEventHandler> logger) => _logger = logger;

        [DiscordEvent(DiscordIntents.GuildMembers)]
        public async Task HandleEventAsync(DiscordClient sender, GuildMemberAddedEventArgs eventArgs)
        {
            await UpdateMemberDatabaseAsync(eventArgs.Member);
            await LogJoinAsync(eventArgs.Member);
        }

        private async ValueTask UpdateMemberDatabaseAsync(DiscordMember member)
        {
            GuildMemberModel? guildMemberModel = await GuildMemberModel.FindMemberAsync(member.Id, member.Guild.Id);
            if (guildMemberModel is null)
            {
                guildMemberModel = await GuildMemberModel.CreateAsync(member.Id, member.Guild.Id, DateTimeOffset.UtcNow, GuildMemberState.None, member.Roles.Select(x => x.Id));
                return;
            }

            List<DiscordRole> assignedRoles = new(member.Roles);
            foreach (ulong roleId in guildMemberModel.RoleIds)
            {
                DiscordRole? role = member.Guild.Roles.GetValueOrDefault(roleId);
                if (role is null || role.Position >= member.Guild.CurrentMember.Hierarchy || role.IsManaged || assignedRoles.Contains(role))
                {
                    // If the role wasn't found or the bot cannot assign it, skip it.
                    continue;
                }

                assignedRoles.Add(role);
            }

            try
            {
                if (await GuildSettingsModel.GetRestoreRolesAsync(member.Guild.Id))
                {
                    // Assign roles all at once instead of one at a time for ratelimiting purposes.
                    await member.ReplaceRolesAsync(assignedRoles);
                }

                guildMemberModel.RoleIds = assignedRoles.Select(x => x.Id).ToList();
            }
            catch (Exception error)
            {
                _logger.LogError(error, "Failed to assign roles to {Member} in {Guild}", member, member.Guild);
            }

            guildMemberModel.State = GuildMemberState.None;
            await guildMemberModel.UpdateAsync();
        }

        private static async ValueTask LogJoinAsync(DiscordMember member)
        {
            // Ensure the guild has logging enabled for the member join event
            if (await GuildLoggingModel.GetLoggingAsync(member.Guild.Id, GuildLoggingType.MemberJoined) is not GuildLoggingModel logging || !logging.Enabled)
            {
                return;
            }

            // Get the channel to log the event in
            DiscordChannel channel = await member.Guild.GetChannelAsync(logging.ChannelId);

            // Get the new member count
            Dictionary<string, string> args = [];
            args["{member_count}"] = (await GuildMemberModel.CountMembersAsync(member.Guild.Id)).ToString("N0", CultureInfo.InvariantCulture);

            // Send the log message
            await LoggingEventHandlers.SendLogMessageAsync(channel, logging, args, member);
        }
    }
}
