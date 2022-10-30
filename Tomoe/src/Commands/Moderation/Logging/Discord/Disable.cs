using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.SlashCommands;
using Tomoe.Commands.Attributes;
using Tomoe.Db;

namespace Tomoe.Commands.Moderation
{
    public sealed partial class Logging : ApplicationCommandModule
    {
        public sealed partial class Discord : ApplicationCommandModule
        {
            [SlashCommand("disable", "Changes where events are logged."), Hierarchy(Permissions.ManageGuild)]
            public async Task DisableAsync(InteractionContext context, [Option("log_type", "Which event to change.")] DiscordEvent logType)
            {
                LogSetting? logSetting = Database.LogSettings.FirstOrDefault(databaseLogSetting => databaseLogSetting.GuildId == context.Guild.Id && databaseLogSetting.DiscordEvent == logType);
                if (logSetting == null)
                {
                    logSetting = new()
                    {
                        GuildId = context.Guild.Id,
                        ChannelId = 0,
                        Format = "",
                        DiscordEvent = logType,
                        IsLoggingEnabled = false
                    };
                    Database.LogSettings.Add(logSetting);
                }
                else
                {
                    logSetting.IsLoggingEnabled = false;
                }
                await Database.SaveChangesAsync();

                await context.EditResponseAsync(new()
                {
                    Content = $"All messages related to the {Formatter.InlineCode(logType.ToString())} event will no longer be logged."
                });
            }
        }
    }
}
