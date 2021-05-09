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
        [Command("delete_bad_messages")]
        public async Task DeleteBadMessages(CommandContext context)
        {
            GuildConfig guildConfig = await Database.GuildConfigs.FirstOrDefaultAsync(guildConfig => guildConfig.Id == context.Guild.Id);
            await Program.SendMessage(context, $"Delete Bad Messages => {guildConfig.DeleteBadMessages}. Bad messages are {(guildConfig.DeleteBadMessages ? "removed" : "kept")} when posted.");
        }

        [Command("delete_bad_messages"), Aliases("bad_messages"), RequireUserPermissions(Permissions.ManageMessages), Description("Determines if messages are removed when automod activates.")]
        public async Task DeleteBadMessages(CommandContext context, bool isEnabled)
        {
            GuildConfig guildConfig = await Database.GuildConfigs.FirstOrDefaultAsync(guildConfig => guildConfig.Id == context.Guild.Id);
            guildConfig.DeleteBadMessages = isEnabled;
            await Record(context.Guild, LogType.ConfigChange, Database, $"DeleteBadMessages => {guildConfig.DeleteBadMessages}. {context.User.Mention} has changed the DeleteBadMessages policy to {guildConfig.DeleteBadMessages}.");
            await Database.SaveChangesAsync();
            await Program.SendMessage(context, $"Bad messages will now be {(guildConfig.DeleteBadMessages ? "deleted" : "kept")} when posted.");
        }
    }
}

