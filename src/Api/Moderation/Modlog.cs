namespace Tomoe.Api
{
    using DSharpPlus.Entities;
    using DSharpPlus.Exceptions;
    using Microsoft.Extensions.DependencyInjection;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Tomoe.Db;

    public partial class Moderation
    {
        public enum LogType
        {
            Ban,
            Kick,
            Mute,
            Antimeme,
            Voiceban,
            Strike,
            Pardon,
            Restrike,
            Unban,
            Unmute,
            Unantimeme,
            Unvoiceban,
            AutoReactionCreate,
            AutoReactionDelete,
            ReactionRoleCreate,
            ReactionRoleDelete,
            ReactionRoleFix,
            Lock,
            Config,
            CommandExecuted,
            CustomEvent,
            Unknown,
            Reminder,
            None,
            MemberJoined,
            MemberLeft
        }

        public static async Task Modlog(DiscordGuild guild, Dictionary<string, string> parameters, LogType logType = LogType.Unknown, Database database = null)
        {
            database ??= Program.ServiceProvider.CreateScope().ServiceProvider.GetService<Database>();
            LogSetting logSetting = database.LogSettings.FirstOrDefault(logSetting => logSetting.GuildId == guild.Id && logSetting.LogType == logType);
            if (logSetting == null || logSetting.ChannelId == 0)
            {
                logSetting = database.LogSettings.FirstOrDefault(logSetting => logSetting.GuildId == guild.Id && logSetting.LogType == LogType.Unknown);
                if (logSetting == null || logSetting.ChannelId == 0)
                {
                    return;
                }
            }

            DiscordChannel discordChannel = guild.GetChannel(logSetting.ChannelId);
            if (discordChannel == null)
            {
                // If the channel is null, we can assume it's been deleted. But we want to keep the previous message formatting though, so let's just keep it there.
                logSetting.ChannelId = 0;
                return;
            }

            string logMessage = logSetting.Format;
            foreach ((string key, string value) in parameters)
            {
                // Replace "{guildName}" with "ForSaken Borders"
                logMessage = logMessage.Replace($"{{{key}}}", value);
            }

            ModLog modLog = new()
            {
                LogId = database.ModLogs.Count(modLog => modLog.GuildId == guild.Id),
                GuildId = guild.Id,
                LogType = logType,
                Reason = logMessage
            };

            database.ModLogs.Add(modLog);
            await database.SaveChangesAsync();

            if (logSetting != null)
            {
                try
                {
                    DiscordMessageBuilder discordMessageBuilder = new()
                    {
                        Content = logMessage
                    };
                    discordMessageBuilder.WithAllowedMentions(new List<IMention>());
                    await discordChannel.SendMessageAsync(discordMessageBuilder);
                }
                catch (UnauthorizedException) { }
            }
        }
    }
}