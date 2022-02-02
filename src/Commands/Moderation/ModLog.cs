namespace Tomoe.Commands
{
    using DSharpPlus;
    using DSharpPlus.Entities;
    using DSharpPlus.Exceptions;
    using DSharpPlus.SlashCommands;
    using Microsoft.Extensions.DependencyInjection;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Tomoe.Commands.Attributes;
    using Tomoe.Db;

    public partial class Moderation : ApplicationCommandModule
    {
        public enum CustomEvent
        {
            [ChoiceName("Mute")] Mute,
            [ChoiceName("Antimeme")] Antimeme,
            [ChoiceName("Voiceban")] Voiceban,
            [ChoiceName("Strike")] Strike,
            [ChoiceName("Pardon")] Drop,
            [ChoiceName("Restrike")] Restrike,
            [ChoiceName("Unmute")] Unmute,
            [ChoiceName("Unantimeme")] Unantimeme,
            [ChoiceName("Unvoiceban")] Unvoiceban,
            [ChoiceName("AutoReaction Create")] AutoReactionCreate,
            [ChoiceName("AutoReaction Delete")] AutoReactionDelete,
            [ChoiceName("ReactionRole Create")] ReactionRoleCreate,
            [ChoiceName("ReactionRole Delete")] ReactionRoleDelete,
            [ChoiceName("ReactionRole Fix")] ReactionRoleFix,
            [ChoiceName("Lock")] Lock,
            [ChoiceName("Config")] Config,
            [ChoiceName("Command Executed")] CommandExecuted,
            [ChoiceName("Custom Event")] CustomEvent,
            [ChoiceName("Unknown")] Unknown,
            [ChoiceName("Reminder")] Reminder,
            [ChoiceName("None")] None,
            [ChoiceName("Role Creation")] RoleCreation
        }

        public enum DiscordEvent
        {
            Ban,
            Kick,
            Unban,
            MemberJoined,
            MemberLeft,
            MessageCreated,
            MessageDeleted,
            Unknown
        }

        public Database Database { private get; set; }

        [SlashCommand("mod_log", "Adds a new log to the mod log."), Hierarchy(Permissions.ManageMessages)]
        public async Task ModLog(InteractionContext context, [Option("reason", "What to add to the mod_log")] string reason = Constants.MissingReason)
        {
            Database.ModLogs.Add(new()
            {
                GuildId = context.Guild.Id,
                LogType = CustomEvent.CustomEvent,
                LogId = Database.ModLogs.Count() + 1,
                Reason = reason
            });
            await Database.SaveChangesAsync();
            await context.EditResponseAsync(new()
            {
                Content = $"Event has been recorded!\n{reason}"
            });
        }

        public static async Task ModLog(DiscordGuild guild, Dictionary<string, string> parameters, CustomEvent logType = CustomEvent.Unknown, Database database = null, bool saveToDatabase = true)
        {
            database ??= Program.ServiceProvider.CreateScope().ServiceProvider.GetService<Database>();
            LogSetting logSetting = database.LogSettings.FirstOrDefault(logSetting => logSetting.GuildId == guild.Id && logSetting.CustomEvent == logType);
            if (logSetting == null || logSetting.ChannelId == 0)
            {
                logSetting = database.LogSettings.FirstOrDefault(logSetting => logSetting.GuildId == guild.Id && logSetting.CustomEvent == CustomEvent.Unknown);
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
            if (saveToDatabase)
            {
                await database.SaveChangesAsync();
            }

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


        public static async Task ModLog(DiscordGuild guild, Dictionary<string, string> parameters, DiscordEvent logType = DiscordEvent.Unknown, Database database = null)
        {
            database ??= Program.ServiceProvider.CreateScope().ServiceProvider.GetService<Database>();
            LogSetting logSetting = database.LogSettings.FirstOrDefault(logSetting => logSetting.GuildId == guild.Id && logSetting.DiscordEvent == logType);
            if (logSetting == null || logSetting.ChannelId == 0)
            {
                logSetting = database.LogSettings.FirstOrDefault(logSetting => logSetting.GuildId == guild.Id && logSetting.DiscordEvent == DiscordEvent.Unknown);
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
            logMessage = logMessage.Replace("\\n", "\n");
            logMessage = logMessage.Replace("\\t", "  ");

            ModLog modLog = new()
            {
                LogId = database.ModLogs.Count(modLog => modLog.GuildId == guild.Id),
                GuildId = guild.Id,
                DiscordEvent = logType,
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
                    await discordChannel.SendMessageAsync(discordMessageBuilder);
                }
                catch (UnauthorizedException) { }
            }
        }
    }
}