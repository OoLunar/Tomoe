using System;
using System.Collections.Generic;
using EdgeDB;

namespace OoLunar.Tomoe.Database
{
    [EdgeDBType("GuildPrefix")]
    public sealed class GuildPrefixModel
    {
        public string Prefix { get; init; } = null!;
        public ulong Creator { get; init; }
        public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;

        [EdgeDBDeserializer]
        private GuildPrefixModel(IDictionary<string, object?> raw)
        {
            Prefix = (string)raw["prefix"]!;
            Creator = (ulong)raw["creator"]!;
            CreatedAt = (DateTimeOffset)raw["created_at"]!;
        }

        internal GuildPrefixModel(string prefix, ulong creator)
        {
            if (string.IsNullOrWhiteSpace(prefix))
            {
                throw new ArgumentException("Guild prefix cannot be null or empty.", nameof(prefix));
            }

            Prefix = prefix;
            Creator = creator;
            CreatedAt = DateTimeOffset.UtcNow;
        }
    }
}
