namespace Tomoe.Commands.Moderation
{
    using DSharpPlus;
    using DSharpPlus.CommandsNext;
    using DSharpPlus.CommandsNext.Attributes;
    using DSharpPlus.Entities;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    public partial class Config : BaseCommandModule
    {
        [Command("admin_list"), Aliases("staff_list"), Description("Shows which roles are admin roles. They're exempt from all of automod.")]
        public async Task AdminList(CommandContext context) => await Program.SendMessage(context, $"Admin Roles => {string.Join(", ", ((List<ulong>)Api.Moderation.Config.Get(context.Guild.Id, Api.Moderation.Config.ConfigSetting.AdminRoles)).Select(role => $"<@&{role}>").DefaultIfEmpty("None set"))}.");

        [Command("admin_add"), Aliases("staff_add", "admin", "staff"), RequireUserPermissions(Permissions.ManageGuild), Description("Adds the specified role to the admin list.")]
        public async Task AdminAdd(CommandContext context, [Description("The Discord role to set as admin.")] DiscordRole discordRole)
        {
            await Api.Moderation.Config.AddList(context.Client, context.Guild.Id, context.User.Id, Api.Moderation.Config.ConfigSetting.AdminRoles, discordRole.Id);
            await Program.SendMessage(context, $"The role {discordRole.Mention} is now considered an admin.");
        }

        [Command("admin_remove"), Aliases("staff_remove", "unadmin", "unstaff"), RequireUserPermissions(Permissions.ManageGuild), Description("Removes the specified role from the admin list.")]
        public async Task AdminRemove(CommandContext context, [Description("The Discord role to remove from admin.")] DiscordRole discordRole)
        {
            if (await Api.Moderation.Config.RemoveList(context.Client, context.Guild.Id, context.User.Id, Api.Moderation.Config.ConfigSetting.AdminRoles, discordRole.Id))
            {
                await Program.SendMessage(context, "The role is no longer considered an admin.");
            }
            else
            {
                await Program.SendMessage(context, "The role wasn't on the admin list!");
            }
        }

        [Command("admin_clear"), Aliases("staff_clear"), RequireUserPermissions(Permissions.ManageGuild), Description("Removes all admin roles from the admin list.")]
        public async Task AdminClear(CommandContext context)
        {
            await Api.Moderation.Config.ClearList(context.Client, context.Guild.Id, context.User.Id, Api.Moderation.Config.ConfigSetting.AdminRoles);
            await Program.SendMessage(context, "All admin roles have been cleared from the admin list!");
        }
    }
}
