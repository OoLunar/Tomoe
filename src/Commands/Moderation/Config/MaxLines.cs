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
        [Command("max_lines")]
        public async Task MaxLines(CommandContext context)
        {
            GuildConfig guildConfig = await Database.GuildConfigs.FirstOrDefaultAsync(guildConfig => guildConfig.Id == context.Guild.Id);
            _ = await Program.SendMessage(context, $"Max Lines Per Message => {guildConfig.MaxLinesPerMessage}. Messages with more than {guildConfig.MaxLinesPerMessage} will be removed.");
        }

        [Command("max_lines"), Aliases("max_line"), RequireUserPermissions(Permissions.ManageMessages), Description("Sets the limit on the max amount of lines allowed on messages.")]
        public async Task MaxLines(CommandContext context, [Description("The maximum amount of lines allowed in a message.")] int maxLineCount)
        {
            GuildConfig guildConfig = await Database.GuildConfigs.FirstOrDefaultAsync(guildConfig => guildConfig.Id == context.Guild.Id);
            guildConfig.MaxLinesPerMessage = maxLineCount;
            await Record(context.Guild, LogType.ConfigChange, Database, $"MaxLines => {context.User.Mention} has changed the max line count to {guildConfig.MaxLinesPerMessage}");
            _ = await Database.SaveChangesAsync();
            _ = await Program.SendMessage(context, $"The maximum lines allowed in a message is now {maxLineCount}.");
        }
    }
}
