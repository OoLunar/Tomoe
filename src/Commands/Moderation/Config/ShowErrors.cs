namespace Tomoe.Commands.Moderation
{
    using DSharpPlus;
    using DSharpPlus.CommandsNext;
    using DSharpPlus.CommandsNext.Attributes;
    using Microsoft.EntityFrameworkCore;
    using System.Threading.Tasks;
    using Tomoe.Db;
    using static Tomoe.Commands.Moderation.ModLogs;

    public partial class Config : BaseCommandModule
    {
        [Command("show_error")]
        public async Task ShowPermissionErrors(CommandContext context)
        {
            GuildConfig guildConfig = await Database.GuildConfigs.FirstOrDefaultAsync(guildConfig => guildConfig.Id == context.Guild.Id);
            _ = await Program.SendMessage(context, $"Show Errors => {guildConfig.ShowPermissionErrors}. Errors will show in the form of {(guildConfig.ShowPermissionErrors ? "messages" : "reactions")}.");
        }

        [Command("show_error"), Aliases("show_errors"), RequireUserPermissions(Permissions.ManageMessages), Description("When enabled, commands that fail for any reason will use a reaction instead of a message to show failure.")]
        public async Task ShowPermissionErrors(CommandContext context, bool isEnabled)
        {
            GuildConfig guildConfig = await Database.GuildConfigs.FirstOrDefaultAsync(guildConfig => guildConfig.Id == context.Guild.Id);
            guildConfig.ShowPermissionErrors = isEnabled;
            await Record(context.Guild, LogType.ConfigChange, Database, $"Show Errors => {context.User.Mention} has made permission errors {(guildConfig.ShowPermissionErrors ? "start" : "stop")} showing.");
            _ = await Database.SaveChangesAsync();
            _ = await Program.SendMessage(context, $"Errors will {(guildConfig.ShowPermissionErrors ? "now" : "no longer")} show when the user doesn't have the correct permissions.");
        }
    }
}
