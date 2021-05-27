namespace Tomoe.Db
{
    using Api;
    using System.ComponentModel.DataAnnotations;

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