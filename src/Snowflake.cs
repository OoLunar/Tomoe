using System;
using System.Globalization;

namespace OoLunar.Tomoe
{
    // TODO: Write a validation test.
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
        public DateTimeOffset Timestamp { get; init; }

        /// <summary>
        /// The internal worker's ID that was used to generate the snowflake.
        /// </summary>
        public byte InternalWorkerId { get; init; }

        /// <summary>
        /// The internal process' ID that was used to generate the snowflake.
        /// </summary>
        public byte InternalProcessId { get; init; }

        /// <summary>
        /// A number incremented by 1 every time the snowflake is generated.
        /// </summary>
        public ushort InternalIncrement { get; init; }

        /// <summary>
        /// Creates a new snowflake from a numerical representation.
        /// </summary>
        /// <param name="value">The numerical representation to translate from.</param>
        public DiscordSnowflake(ulong value)
        {
            Value = value;
            Timestamp = DiscordEpoch.AddMilliseconds(value >> 22);
            InternalWorkerId = (byte)((value & 0x3E0000) >> 17);
            InternalProcessId = (byte)((value & 0x1F000) >> 12);
            InternalIncrement = (ushort)(value & 0xFFF);
        }

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
            workerId ??= (byte)Random.Shared.Next(1, 32);
            processId ??= (byte)Random.Shared.Next(1, 32);
            increment ??= (ushort)Random.Shared.Next(1, 4095);

            Timestamp = timestamp.Value;
            InternalWorkerId = workerId.Value;
            InternalProcessId = processId.Value;
            InternalIncrement = increment.Value;

            Value = (((uint)timestamp.Value.Subtract(DiscordEpoch).TotalMilliseconds) << 22)
                | ((ulong)workerId.Value << 17)
                | ((ulong)processId.Value << 12)
                | increment.Value;
        }

        public override string ToString() => Value.ToString(CultureInfo.InvariantCulture);
        public override int GetHashCode() => HashCode.Combine(Value, Timestamp, InternalWorkerId, InternalProcessId, InternalIncrement);
        public static implicit operator ulong(DiscordSnowflake snowflake) => snowflake.Value;
        public static implicit operator DiscordSnowflake(ulong value) => new(value);
    }
}
