namespace Tomoe.Db
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public class Strike
    {
        [Key]
        public int Id { get; internal set; }
        public long LogId { get; internal set; }
        public ulong GuildId { get; internal set; }
        public ulong IssuerId { get; internal set; }
        public ulong VictimId { get; internal set; }
        public List<string> Reasons { get; } = new();
        public bool VictimMessaged { get; internal set; }
        public bool Dropped { get; internal set; }
        public List<DateTime> Changes { get; } = new() { DateTime.UtcNow };
    }
}