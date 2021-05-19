namespace Tomoe.Api
{
    using DSharpPlus;
    using DSharpPlus.Entities;
    using Humanizer;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using System.Linq;
    using System.Threading.Tasks;
    using Tomoe.Db;

    public partial class Moderation
    {
        public class Logging
        {
            public static async Task Set(DiscordClient client, ulong discordGuildId, ulong discordUserId, LogType logType, DiscordChannel channel, bool isLoggingEnabled)
            {
                using IServiceScope scope = Program.ServiceProvider.CreateScope();
                Database database = scope.ServiceProvider.GetService<Database>();

                LogSetting logSetting = await database.LogSettings.FirstOrDefaultAsync(logSetting => logSetting.Action == logType && logSetting.GuildId == discordGuildId);
                if (logSetting == null)
                {
                    logSetting = new();
                    logSetting.Action = logType;
                    logSetting.GuildId = discordGuildId;
                }
                logSetting.ChannelId = channel.Id;
                logSetting.IsLoggingEnabled = isLoggingEnabled;
                database.LogSettings.Add(logSetting);
                await ModLog(client, discordGuildId, LogType.ConfigChange, database, $"Logging {logType.Humanize()} => <@{discordUserId}> changed the {logType.Humanize()} log channel to {channel.Mention}");
                await database.SaveChangesAsync();
            }

            public static LogSetting Get(ulong guildId, LogType logType)
            {
                using IServiceScope scope = Program.ServiceProvider.CreateScope();
                Database database = scope.ServiceProvider.GetService<Database>();
                return database.LogSettings.FirstOrDefault(logChannel => logChannel.GuildId == guildId && logChannel.Action == logType);
            }
        }
    }
}