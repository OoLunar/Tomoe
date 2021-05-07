namespace Tomoe.Commands.Moderation
{
    using DSharpPlus;
    using DSharpPlus.CommandsNext;
    using DSharpPlus.CommandsNext.Attributes;
    using DSharpPlus.Entities;
    using Humanizer;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using System.Threading.Tasks;
    using Tomoe.Db;
    using static Tomoe.Commands.Moderation.ModLogs;

    public class Logging : BaseCommandModule
    {
        [Command("logging"), RequireGuild, RequireUserPermissions(Permissions.ManageChannels), Aliases("logs"), Description("Sets which logs go into what channel.")]
        public async Task ByUser(CommandContext context, LogType logType, DiscordChannel channel, bool isEnabled = false)
        {
            await ByProgram(context, logType, channel, isEnabled);
            _ = await Program.SendMessage(context, $"Event {logType.Humanize()} will now be recorded in channel {channel.Mention}");
        }

        public static async Task ByProgram(CommandContext context, LogType logType, DiscordChannel channel, bool isEnabled)
        {
            using IServiceScope scope = Program.ServiceProvider.CreateScope();
            Database database = scope.ServiceProvider.GetService<Database>();

            LogSetting logSetting = await database.LogSettings.FirstOrDefaultAsync(logSetting => logSetting.Action == logType && logSetting.GuildId == context.Guild.Id);
            if (logSetting == null)
            {
                logSetting = new();
                logSetting.Action = logType;
                logSetting.GuildId = context.Guild.Id;
            }
            logSetting.ChannelId = channel.Id;
            logSetting.IsEnabled = isEnabled;
            database.LogSettings.Add(logSetting);
            await Record(context.Guild, LogType.ConfigChange, database, $"Logging {logType.Humanize()} => {context.User.Mention} changed the {logType.Humanize()} log channel to {channel.Mention}");
            await database.SaveChangesAsync();
        }
    }
}
