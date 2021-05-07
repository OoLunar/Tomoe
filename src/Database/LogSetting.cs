namespace Tomoe.Db
{
    using System.ComponentModel.DataAnnotations;
    using static Tomoe.Commands.Moderation.ModLogs;

    public class LogSetting
    {
        [Key]
        public int Id { get; internal set; }
        public ulong GuildId { get; internal set; }
        public ulong ChannelId { get; internal set; }
        public LogType Action { get; internal set; }
        public string Format { get; internal set; }
        public bool IsEnabled { get; internal set; }
    }
}
