using System;
using System.ComponentModel.DataAnnotations;

namespace Tomoe.Models
{
    public sealed class AutoReaction
    {
        [Key]
        public int Id { get; init; }
        public ulong GuildId { get; init; }
        public ulong ChannelId { get; init; }
        public string EmojiName { get; init; }

        public AutoReaction() { }

        public AutoReaction(ulong guildId, ulong channelId, string emojiName)
        {
            GuildId = guildId;
            ChannelId = channelId;
            EmojiName = string.IsNullOrWhiteSpace(emojiName) ? throw new ArgumentException("Emoji name cannot be null or whitespace.", nameof(emojiName)) : emojiName;
        }
    }
}
