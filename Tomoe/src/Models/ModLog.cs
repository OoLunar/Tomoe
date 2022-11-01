using System.ComponentModel.DataAnnotations;
using Tomoe.Commands.Moderation;

namespace Tomoe.Models
{
    public class ModLog
    {
        [Key]
        public int Id { get; init; }
        public int LogId { get; init; }
        public ulong GuildId { get; init; }
        public string Reason { get; init; }
        public CustomEvent? LogType { get; internal set; }
        public DiscordEvent? DiscordEvent { get; internal set; }

        public ModLog() { }

        public ModLog(int logId, ulong guildId, string reason, CustomEvent? logType, DiscordEvent? discordEvent)
        {
            LogId = logId;
            GuildId = guildId;
            Reason = string.IsNullOrWhiteSpace(reason) ? "No reason provided." : reason;
            LogType = logType;
            DiscordEvent = discordEvent;
        }
    }
}
