namespace Tomoe.Db
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public class Strike
    {
        [Key] public int Id { get; internal set; }
        public int LogId { get; internal set; }
        public ulong GuildId { get; internal set; }
        public ulong IssuerId { get; internal set; }
        public ulong VictimId { get; internal set; }
        public List<string> Reasons { get; } = new();
        public List<string> JumpLinks { get; } = new();
        public bool VictimMessaged { get; internal set; }
        public bool Dropped { get; internal set; }
        public DateTime CreatedAt { get; } = DateTime.UtcNow;
    }
}