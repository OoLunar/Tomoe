namespace Tomoe.Db
{
    using System.ComponentModel.DataAnnotations;

    public class ModLog
    {
        [Key]
        public int Id { get; internal set; }
        public int LogId { get; internal set; }
        public ulong GuildId { get; internal set; }
        public string Reason { get; internal set; }
        public Commands.Moderation.LogType LogType { get; internal set; }
    }
}