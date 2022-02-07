using System;
using System.ComponentModel.DataAnnotations;

namespace Tomoe.Models
{
    public class GuildConfigModel
    {
        [Key]
        public ulong GuildId { get; init; }

        public override bool Equals(object? obj) => obj is GuildConfigModel config && GuildId == config.GuildId;
        public override int GetHashCode() => HashCode.Combine(GuildId);
    }
}