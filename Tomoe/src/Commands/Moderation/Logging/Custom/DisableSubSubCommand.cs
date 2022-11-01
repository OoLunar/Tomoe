using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.SlashCommands;
using Tomoe.Commands.Attributes;
using Tomoe.Models;

namespace Tomoe.Commands.Moderation
{
    public sealed partial class LoggingCommand : ApplicationCommandModule
    {
        public sealed partial class TomoeSubCommand : ApplicationCommandModule
        {
            [SlashCommand("disable", "Changes where events are logged."), Hierarchy(Permissions.ManageGuild)]
            public async Task DisableAsync(InteractionContext context, [Option("log_type", "Which event to change.")] CustomEvent logType)
            {
                LogSetting? logSetting = Database.LogSettings.FirstOrDefault(databaseLogSetting => databaseLogSetting.GuildId == context.Guild.Id && databaseLogSetting.CustomEvent == logType);
                if (logSetting is null)
                {
                    await context.EditResponseAsync(new()
                    {
                        Content = $"{Formatter.InlineCode(logType.ToString())} was not previously tracked. Nothing has changed.",
                    });
                    return;
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
