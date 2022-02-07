using DSharpPlus.Entities;
using System;

namespace Tomoe.Models
{
    /// <summary>
    /// Represents a <see cref="DiscordEmoji"/> that can be used in a <see cref="AutoReactionModel"/>. Unable to use the actual entity due to serialization issues.
    /// </summary>
    public class EmojiData
    {
        public Guid Id { get; init; }
        public ulong EmojiId { get; init; }
        public string EmojiName { get; init; } = null!;

        public EmojiData() { }

        public EmojiData(DiscordEmoji emoji)
        {
            Id = Guid.NewGuid();
            EmojiId = emoji.Id;
            EmojiName = emoji.Name;
        }

        public override bool Equals(object? obj) => obj is EmojiData data && Id.Equals(data.Id) && EmojiId == data.EmojiId && EmojiName == data.EmojiName;
        public override int GetHashCode() => HashCode.Combine(Id, EmojiId, EmojiName);
    }
}