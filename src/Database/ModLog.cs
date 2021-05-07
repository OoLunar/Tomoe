namespace Tomoe.Db
{
    using System.ComponentModel.DataAnnotations;
    using static Tomoe.Commands.Moderation.ModLogs;

    public class ModLog
    {
        [Key]
        public int Id { get; internal set; }
        public int LogId { get; internal set; }
        public ulong GuildId { get; internal set; }
        public ulong ChannelId { get; internal set; }
        public ulong MessageId { get; internal set; }
        public string JumpLink { get; internal set; }
        public string Reason { get; internal set; }
        public LogType Action { get; internal set; }
    }
}
