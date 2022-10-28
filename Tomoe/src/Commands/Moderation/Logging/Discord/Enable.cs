using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.SlashCommands;
using Tomoe.Commands.Attributes;
using Tomoe.Db;

namespace Tomoe.Commands
{
    public partial class Moderation : ApplicationCommandModule
    {
        public partial class Logging : ApplicationCommandModule
        {
            public partial class Discord : ApplicationCommandModule
            {
                [SlashCommand("enable", "Changes where events are logged."), Hierarchy(Permissions.ManageGuild)]
                public async Task EnableAsync(InteractionContext context, [Option("log_type", "Which event to change.")] DiscordEvent logType)
                {
                    LogSetting logSetting = Database.LogSettings.FirstOrDefault(databaseLogSetting => databaseLogSetting.GuildId == context.Guild.Id && databaseLogSetting.DiscordEvent == logType);
                    if (logSetting == null)
                    {
                        await context.EditResponseAsync(new()
                        {
                            Content = $"Error: The {Formatter.InlineCode(logType.ToString())} event was never setup! Run {Formatter.InlineCode("/logging change")} to do so now.",
                        });
                        return;
                    }
                    else
                    {
                        logSetting.IsLoggingEnabled = true;
                    }
                    await Database.SaveChangesAsync();

                    await context.EditResponseAsync(new()
                    {
                        Content = $"All messages related to the {Formatter.InlineCode(logType.ToString())} event will be logged."
                    });
                }
            }
        }
    }
}
