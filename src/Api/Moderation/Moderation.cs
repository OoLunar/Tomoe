namespace Tomoe.Api
{
    using DSharpPlus;
    using DSharpPlus.Entities;
    using DSharpPlus.Exceptions;
    using Humanizer;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Tomoe.Db;

    public partial class Moderation
    {
        public enum RoleAction
        {
            None,
            Antimeme,
            Mute,
            Voiceban
        }

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
            ConfigChange,
            CommandExecuted,
            CustomEvent,
            Unknown,
            Reminder
        }

        public static async Task<bool> Ban(DiscordGuild discordGuild, DiscordUser victim, ulong issuerId, string discordMessageLink, string banReason = Constants.MissingReason)
        {
            bool sentDm = await (await victim.Id.GetMember(discordGuild)).TryDmMember($"You've been banned from {Formatter.Bold(discordGuild.Name)}. Reason: {Formatter.BlockCode(Formatter.Strip(banReason))}Context: {discordMessageLink}");
            await discordGuild.BanMemberAsync(victim.Id, 0, banReason);
            await ModLog(discordGuild, LogType.Ban, null, $"<@{issuerId}> has banned {victim.Mention}{(sentDm ? '.' : " (Failed to dm).")} Reason: {banReason}");
            return sentDm;
        }

        public static async Task<bool> Kick(DiscordGuild discordGuild, DiscordMember victim, ulong issuerId, string discordMessageLink, string kickReason = Constants.MissingReason)
        {
            bool sentDm = await victim.TryDmMember($"You've been kicked from {Formatter.Bold(discordGuild.Name)}. Reason: {Formatter.BlockCode(Formatter.Strip(kickReason))}Context: {discordMessageLink}");
            await victim.RemoveAsync(kickReason);
            await ModLog(discordGuild, LogType.Kick, null, $"<@{issuerId}> has kicked {victim.Mention}{(sentDm ? '.' : " (Failed to dm).")} Reason: {kickReason}");
            return sentDm;
        }

        public static async Task ModLog(DiscordClient discordClient, ulong discordGuildId, LogType logType, Database database = null, string reason = Constants.MissingReason)
        {
            bool saveToDatabase = database == null;
            if (database == null)
            {
                IServiceScope scope = Program.ServiceProvider.CreateScope();
                database = scope.ServiceProvider.GetService<Database>();
            }

            ModLog modLog = new();
            modLog.LogType = logType;
            modLog.GuildId = discordGuildId;
            modLog.LogId = database.ModLogs.Where(log => log.GuildId == discordGuildId).Count() + 1;
            modLog.Reason = reason;
            database.ModLogs.Add(modLog);
            LogSetting logSetting = database.LogSettings.FirstOrDefault(logSetting => logSetting.GuildId == discordGuildId && logSetting.Action == logType) ?? database.LogSettings.FirstOrDefault(logSetting => logSetting.GuildId == discordGuildId && logSetting.Action == LogType.Unknown);
            if (saveToDatabase)
            {
                await database.SaveChangesAsync();
                await database.DisposeAsync();
            }

            if (logSetting == null)
            {
                return;
            }

            DiscordChannel modLogDiscordChannel = null;
            try
            {
                modLogDiscordChannel = await discordClient.GetChannelAsync(logSetting.ChannelId);
            }
            catch (NotFoundException) { }
            catch (BadRequestException) { }

            if (modLogDiscordChannel != null && logSetting.IsLoggingEnabled)
            {
                DiscordMessageBuilder discordMessageBuilder = new();
                discordMessageBuilder.Content = $"**{logType.Humanize(LetterCasing.Title)}**: {reason}";
                discordMessageBuilder.WithAllowedMentions(new List<IMention>());
                discordMessageBuilder.HasTTS(false);
                await modLogDiscordChannel.SendMessageAsync(discordMessageBuilder);
            }
        }

        public static async Task ModLog(DiscordGuild discordGuild, LogType logType, Database database = null, string reason = Constants.MissingReason)
        {
            bool saveToDatabase = database == null;
            if (database == null)
            {
                IServiceScope scope = Program.ServiceProvider.CreateScope();
                database = scope.ServiceProvider.GetService<Database>();
            }

            ModLog modLog = new();
            modLog.LogType = logType;
            modLog.GuildId = discordGuild.Id;
            modLog.LogId = database.ModLogs.Where(log => log.GuildId == discordGuild.Id).Count() + 1;
            modLog.Reason = reason;
            database.ModLogs.Add(modLog);
            LogSetting logSetting = await database.LogSettings.FirstOrDefaultAsync(logSetting => logSetting.GuildId == discordGuild.Id && logSetting.Action == logType) ?? await database.LogSettings.FirstOrDefaultAsync(logSetting => logSetting.GuildId == discordGuild.Id && logSetting.Action == LogType.Unknown);
            if (saveToDatabase)
            {
                await database.SaveChangesAsync();
                await database.DisposeAsync();
            }

            if (logSetting == null)
            {
                return;
            }

            DiscordChannel modLogDiscordChannel = discordGuild.GetChannel(logSetting.ChannelId);
            if (modLogDiscordChannel != null && logSetting.IsLoggingEnabled)
            {
                DiscordMessageBuilder discordMessageBuilder = new();
                discordMessageBuilder.Content = $"**{logType.Humanize(LetterCasing.Title)}**: {reason}";
                discordMessageBuilder.WithAllowedMentions(new List<IMention>());
                discordMessageBuilder.HasTTS(false);
                await modLogDiscordChannel.SendMessageAsync(discordMessageBuilder);
            }
        }

        public static async Task<bool> MuteMemeBan(DiscordGuild discordGuild, DiscordUser victim, ulong issuerId, string punishReason, RoleAction roleAction, string discordMessageLink)
        {
            using IServiceScope scope = Program.ServiceProvider.CreateScope();
            Database database = scope.ServiceProvider.GetService<Database>();
            GuildConfig guildConfig = await database.GuildConfigs.FirstOrDefaultAsync(guildConfig => guildConfig.Id == discordGuild.Id);
            GuildUser databaseVictim = await database.GuildUsers.FirstOrDefaultAsync(guildUser => guildUser.UserId == victim.Id && guildUser.GuildId == discordGuild.Id) ?? new GuildUser(victim.Id);

            DiscordRole discordPunishRole = null;
            string roleNameGrammarized = null;
            string dmMessage = null;
            LogType logType = LogType.Unknown;

            switch (roleAction)
            {
                case RoleAction.Antimeme:
                    discordPunishRole = guildConfig.AntimemeRole.GetRole(discordGuild);
                    if (discordPunishRole == null)
                    {
                        //TODO: Turn this into a MissingRoleException
                        throw new MissingFieldException(Constants.MissingRole);
                    }
                    databaseVictim.IsAntimemed = databaseVictim.IsAntimemed ? throw new ArgumentException(Formatter.Bold("[Error]: User is already antimemed.")) : true;
                    dmMessage = $"You've been antimemed from {Formatter.Bold(discordGuild.Name)}. Reason: {Formatter.BlockCode(Formatter.Strip(punishReason))}Context: {discordMessageLink}\nNote: Antimeme prevents you from reacting to messages, sending embeds, uploading files, streaming to voice channels, and adds the push-to-talk restriction to voice channels.";
                    roleNameGrammarized = "antimemed";
                    logType = LogType.Antimeme;
                    break;
                case RoleAction.Mute:
                    discordPunishRole = guildConfig.MuteRole.GetRole(discordGuild);
                    if (discordPunishRole == null)
                    {
                        throw new MissingFieldException(Constants.MissingRole);
                    }
                    databaseVictim.IsMuted = databaseVictim.IsMuted ? throw new ArgumentException(Formatter.Bold("[Error]: User is already muted.")) : true;
                    dmMessage = $"You've been muted from {Formatter.Bold(discordGuild.Name)}. Reason: {Formatter.BlockCode(Formatter.Strip(punishReason))}Context: {discordMessageLink}\nNote: A mute prevents you from having any sort of interaction with the guild. It makes the entire guild readonly.";
                    roleNameGrammarized = "muted";
                    logType = LogType.Mute;
                    break;
                case RoleAction.Voiceban:
                    discordPunishRole = guildConfig.VoicebanRole.GetRole(discordGuild);
                    if (discordPunishRole == null)
                    {
                        throw new MissingFieldException(Constants.MissingRole);
                    }
                    databaseVictim.IsVoicebanned = databaseVictim.IsVoicebanned ? throw new ArgumentException(Formatter.Bold("[Error]: User is already voicebanned.")) : true;
                    dmMessage = $"You've been voicebanned from {Formatter.Bold(discordGuild.Name)}. Reason: {Formatter.BlockCode(Formatter.Strip(punishReason))}Context: {discordMessageLink}\nNote: A voiceban prevents you from connecting to voice channels.";
                    roleNameGrammarized = "voicebanned";
                    logType = LogType.Voiceban;
                    break;
                default:
                    throw new ArgumentException("Unable to determine which punishment to set.");
            }

            DiscordMember discordVictim = await victim.Id.GetMember(discordGuild);
            bool sentDm = false;
            if (discordVictim != null)
            {
                await discordVictim.GrantRoleAsync(discordPunishRole, punishReason);
                sentDm = await discordVictim.TryDmMember(dmMessage);
            }

            await ModLog(discordGuild, logType, database, $"<@{issuerId}> {roleNameGrammarized} {victim.Mention}{(sentDm ? '.' : " (Failed to dm).")} Reason: {punishReason}");
            await database.SaveChangesAsync();
            return sentDm;
        }

        public static async Task<bool> Unban(DiscordClient client, DiscordGuild discordGuild, ulong victimId, ulong issuerId, string discordMessageLink, string unbanReason = Constants.MissingReason)
        {
            await discordGuild.UnbanMemberAsync(victimId, unbanReason);
            DiscordUser victim = null;
            try
            {
                victim = await client.GetUserAsync(victimId);
            }
            catch (NotFoundException) { }
            bool sentDm = false;
            if (victim != null)
            {
                sentDm = await victim.TryDmMember($"You've been unbanned from {Formatter.Bold(discordGuild.Name)}. Reason: {Formatter.BlockCode(Formatter.Strip(unbanReason))}Context: {discordMessageLink}");
            }
            await ModLog(client, discordGuild.Id, LogType.Unban, null, $"<@{issuerId}> unbanned <@{victimId}>{(sentDm ? '.' : " (Failed to dm).")} Reason: {unbanReason}");
            return sentDm;
        }

        public static async Task<bool> UnmuteMemeBan(DiscordGuild discordGuild, DiscordUser victim, ulong issuerId, string punishReason, RoleAction roleAction, string discordMessageLink)
        {
            using IServiceScope scope = Program.ServiceProvider.CreateScope();
            Database database = scope.ServiceProvider.GetService<Database>();
            GuildConfig guildConfig = await database.GuildConfigs.FirstOrDefaultAsync(guildConfig => guildConfig.Id == discordGuild.Id);
            GuildUser databaseVictim = await database.GuildUsers.FirstOrDefaultAsync(guildUser => guildUser.UserId == victim.Id && guildUser.GuildId == discordGuild.Id) ?? new GuildUser(victim.Id);

            DiscordRole discordPunishRole = null;
            string roleNameGrammarized = null;
            string dmMessage = null;
            LogType logType = LogType.Unknown;

            switch (roleAction)
            {
                case RoleAction.Antimeme:
                    discordPunishRole = guildConfig.AntimemeRole.GetRole(discordGuild);
                    if (discordPunishRole == null)
                    {
                        throw new MissingFieldException(Constants.MissingRole);
                    }
                    databaseVictim.IsAntimemed = !databaseVictim.IsAntimemed ? throw new ArgumentException(Formatter.Bold("[Error]: User isn't antimemed.")) : false;
                    dmMessage = $"Your antimeme has been removed from {Formatter.Bold(discordGuild.Name)}. Reason: {Formatter.BlockCode(Formatter.Strip(punishReason))}Context: {discordMessageLink}\nNote: Antimeme prevents you from reacting to messages, sending embeds, uploading files, streaming to voice channels, and adds the push-to-talk restriction to voice channels.";
                    roleNameGrammarized = "antimemed";
                    logType = LogType.Unantimeme;
                    break;
                case RoleAction.Mute:
                    discordPunishRole = guildConfig.MuteRole.GetRole(discordGuild);
                    if (discordPunishRole == null)
                    {
                        throw new MissingFieldException(Constants.MissingRole);
                    }
                    databaseVictim.IsMuted = !databaseVictim.IsMuted ? throw new ArgumentException(Formatter.Bold("[Error]: User isn't muted.")) : false;
                    dmMessage = $"Your mute has been removed from {Formatter.Bold(discordGuild.Name)}. Reason: {Formatter.BlockCode(Formatter.Strip(punishReason))}Context: {discordMessageLink}\nNote: A mute prevents you from having any sort of interaction with the guild. It makes the entire guild readonly.";
                    roleNameGrammarized = "muted";
                    logType = LogType.Unmute;
                    break;
                case RoleAction.Voiceban:
                    discordPunishRole = guildConfig.VoicebanRole.GetRole(discordGuild);
                    if (discordPunishRole == null)
                    {
                        throw new MissingFieldException(Constants.MissingRole);
                    }
                    databaseVictim.IsVoicebanned = !databaseVictim.IsVoicebanned ? throw new ArgumentException(Formatter.Bold("[Error]: User isn't voicebanned.")) : false;
                    dmMessage = $"You're voiceban has been removed from {Formatter.Bold(discordGuild.Name)}. Reason: {Formatter.BlockCode(Formatter.Strip(punishReason))}Context: {discordMessageLink}\nNote: A voiceban prevents you from connecting to voice channels.";
                    roleNameGrammarized = "voicebanned";
                    logType = LogType.Unvoiceban;
                    break;
                default:
                    throw new ArgumentException("Unable to determine which punishment to set.");
            }

            DiscordMember discordVictim = await victim.Id.GetMember(discordGuild);
            bool sentDm = false;
            if (discordVictim != null)
            {
                await discordVictim.RevokeRoleAsync(discordPunishRole, punishReason);
                sentDm = await discordVictim.TryDmMember(dmMessage);
            }

            await ModLog(discordGuild, logType, database, $"<@{issuerId}> removed {victim.Mention}'s {roleNameGrammarized}{(sentDm ? '.' : " (Failed to dm).")} Reason: {punishReason}");
            await database.SaveChangesAsync();
            return sentDm;
        }
    }
}