using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using OoLunar.Tomoe.Commands.Moderation;
using OoLunar.Tomoe.Database.Models;

namespace OoLunar.Tomoe.Events.Handlers
{
    public sealed class GuildMemberNicknameDehoisterEventHandler : IEventHandler<GuildMemberUpdatedEventArgs>
    {
        [DiscordEvent(DiscordIntents.Guilds | DiscordIntents.GuildMembers)]
        public async Task HandleEventAsync(DiscordClient sender, GuildMemberUpdatedEventArgs eventArgs)
        {
            // Check to see if we can rename users
            if (eventArgs.Guild.CurrentMember.Permissions.HasPermission(DiscordPermissions.ManageNicknames)
                // Check to see if we can rename the user
                || eventArgs.Member.Hierarchy >= eventArgs.Guild.CurrentMember.Hierarchy
                // Check to see if the user's nickname needs to be dehoisted
                // By default, Discord sorts by ASCII characters. If the first character is not a letter or number, it is considered a dehoist
                || !DehoistCommand.ShouldDehoist(eventArgs.Member)
                // Check to see if the guild has auto-dehoist enabled
                || !await GuildSettingsModel.GetAutoDehoistAsync(eventArgs.Guild.Id))
            {
                return;
            }

            // Read the format
            string format = await GuildSettingsModel.GetAutoDehoistFormatAsync(eventArgs.Guild.Id) ?? "Dehoisted";

            // Replace the variables within the format
            format = format
                .Replace("{display_name}", eventArgs.Member.DisplayName)
                .Replace("{user_name}", eventArgs.Member.Username);

            // Rename the user
            await eventArgs.Member.ModifyAsync(memberEditModel => memberEditModel.Nickname = format);
        }
    }
}
