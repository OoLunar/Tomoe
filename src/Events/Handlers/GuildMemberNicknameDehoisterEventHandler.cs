using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Exceptions;
using Microsoft.Extensions.Logging;
using OoLunar.Tomoe.Commands.Moderation;
using OoLunar.Tomoe.Database.Models;

namespace OoLunar.Tomoe.Events.Handlers
{
    public sealed class GuildMemberNicknameDehoisterEventHandler : IEventHandler<GuildMemberUpdatedEventArgs>, IEventHandler<GuildMemberAddedEventArgs>, IEventHandler<GuildAvailableEventArgs>, IEventHandler<GuildCreatedEventArgs>
    {
        private readonly ILogger<GuildMemberNicknameDehoisterEventHandler> _logger;

        public GuildMemberNicknameDehoisterEventHandler(ILogger<GuildMemberNicknameDehoisterEventHandler> logger) => _logger = logger;

        [DiscordEvent(DiscordIntents.Guilds | DiscordIntents.GuildMembers)]
        public async Task HandleEventAsync(DiscordClient sender, GuildMemberUpdatedEventArgs eventArgs)
        {
            // Check to see if we can rename users
            if (!eventArgs.Guild.CurrentMember.Permissions.HasPermission(DiscordPermissions.ManageNicknames)
                // Check to see if the guild has auto-dehoist enabled
                || !await GuildSettingsModel.GetAutoDehoistAsync(eventArgs.Guild.Id))
            {
                return;
            }

            await HandleGuildMemberAsync(eventArgs.Member, await GuildSettingsModel.GetAutoDehoistFormatAsync(eventArgs.Guild.Id) ?? "Dehoisted");
        }

        [DiscordEvent(DiscordIntents.Guilds | DiscordIntents.GuildMembers)]
        public async Task HandleEventAsync(DiscordClient sender, GuildMemberAddedEventArgs eventArgs)
        {
            // Check to see if we can rename users
            if (!eventArgs.Guild.CurrentMember.Permissions.HasPermission(DiscordPermissions.ManageNicknames)
                // Check to see if the guild has auto-dehoist enabled
                || !await GuildSettingsModel.GetAutoDehoistAsync(eventArgs.Guild.Id))
            {
                return;
            }

            await HandleGuildMemberAsync(eventArgs.Member, await GuildSettingsModel.GetAutoDehoistFormatAsync(eventArgs.Guild.Id) ?? "Dehoisted");
        }

        [DiscordEvent(DiscordIntents.Guilds)]
        public async Task HandleEventAsync(DiscordClient sender, GuildAvailableEventArgs eventArgs)
        {
            // Check to see if we can rename users
            if (!eventArgs.Guild.CurrentMember.Permissions.HasPermission(DiscordPermissions.ManageNicknames)
                // Check to see if the guild has auto-dehoist enabled
                || !await GuildSettingsModel.GetAutoDehoistAsync(eventArgs.Guild.Id))
            {
                return;
            }

            string format = await GuildSettingsModel.GetAutoDehoistFormatAsync(eventArgs.Guild.Id) ?? "Dehoisted";
            foreach (DiscordMember member in eventArgs.Guild.Members.Values)
            {
                await HandleGuildMemberAsync(member, format);
            }
        }

        [DiscordEvent(DiscordIntents.Guilds)]
        public async Task HandleEventAsync(DiscordClient sender, GuildCreatedEventArgs eventArgs)
        {
            // Check to see if we can rename users
            if (!eventArgs.Guild.CurrentMember.Permissions.HasPermission(DiscordPermissions.ManageNicknames)
                // Check to see if the guild has auto-dehoist enabled
                || !await GuildSettingsModel.GetAutoDehoistAsync(eventArgs.Guild.Id))
            {
                return;
            }

            string format = await GuildSettingsModel.GetAutoDehoistFormatAsync(eventArgs.Guild.Id) ?? "Dehoisted";
            foreach (DiscordMember member in eventArgs.Guild.Members.Values)
            {
                await HandleGuildMemberAsync(member, format);
            }
        }

        private async ValueTask HandleGuildMemberAsync(DiscordMember member, string format)
        {
            // Check to see if we can and should rename the user
            if (member.Hierarchy >= member.Guild.CurrentMember.Hierarchy || !DehoistCommand.ShouldDehoist(member))
            {
                return;
            }

            // Rename the user
            try
            {
                await member.ModifyAsync(memberEditModel =>
                {
                    memberEditModel.AuditLogReason = "Auto-dehoisted.";
                    memberEditModel.Nickname = DehoistCommand.GetNewDisplayName(member, format);
                });
            }
            catch (DiscordException)
            {
                try
                {
                    await member.ModifyAsync(memberEditModel =>
                    {
                        memberEditModel.AuditLogReason = "Auto-dehoisted.";
                        memberEditModel.Nickname = DehoistCommand.GetNewDisplayName(member, format, true);
                    });
                }
                catch (DiscordException error)
                {
                    _logger.LogError(error, "Failed to dehoist {UserId} in {GuildId}", member.Id, member.Guild.Id);
                }
            }
        }
    }
}
