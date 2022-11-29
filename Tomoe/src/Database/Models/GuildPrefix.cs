using System;
using EdgeDB;
using OoLunar.Tomoe.Database.Converters;

namespace OoLunar.Tomoe.Database.Models
{
    /// <summary>
    /// Represents a prefix that a guild can use to activate the bot. Contains metadata on who created the prefix and when.
    /// </summary>
    [EdgeDBType("GuildPrefix")]
    public sealed class GuildPrefixModel
    {
        /// <summary>
        /// The prefix to listen for.
        /// </summary>
        public string Prefix { get; private set; } = null!;

        /// <summary>
        /// The id of the user who created the prefix.
        /// </summary>
        //[EdgeDBTypeConverter(typeof(UlongTypeConverter))]
        public ulong Creator { get; private set; }

        /// <summary>
        /// When the prefix was created.
        /// </summary>
        public DateTimeOffset CreatedAt { get; private set; } = DateTimeOffset.UtcNow;

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
