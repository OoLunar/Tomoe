using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.SlashCommands;
using Tomoe.Commands.Attributes;
using Tomoe.Models;

namespace Tomoe.Commands.Moderation
{
    [SlashCommandGroup("logging", "Handles how logging is done.")]
    public sealed partial class LoggingCommand : ApplicationCommandModule
    {
        [SlashCommandGroup("tomoe", "Handles Tomoe related events for logging.")]
        public sealed partial class TomoeSubCommand : ApplicationCommandModule
        {
            public Database Database { private get; set; } = null!;

            [SlashCommand("change", "Changes where events are logged."), Hierarchy(Permissions.ManageGuild)]
            public async Task ChangeAsync(InteractionContext context, [Option("log_type", "Which event to change.")] CustomEvent logType, [Option("channel", "Where will the new logging messages be sent?")] DiscordChannel channel, [Option("formatted_message", "What message to send. Please read the documentation to know how to use this properly.")] string? formatting = null)
            {
                LogSetting? logSetting = Database.LogSettings.FirstOrDefault(databaseLogSetting => databaseLogSetting.GuildId == context.Guild.Id && databaseLogSetting.CustomEvent == logType);
                if (logSetting is null)
                {
                    if (formatting is null)
                    {
                        await context.EditResponseAsync(new()
                        {
                            Content = $"No formatting was provided and {Formatter.InlineCode(logType.ToString())} was not previously tracked. Nothing has changed.",
                        });
                        return;
                    }

                    logSetting = new(context.Guild.Id, channel.Id, logType, null, formatting, true);
                    Database.LogSettings.Add(logSetting);
                }

                logSetting.ChannelId = channel.Id;
                if (!string.IsNullOrEmpty(formatting))
                {
                    logSetting.Format = formatting;
                }
                await Database.SaveChangesAsync();

                await context.EditResponseAsync(new()
                {
                    Content = $"{channel.Mention} will now log all messages related to the {Formatter.InlineCode(logType.ToString())} event."
                });
            }
        }
    }
}
