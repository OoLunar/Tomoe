namespace Tomoe.Commands
{
    using DSharpPlus;
    using DSharpPlus.SlashCommands;
    using System.Linq;
    using System.Threading.Tasks;
    using Tomoe.Commands.Attributes;
    using Tomoe.Db;

    public partial class Moderation : SlashCommandModule
    {
        public partial class Logging : SlashCommandModule
        {
            public partial class Tomoe : SlashCommandModule
            {
                [SlashCommand("disable", "Changes where events are logged."), Hierarchy(Permissions.ManageGuild)]
                public async Task Disable(InteractionContext context, [Option("log_type", "Which event to change.")] CustomEvent logType)
                {
                    LogSetting logSetting = Database.LogSettings.FirstOrDefault(databaseLogSetting => databaseLogSetting.GuildId == context.Guild.Id && databaseLogSetting.CustomEvent == logType);
                    if (logSetting == null)
                    {
                        logSetting = new()
                        {
                            GuildId = context.Guild.Id,
                            ChannelId = 0,
                            Format = "",
                            CustomEvent = logType,
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
}