using System;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Commands.Trees;
using DSharpPlus.Commands.Trees.Attributes;
using DSharpPlus.Commands.Processors.TextCommands.Attributes;

namespace OoLunar.Tomoe.Commands.Common
{
    [Command("epoch")]
    public sealed class EpochCommand
    {
        [Command("parse")]
        public sealed class ParseSubCommand
        {
            private static readonly DateTimeOffset DiscordEpoch = new(2015, 1, 1, 0, 0, 0, TimeSpan.Zero);

            [Command("seconds"), TextAlias("second", "s")]
            public static async Task ParseSecondsAsync(CommandContext context, params long[] unixTimestamps)
            {
                StringBuilder builder = new();
                foreach (long unixTimestamp in unixTimestamps)
                {
                    builder.AppendLine(CultureInfo.InvariantCulture, $"`{unixTimestamp}` => {Formatter.Timestamp(DateTimeOffset.FromUnixTimeSeconds(unixTimestamp), TimestampFormat.LongDateTime)}");
                }

                await context.RespondAsync(builder.ToString());
            }

            [Command("milliseconds"), TextAlias("millisecond", "ms")]
            public static async Task ParseMillisecondsAsync(CommandContext context, params long[] unixTimestamps)
            {
                StringBuilder builder = new();
                foreach (long unixTimestamp in unixTimestamps)
                {
                    builder.AppendLine(CultureInfo.InvariantCulture, $"`{unixTimestamp}` => {Formatter.Timestamp(DateTimeOffset.FromUnixTimeMilliseconds(unixTimestamp), TimestampFormat.LongDateTime)}");
                }

                await context.RespondAsync(builder.ToString());
            }

            [Command("snowflake"), TextAlias("discord", "d")]
            public static async Task ParseSnowflakeAsync(CommandContext context, params ulong[] unixTimestamps)
            {
                StringBuilder builder = new();
                foreach (ulong unixTimestamp in unixTimestamps)
                {
                    builder.AppendLine(CultureInfo.InvariantCulture, $"`{unixTimestamp}` => {Formatter.Timestamp(DiscordEpoch.AddMilliseconds(unixTimestamp >> 22), TimestampFormat.LongDateTime)}");
                }

                await context.RespondAsync(builder.ToString());
            }
        }

        [Command("now")]
        public sealed class NowSubCommand
        {
            [Command("seconds"), TextAlias("second", "s")]
            public static async Task NowSecondsAsync(CommandContext context) => await context.RespondAsync(Formatter.InlineCode(DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture)));

            [Command("milliseconds"), TextAlias("millisecond", "ms")]
            public static async Task NowMillisecondsAsync(CommandContext context) => await context.RespondAsync(Formatter.InlineCode(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString(CultureInfo.InvariantCulture)));
        }
    }
}
