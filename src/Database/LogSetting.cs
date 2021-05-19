namespace Tomoe.Db
{
    using System.ComponentModel.DataAnnotations;

    public class LogSetting
    {
        [Key]
        public int Id { get; internal set; }
        public ulong GuildId { get; internal set; }
        public ulong ChannelId { get; internal set; }
        public Api.Moderation.LogType Action { get; internal set; }
        public string Format { get; internal set; }
        public bool IsLoggingEnabled { get; internal set; }
    }
}
