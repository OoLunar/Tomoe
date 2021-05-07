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
        [Command("anti_invite")]
        public async Task AntiInvite(CommandContext context)
        {
            GuildConfig guildConfig = await Database.GuildConfigs.FirstOrDefaultAsync(guildConfig => guildConfig.Id == context.Guild.Id);
            _ = await Program.SendMessage(context, $"AntiInvite => {guildConfig.AntiInvite}. Invites are {(guildConfig.AntiInvite ? "removed" : "kept")} when posted.");
        }

        [Command("anti_invite"), Aliases("remove_invites"), RequireUserPermissions(Permissions.ManageMessages), Description("Determines whether invites are allowed to be posted or not.")]
        public async Task AntiInvite(CommandContext context, bool isEnabled)
        {
            GuildConfig guildConfig = await Database.GuildConfigs.FirstOrDefaultAsync(guildConfig => guildConfig.Id == context.Guild.Id);
            guildConfig.AntiInvite = isEnabled;
            await Record(context.Guild, LogType.ConfigChange, Database, $"AntiInvite => {context.User.Mention} has changed the AntiInvite policy to {guildConfig.AntiInvite}");
            _ = await Database.SaveChangesAsync();
            _ = await Program.SendMessage(context, $"Invites will now be {(guildConfig.AntiInvite ? "removed" : "kept")} when posted.");
        }
    }
}
