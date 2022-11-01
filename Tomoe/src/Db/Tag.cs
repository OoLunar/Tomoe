using System;
using System.ComponentModel.DataAnnotations;

namespace Tomoe.Db
{
    public class Tag
    {
        [Key]
        public int Id { get; init; }
        public int TagId { get; init; }
        public string Name { get; internal set; }
        public string? Content { get; internal set; }
        public bool IsAlias => string.IsNullOrWhiteSpace(AliasTo);
        public string? AliasTo { get; init; }
        public ulong OwnerId { get; internal set; }
        public ulong GuildId { get; init; }
        public int Uses { get; internal set; }
        public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

        public Tag(int tagId, string name, string? content, string? aliasTo, ulong ownerId, ulong guildId, int uses)
        {
            TagId = tagId;
            Name = string.IsNullOrWhiteSpace(name) ? throw new ArgumentException("Name cannot be null or whitespace.", nameof(name)) : name;
            Content = string.IsNullOrWhiteSpace(content) && string.IsNullOrWhiteSpace(aliasTo) ? throw new ArgumentException($"Content cannot be null or whitespace unless the {nameof(aliasTo)} argument is passed.", nameof(content)) : content;
            AliasTo = aliasTo;
            OwnerId = ownerId;
            GuildId = guildId;
            Uses = uses;
        }
    }
}
