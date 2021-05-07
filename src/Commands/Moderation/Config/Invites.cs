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
        [Command("invites"), Aliases("add_invite", "allow_invite")]
        public async Task Invites(CommandContext context)
        {
            GuildConfig guildConfig = await Database.GuildConfigs.FirstOrDefaultAsync(guildConfig => guildConfig.Id == context.Guild.Id);
            _ = await Program.SendMessage(context, $"Allowed Invites => {string.Join(", ", guildConfig.AllowedInvites.Select(code => $"<https://discord.gg/{code}>").DefaultIfEmpty("None set"))}\nNone of these invites will be deleted when posted.");
        }

        [Command("add_invite"), Aliases("allow_invite"), RequireUserPermissions(Permissions.ManageMessages), Description("Adds a Discord invite to the whitelist. Only effective if `anti_invite` is enabled.")]
        public async Task AddInvite(CommandContext context, [Description("The Discord invite to whitelist.")] DiscordInvite discordInvite)
        {
            GuildConfig guildConfig = await Database.GuildConfigs.FirstOrDefaultAsync(guildConfig => guildConfig.Id == context.Guild.Id);
            if (guildConfig.AllowedInvites.Contains(discordInvite.Code))
            {
                _ = await Program.SendMessage(context, $"Invite discord.gg/{discordInvite.Code} was already whitelisted!");
            }
            else
            {
                guildConfig.AllowedInvites.Add(discordInvite.Code);
                Database.Entry(guildConfig).State = EntityState.Modified;
                await Record(context.Guild, LogType.ConfigChange, Database, $"AllowedInvites => {context.User.Mention} has added the invite `discord.gg/{discordInvite.Code}` to the invite whitelist.");
                _ = await Database.SaveChangesAsync();
                _ = await Program.SendMessage(context, $"Invite discord.gg/{discordInvite.Code} is now whitelisted.");
            }
        }

        [Command("remove_invite"), Aliases("delete_invite"), RequireUserPermissions(Permissions.ManageMessages), Description("Removes an invite from the whitelist. Only effective if `anti_invite` is enabled.")]
        public async Task RemoveInvite(CommandContext context, [Description("The Discord invite to whitelist.")] DiscordInvite discordInvite)
        {
            GuildConfig guildConfig = await Database.GuildConfigs.FirstOrDefaultAsync(guildConfig => guildConfig.Id == context.Guild.Id);
            if (guildConfig.AllowedInvites.Remove(discordInvite.Code))
            {
                Database.Entry(guildConfig).State = EntityState.Modified;
                _ = await Database.SaveChangesAsync();
                _ = await Program.SendMessage(context, "Invite has been removed from the whitelist.");
                await Record(context.Guild, LogType.ConfigChange, Database, $"AllowedInvites => {context.User.Mention} has removed the invite `discord.gg/{discordInvite.Code}` from the invite whitelist.");
            }
            else
            {
                _ = await Program.SendMessage(context, "Invite was not whitelisted!");
            }
        }

        [Command("clear_invites"), Aliases("invites_clear"), RequireUserPermissions(Permissions.ManageMessages), Description("Removes all invites from the invite whitelist.")]
        public async Task ClearInvites(CommandContext context)
        {
            GuildConfig guildConfig = await Database.GuildConfigs.FirstOrDefaultAsync(guildConfig => guildConfig.Id == context.Guild.Id);
            guildConfig.AllowedInvites.Clear();
            Database.Entry(guildConfig).State = EntityState.Modified;
            await Record(context.Guild, LogType.ConfigChange, Database, $"AllowedInvites => {context.User.Mention} cleared all invites from the invite whitelist.");
            await Database.SaveChangesAsync();
            await Program.SendMessage(context, "All invites have been cleared from the whitelist!");
        }
    }
}
