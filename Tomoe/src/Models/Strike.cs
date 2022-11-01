using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Tomoe.Models
{
    public class Strike
    {
        [Key]
        public int Id { get; init; }
        public long LogId { get; init; }
        public ulong GuildId { get; init; }
        public ulong IssuerId { get; init; }
        public ulong VictimId { get; init; }
        public List<string> Reasons { get; } = new();
        public bool VictimMessaged { get; internal set; }
        public bool Dropped { get; internal set; }
        public List<DateTime> Changes { get; } = new() { DateTime.UtcNow };

        public Strike() { }

        public Strike(long logId, ulong guildId, ulong issuerId, ulong victimId, string reason)
        {
            LogId = logId;
            GuildId = guildId;
            IssuerId = issuerId;
            VictimId = victimId;
            Reasons.Add(reason ?? "No reason provided.");
        }
    }
}
