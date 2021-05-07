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
        [Command("auto_strike")]
        public async Task StrikeAutomod(CommandContext context)
        {
            GuildConfig guildConfig = await Database.GuildConfigs.FirstOrDefaultAsync(guildConfig => guildConfig.Id == context.Guild.Id);
            _ = await Program.SendMessage(context, $"AutoDehoist => {guildConfig.AutoDehoist}. Hoisted nicknames are {(guildConfig.AutoDehoist ? $"renamed to {Formatter.InlineCode("dehoisted")}" : "kept")}.");
        }

        [Command("auto_strike"), Aliases("auto_strikes"), RequireUserPermissions(Permissions.ManageMessages), Description("Determines whether automod should add a strike to the victim when automodding.")]
        public async Task StrikeAutomod(CommandContext context, bool isEnabled)
        {
            GuildConfig guildConfig = await Database.GuildConfigs.FirstOrDefaultAsync(guildConfig => guildConfig.Id == context.Guild.Id);
            guildConfig.AutoStrikes = isEnabled;
            await Record(context.Guild, LogType.ConfigChange, Database, $"AutoStrikes => {context.User.Mention} has made automod {(guildConfig.AutoStrikes ? "start" : "stop")} issuing strikes.");
            _ = await Database.SaveChangesAsync();
            _ = await Program.SendMessage(context, $"Automod will {(guildConfig.AutoStrikes ? "now" : "no longer")} issue strikes.");
        }
    }
}
