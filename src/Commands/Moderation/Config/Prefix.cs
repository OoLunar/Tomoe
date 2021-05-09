namespace Tomoe.Commands.Moderation
{
    using DSharpPlus;
    using DSharpPlus.CommandsNext;
    using DSharpPlus.CommandsNext.Attributes;
    using Microsoft.EntityFrameworkCore;
    using System.Linq;
    using System.Threading.Tasks;
    using Tomoe.Db;
    using static Tomoe.Commands.Moderation.ModLogs;

    public partial class Config : BaseCommandModule
    {
        [Command("prefixes"), Description("Shows currently allowed prefixes."), Aliases("prefix_add")]
        public async Task Prefixes(CommandContext context)
        {
            GuildConfig guildConfig = await Database.GuildConfigs.FirstOrDefaultAsync(guildConfig => guildConfig.Id == context.Guild.Id);
            await Program.SendMessage(context, $"Prefixes => {string.Join(", ", guildConfig.Prefixes.Select(prefix => Formatter.Sanitize(prefix)).DefaultIfEmpty("None set"))}. Bad messages are {(guildConfig.DeleteBadMessages ? "removed" : "kept")} when posted.");
        }

        [Command("prefix_add"), RequireUserPermissions(Permissions.ManageGuild), Description("Adds a prefix that the bot responds to.")]
        public async Task PrefixAdd(CommandContext context, string prefix)
        {
            GuildConfig guildConfig = await Database.GuildConfigs.FirstOrDefaultAsync(guildConfig => guildConfig.Id == context.Guild.Id);
            guildConfig.Prefixes.Add(prefix);
            Database.Entry(guildConfig).State = EntityState.Modified;
            await Record(context.Guild, LogType.ConfigChange, Database, $"Prefixes => {context.User.Mention} has added `{prefix}` to the prefix list.");
            await Database.SaveChangesAsync();
            await Program.SendMessage(context, $"Added \"{prefix}\" as a prefix!");
        }

        [Command("prefix_remove"), RequireUserPermissions(Permissions.ManageGuild), Description("Removes a prefix that the bot responds to.")]
        public async Task PrefixRemove(CommandContext context, string prefix)
        {
            GuildConfig guildConfig = await Database.GuildConfigs.FirstOrDefaultAsync(guildConfig => guildConfig.Id == context.Guild.Id);

            if (guildConfig.Prefixes.Remove(prefix))
            {
                Database.Entry(guildConfig).State = EntityState.Modified;
                await Record(context.Guild, LogType.ConfigChange, Database, $"Prefixes => {context.User.Mention} has removed `{prefix}` from the prefix list.");
                await Database.SaveChangesAsync();
                await Program.SendMessage(context, $"Removed the \"{prefix}\" prefix!");
            }
            else
            {
                await Program.SendMessage(context, $"\"{prefix}\" was never a prefix!");
            }
        }

        [Command("prefixes_clear"), Aliases("prefix_clear"), RequireUserPermissions(Permissions.ManageGuild), Description("Clears all guild prefixes from the prefix list.")]
        public async Task PrefixClear(CommandContext context)
        {
            GuildConfig guildConfig = await Database.GuildConfigs.FirstOrDefaultAsync(guildConfig => guildConfig.Id == context.Guild.Id);
            guildConfig.Prefixes.Clear();
            Database.Entry(guildConfig).State = EntityState.Modified;
            await Record(context.Guild, LogType.ConfigChange, Database, $"Prefixes => {context.User.Mention} cleared all prefixes from the prefix list.");
            await Database.SaveChangesAsync();
            await Program.SendMessage(context, "All prefixes have been cleared from the prefix list!");
        }
    }
}

