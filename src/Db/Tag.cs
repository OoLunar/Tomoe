using System;
using System.ComponentModel.DataAnnotations;

namespace Tomoe.Db
{
    public class Tag
    {
        [Key] public int Id { get; internal set; }
        public int TagId { get; internal set; }
        public string Name { get; internal set; }
        public string Content { get; internal set; }
        public bool IsAlias { get; internal set; }
        public string AliasTo { get; internal set; }
        public ulong OwnerId { get; internal set; }
        public ulong GuildId { get; internal set; }
        public int Uses { get; internal set; }
        public DateTime CreatedAt { get; } = DateTime.UtcNow;
    }
}