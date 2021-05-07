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
        [Command("auto_dehoist")]
        public async Task AutoDehoist(CommandContext context)
        {
            GuildConfig guildConfig = await Database.GuildConfigs.FirstOrDefaultAsync(guildConfig => guildConfig.Id == context.Guild.Id);
            _ = await Program.SendMessage(context, $"AutoDehoist => {guildConfig.AutoDehoist}. Hoisted nicknames are {(guildConfig.AutoDehoist ? $"renamed to {Formatter.InlineCode("dehoisted")}" : "kept")}.");
        }

        [Command("auto_dehoist"), Aliases("dehoist"), RequireUserPermissions(Permissions.ManageNicknames), Description("Determines whether nicknames are allowed at the top of the list or not.")]
        public async Task AutoDehoist(CommandContext context, bool isEnabled)
        {
            GuildConfig guildConfig = await Database.GuildConfigs.FirstOrDefaultAsync(guildConfig => guildConfig.Id == context.Guild.Id);
            guildConfig.AutoDehoist = isEnabled;
            await Record(context.Guild, LogType.ConfigChange, Database, $"AutoDehoist => {context.User.Mention} has changed the AutoDehoist policy to {guildConfig.AutoDehoist}");
            _ = await Database.SaveChangesAsync();
            _ = await Program.SendMessage(context, $"Hoisted nicknames will now be {(guildConfig.AutoDehoist ? $"renamed to {Formatter.InlineCode("dehoisted")}" : "kept")}.");
        }
    }
}
