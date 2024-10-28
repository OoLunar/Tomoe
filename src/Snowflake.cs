using System;
using System.Globalization;

namespace OoLunar.Tomoe
{
    /// <summary>
    /// Implements https://discord.com/developers/docs/reference#snowflakes.
    /// </summary>
    public readonly record struct DiscordSnowflake
    {
        /// <summary>
        /// The first second of 2015. The date Discord officially recognizes as it's epoch.
        /// </summary>
        public static readonly DateTimeOffset DiscordEpoch = new(2015, 1, 1, 0, 0, 0, TimeSpan.Zero);

        /// <summary>
        /// A numerical representation of the snowflake.
        /// </summary>
        public ulong Value { get; init; }

        /// <summary>
        /// Milliseconds since Discord Epoch, the first second of 2015 or 1420070400000.
        /// </summary>
        public DateTimeOffset Timestamp => DiscordEpoch.AddMilliseconds(Value >> 22);

        /// <summary>
        /// The internal worker's ID that was used to generate the snowflake.
        /// </summary>
        public byte InternalWorkerId => (byte)((Value & 0x3E0000) >> 17);

        /// <summary>
        /// The internal process' ID that was used to generate the snowflake.
        /// </summary>
        public byte InternalProcessId => (byte)((Value & 0x1F000) >> 12);

        /// <summary>
        /// A number incremented by 1 every time the snowflake is generated.
        /// </summary>
        public ushort InternalIncrement => (ushort)(Value & 0xFFF);

        /// <summary>
        /// Creates a new snowflake from a numerical representation.
        /// </summary>
        /// <param name="value">The numerical representation to translate from.</param>
        public DiscordSnowflake(ulong value) => Value = value;

        /// <summary>
        /// Creates a fake snowflake from scratch. If no parameters are provided, returns a randomly generated snowflake.
        /// </summary>
        /// <param name="timestamp">The date when the snowflake was created at. If null, defaults to the current time.</param>
        /// <param name="workerId">A 5 bit worker id that was used to create the snowflake. If null, generates a random number between 1 and 31.</param>
        /// <param name="processId">A 5 bit process id that was used to create the snowflake. If null, generates a random number between 1 and 31.</param>
        /// <param name="increment">A 12 bit integer which represents the number of previously generated snowflakes. If null, generates a random number between 1 and 4,095.</param>
        public DiscordSnowflake(DateTimeOffset? timestamp, byte? workerId, byte? processId, ushort? increment)
        {
            timestamp ??= DateTimeOffset.UtcNow;
            workerId ??= (byte)Random.Shared.Next(1, byte.MaxValue);
            processId ??= (byte)Random.Shared.Next(1, byte.MaxValue);
            increment ??= (ushort)Random.Shared.Next(1, ushort.MaxValue);

            Value = (((uint)timestamp.Value.Subtract(DiscordEpoch).TotalMilliseconds) << 22)
                | ((ulong)workerId.Value << 17)
                | ((ulong)processId.Value << 12)
                | increment.Value;
        }

        public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);
        public override int GetHashCode() => Value.GetHashCode();
        public static implicit operator ulong(DiscordSnowflake snowflake) => snowflake.Value;
        public static implicit operator DiscordSnowflake(ulong value) => new(value);
    }
}
