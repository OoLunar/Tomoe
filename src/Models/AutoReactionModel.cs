using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;
using Tomoe.Enums;

namespace Tomoe.Models
{
    public class AutoReactionModel
    {
        public Guid Id { get; init; }
        public ulong GuildId { get; init; }
        public ulong ChannelId { get; init; }
        public EmojiData[] EmojiData { get; init; } = Array.Empty<EmojiData>();
        public FilterType FilterType { get; init; }
        [Column(TypeName = "text")]
        public Regex? Regex { get; init; }

        public override bool Equals(object? obj) => obj is AutoReactionModel reaction && Id.Equals(reaction.Id) && GuildId == reaction.GuildId && ChannelId == reaction.ChannelId && EqualityComparer<EmojiData[]>.Default.Equals(EmojiData, reaction.EmojiData) && FilterType == reaction.FilterType && EqualityComparer<Regex>.Default.Equals(Regex, reaction.Regex);
        public override int GetHashCode() => HashCode.Combine(Id, GuildId, ChannelId, EmojiData, FilterType, Regex);
    }
}