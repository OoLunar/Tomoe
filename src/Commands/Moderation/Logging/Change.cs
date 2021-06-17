using Humanizer;

namespace Tomoe.Commands
{
    using DSharpPlus;
    using DSharpPlus.Entities;
    using DSharpPlus.SlashCommands;
    using System.Linq;
    using System.Threading.Tasks;
    using Tomoe.Db;

    public partial class Moderation : SlashCommandModule
    {
        [SlashCommandGroup("logging", "Handles how logging is done.")]
        public partial class Logging : SlashCommandModule
        {
            public Database Database { private get; set; }

            [SlashCommand("change", "Changes where events are logged.")]
            public async Task Change(InteractionContext context, [Option("log_type", "Which event to change.")] LogType logType, [Option("channel", "Where will the new logging messages be sent?")] DiscordChannel channel, [Option("formatted_message", "What message to send. Please read the documentation to know how to use this properly.")] string formatting = null)
            {
                LogSetting logSetting = Database.LogSettings.FirstOrDefault(databaseLogSetting => databaseLogSetting.GuildId == context.Guild.Id && databaseLogSetting.LogType == logType);
                if (logSetting == null)
                {
                    logSetting = new()
                    {
                        GuildId = context.Guild.Id,
                        LogType = logType,
                        IsLoggingEnabled = true
                    };
                    Database.LogSettings.Add(logSetting);
                }

                logSetting.ChannelId = channel.Id;
                if (!string.IsNullOrEmpty(formatting))
                {
                    logSetting.Format = formatting;
                }
                await Database.SaveChangesAsync();

                await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new()
                {
                    Content = $"{channel.Mention} will now log all messages related to the {Formatter.InlineCode(logType.ToString())} event."
                });
            }
        }
    }
}