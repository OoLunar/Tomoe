using System.ComponentModel.DataAnnotations;
using Tomoe.Api;

namespace Tomoe.Db
{
    public class LogSetting
    {
        [Key] public int Id { get; internal set; }
        public ulong GuildId { get; internal set; }
        public ulong ChannelId { get; internal set; }
        public Moderation.LogType Action { get; internal set; }
        public string Format { get; internal set; }
        public bool IsLoggingEnabled { get; internal set; }
    }
}