namespace Tomoe.Commands.Moderation
{
    using DSharpPlus;
    using DSharpPlus.CommandsNext;
    using DSharpPlus.CommandsNext.Attributes;
    using DSharpPlus.Entities;
    using Humanizer;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Tomoe.Db;

    public class ModLogs : BaseCommandModule
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
            ReactionRoleCreate,
            LockServer,
            LockChannel,
            LockRole,
            LockBots,
            AutoReactionDelete,
            ReactionRoleDelete,
            UnlockServer,
            UnlockChannel,
            UnlockRole,
            UnlockBots,
            ReactionRoleFix,
            ConfigChange,
            CommandExecuted,
            CustomEvent,
            Unknown,
        }

        [Command("modlog"), Description("Logs something to the modlog."), Aliases("mod_log", "log", "ml", "mod_logs", "modlogs", "mls"), RequireUserPermissions(Permissions.ManageGuild)]
        public async Task Overload(CommandContext context, [Description("More details on what's being recorded."), RemainingText] string reason = Constants.MissingReason)
        {
            await Record(context.Guild, LogType.CustomEvent, null, reason);
            _ = await Program.SendMessage(context, "Successfully recorded event into the ModLog.");
        }

        public static async Task Record(DiscordGuild guild, LogType action, Database database = null, [RemainingText] string reason = Constants.MissingReason)
        {
            bool saveDatabase = database == null;
            if (database == null)
            {
                IServiceScope scope = Program.ServiceProvider.CreateScope();
                database = scope.ServiceProvider.GetService<Database>();
            }
            ModLog modLog = new();
            modLog.Action = action;
            modLog.GuildId = guild.Id;
            modLog.LogId = database.ModLogs.Where(log => log.GuildId == guild.Id).Count() + 1;
            modLog.Reason = reason;
            _ = database.ModLogs.Add(modLog);
            LogSetting logSetting = await database.LogSettings.FirstOrDefaultAsync(logSetting => logSetting.GuildId == guild.Id && logSetting.Action == action) ?? await database.LogSettings.FirstOrDefaultAsync(logSetting => logSetting.GuildId == guild.Id && logSetting.Action == LogType.Unknown);
            if (saveDatabase)
            {
                _ = await database.SaveChangesAsync();
                await database.DisposeAsync();
            }

            if (logSetting == null)
            {
                return;
            }

            DiscordChannel modLogChannel = guild.GetChannel(logSetting.ChannelId);
            if (modLogChannel != null && logSetting.IsEnabled)
            {
                DiscordMessageBuilder discordMessageBuilder = new();
                discordMessageBuilder.Content = $"**{action.Humanize(LetterCasing.Title)}**: {reason}";
                discordMessageBuilder.WithAllowedMentions(new List<IMention>());
                discordMessageBuilder.HasTTS(false);
                _ = await modLogChannel.SendMessageAsync(discordMessageBuilder);
            }
        }
    }
}
