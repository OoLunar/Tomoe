using System;
using System.Collections.Generic;

namespace Tomoe.Models
{
    public class TagModel
    {
        public Guid Id { get; init; }
        public ulong GuildId { get; init; }
        public string Name { get; set; } = null!;
        public List<string> Aliases { get; set; } = new();
        public string Content { get; set; } = null!;
        public int UsageCount { get; set; }
        public ulong AuthorId { get; set; }
        public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

        public override bool Equals(object? obj) => obj is TagModel tag && Id.Equals(tag.Id) && GuildId == tag.GuildId && Name == tag.Name && EqualityComparer<List<string>>.Default.Equals(Aliases, tag.Aliases) && Content == tag.Content && UsageCount == tag.UsageCount && AuthorId == tag.AuthorId && CreatedAt == tag.CreatedAt;
        public override int GetHashCode() => HashCode.Combine(Id, GuildId, Name, Aliases, Content, UsageCount, AuthorId, CreatedAt);
    }
}