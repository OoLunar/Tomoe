using System;

namespace Tomoe.Models
{
    public class AutoMentionModel
    {
        public Guid Id { get; init; }
        public ulong GuildId { get; init; }
        public ulong ChannelId { get; init; }
        public ulong Snowflake { get; init; }
        public bool IsRole { get; init; }
        public string? Regex { get; init; }

        public override bool Equals(object? obj) => obj is AutoMentionModel mention && Id.Equals(mention.Id) && GuildId == mention.GuildId && ChannelId == mention.ChannelId && Snowflake == mention.Snowflake && IsRole == mention.IsRole && Regex == mention.Regex;
        public override int GetHashCode() => HashCode.Combine(Id, GuildId, ChannelId, Snowflake, IsRole, Regex);
    }
}