using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Tomoe.Commands.Moderation
{
    public partial class Config : BaseCommandModule
    {
        [Command("invites")]
        public async Task Invites(CommandContext context) => await Program.SendMessage(context, $"Allowed Invites => {string.Join(", ", ((List<string>)Api.Moderation.Config.Get(context.Guild.Id, Api.Moderation.Config.ConfigSetting.AllowedInvites)).Select(code => $"<https://discord.gg/{code}>").DefaultIfEmpty("None set"))}\nNone of these invites will be deleted when posted.");

        [Command("add_invite"), Aliases("allow_invite"), RequireUserPermissions(Permissions.ManageMessages), Description("Adds a Discord invite to the whitelist. Only effective if `anti_invite` is enabled.")]
        public async Task AddInvite(CommandContext context, [Description("The Discord invite to whitelist.")] DiscordInvite discordInvite)
        {
            await Api.Moderation.Config.AddList(context.Client, context.Guild.Id, context.User.Id, Api.Moderation.Config.ConfigSetting.AllowedInvites, discordInvite.Code);
            await Program.SendMessage(context, $"Invite discord.gg/{discordInvite.Code} is now whitelisted.");
        }

        [Command("remove_invite"), Aliases("delete_invite"), RequireUserPermissions(Permissions.ManageMessages), Description("Removes an invite from the whitelist. Only effective if `anti_invite` is enabled.")]
        public async Task RemoveInvite(CommandContext context, [Description("The Discord invite to whitelist.")] DiscordInvite discordInvite)
        {
            if (await Api.Moderation.Config.RemoveList(context.Client, context.Guild.Id, context.User.Id, Api.Moderation.Config.ConfigSetting.AllowedInvites, discordInvite.Code))
            {
                await Program.SendMessage(context, "Invite has been removed from the whitelist.");
            }
            else
            {
                await Program.SendMessage(context, "Invite was not whitelisted!");
            }
        }

        [Command("clear_invites"), Aliases("invites_clear"), RequireUserPermissions(Permissions.ManageMessages), Description("Removes all invites from the invite whitelist.")]
        public async Task ClearInvites(CommandContext context)
        {
            await Api.Moderation.Config.ClearList(context.Client, context.Guild.Id, context.User.Id, Api.Moderation.Config.ConfigSetting.AllowedInvites);
            await Program.SendMessage(context, "All invites have been cleared from the whitelist!");
        }
    }
}