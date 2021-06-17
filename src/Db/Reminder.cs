namespace Tomoe.Db
{
    using System;
    using System.ComponentModel.DataAnnotations;

    public class Reminder
    {
        [Key]
        public int Id { get; internal set; }
        public int LogId { get; internal set; }
        public ulong GuildId { get; internal set; }
        public ulong ChannelId { get; internal set; }
        public ulong MessageId { get; internal set; }
        public ulong UserId { get; internal set; }
        public string JumpLink { get; internal set; }
        public string Content { get; internal set; }
        public bool Expires { get; internal set; }
        public DateTime ExpiresOn { get; internal set; }
        public DateTime IssuedAt { get; } = DateTime.UtcNow;
        public Commands.Moderation.LogType Punishment { get; internal set; }
    }
}