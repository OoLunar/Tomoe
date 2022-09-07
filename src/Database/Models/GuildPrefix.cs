using System;
using EdgeDB;

namespace OoLunar.Tomoe.Database.Models
{
    [EdgeDBType("GuildPrefix")]
    public sealed class GuildPrefixModel
    {
        public string Prefix { get; init; } = null!;
        public ulong Creator { get; init; }
        public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;

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
