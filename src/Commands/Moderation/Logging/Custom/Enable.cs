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
                [SlashCommand("enable", "Changes where events are logged."), Hierarchy(Permissions.ManageGuild)]
                public async Task Enable(InteractionContext context, [Option("log_type", "Which event to change.")] CustomEvent logType)
                {
                    LogSetting logSetting = Database.LogSettings.FirstOrDefault(databaseLogSetting => databaseLogSetting.GuildId == context.Guild.Id && databaseLogSetting.CustomEvent == logType);
                    if (logSetting == null)
                    {
                        await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new()
                        {
                            Content = $"Error: The {Formatter.InlineCode(logType.ToString())} event was never setup! Run {Formatter.InlineCode("/logging change")} to do so now.",
                            IsEphemeral = true
                        });
                    }
                    else
                    {
                        logSetting.IsLoggingEnabled = true;
                    }
                    await Database.SaveChangesAsync();

                    await context.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new()
                    {
                        Content = $"All messages related to the {Formatter.InlineCode(logType.ToString())} event will be logged."
                    });
                }
            }
        }
    }
}