namespace Tomoe.Commands.Moderation
{
    using DSharpPlus;
    using DSharpPlus.CommandsNext;
    using DSharpPlus.CommandsNext.Attributes;
    using DSharpPlus.Entities;
    using Microsoft.EntityFrameworkCore;
    using System.Linq;
    using System.Threading.Tasks;
    using Tomoe.Db;
    using static Tomoe.Commands.Moderation.ModLogs;

    public partial class Config : BaseCommandModule
    {
        [Command("admin_list"), Aliases("staff_list"), Description("Shows which roles are admin roles. They're exempt from all of automod.")]
        public async Task AdminList(CommandContext context)
        {
            GuildConfig guildConfig = await Database.GuildConfigs.FirstOrDefaultAsync(guildConfig => guildConfig.Id == context.Guild.Id);
            await Program.SendMessage(context, $"Admin Roles => {string.Join(", ", guildConfig.AdminRoles.Select(roleId => $"<@&{roleId}>").DefaultIfEmpty("None set"))}.");
        }


        [Command("admin_add"), Aliases("staff_add", "admin", "staff"), RequireUserPermissions(Permissions.ManageGuild), Description("Adds the specified role to the admin list.")]
        public async Task AdminAdd(CommandContext context, [Description("The Discord role to set as admin.")] DiscordRole discordRole)
        {
            GuildConfig guildConfig = await Database.GuildConfigs.FirstOrDefaultAsync(guildConfig => guildConfig.Id == context.Guild.Id);
            if (guildConfig.AdminRoles.Contains(discordRole.Id))
            {
                await Program.SendMessage(context, $"The role {discordRole.Mention} was already on the admin list!");
            }
            else
            {
                guildConfig.AdminRoles.Add(discordRole.Id);
                Database.Entry(guildConfig).State = EntityState.Modified;
                await Record(context.Guild, LogType.ConfigChange, Database, $"Admin Roles => {context.User.Mention} has added the role {discordRole} to the admin list.");
                await Database.SaveChangesAsync();
                await Program.SendMessage(context, $"The role {discordRole.Mention} is now considered an admin.");
            }
        }

        [Command("admin_remove"), Aliases("staff_remove", "unadmin", "unstaff"), RequireUserPermissions(Permissions.ManageGuild), Description("Removes the specified role from the admin list.")]
        public async Task AdminRemove(CommandContext context, [Description("The Discord role to remove from admin.")] DiscordRole discordRole)
        {
            GuildConfig guildConfig = await Database.GuildConfigs.FirstOrDefaultAsync(guildConfig => guildConfig.Id == context.Guild.Id);

            if (guildConfig.AdminRoles.Remove(discordRole.Id))
            {
                Database.Entry(guildConfig).State = EntityState.Modified;
                await Record(context.Guild, LogType.ConfigChange, Database, $"Admin Roles => {context.User.Mention} has removed the role {discordRole} from the admin list.");
                await Database.SaveChangesAsync();
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
            GuildConfig guildConfig = await Database.GuildConfigs.FirstOrDefaultAsync(guildConfig => guildConfig.Id == context.Guild.Id);
            guildConfig.AdminRoles.Clear();
            Database.Entry(guildConfig).State = EntityState.Modified;
            await Record(context.Guild, LogType.ConfigChange, Database, $"Admin Roles => {context.User.Mention} cleared all admin roles from the admin list.");
            await Database.SaveChangesAsync();
            await Program.SendMessage(context, "All admin roles have been cleared from the admin list!");
        }
    }
}
