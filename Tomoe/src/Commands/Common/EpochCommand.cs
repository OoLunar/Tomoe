using System;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using OoLunar.DSharpPlus.CommandAll.Attributes;
using OoLunar.DSharpPlus.CommandAll.Commands;

namespace OoLunar.Tomoe.Commands.Common
{
    [Command("epoch")]
    public sealed class EpochCommand : BaseCommand
    {
        [Command("parse")]
        public sealed class ParseSubCommand : BaseCommand
        {
            private static readonly DateTimeOffset DiscordEpoch = new(2015, 1, 1, 0, 0, 0, TimeSpan.Zero);

            [Command("seconds", "second", "s")]
            public static Task ParseSecondsAsync(CommandContext context, params long[] unixTimestamps)
            {
                StringBuilder builder = new();
                foreach (long unixTimestamp in unixTimestamps)
                {
                    _ = builder.AppendLine(CultureInfo.InvariantCulture, $"`{unixTimestamp}` => {Formatter.Timestamp(DateTimeOffset.FromUnixTimeSeconds(unixTimestamp), TimestampFormat.LongDateTime)}");
                }
                return context.ReplyAsync(builder.ToString());
            }

            [Command("milliseconds", "millisecond", "ms")]
            public static Task ParseMillisecondsAsync(CommandContext context, params long[] unixTimestamps)
            {
                StringBuilder builder = new();
                foreach (long unixTimestamp in unixTimestamps)
                {
                    _ = builder.AppendLine(CultureInfo.InvariantCulture, $"`{unixTimestamp}` => {Formatter.Timestamp(DateTimeOffset.FromUnixTimeMilliseconds(unixTimestamp), TimestampFormat.LongDateTime)}");
                }
                return context.ReplyAsync(builder.ToString());
            }

            [Command("snowflake", "discord", "d")]
            public static Task ParseSnowflakeAsync(CommandContext context, params ulong[] unixTimestamps)
            {
                StringBuilder builder = new();
                foreach (ulong unixTimestamp in unixTimestamps)
                {
                    _ = builder.AppendLine(CultureInfo.InvariantCulture, $"`{unixTimestamp}` => {Formatter.Timestamp(DiscordEpoch.AddMilliseconds(unixTimestamp >> 22), TimestampFormat.LongDateTime)}");
                }
                return context.ReplyAsync(builder.ToString());
            }
        }

        [Command("now")]
        public sealed class NowSubCommand : BaseCommand
        {
            [Command("seconds", "second", "s")]
            public static Task NowSecondsAsync(CommandContext context) => context.ReplyAsync(Formatter.InlineCode(DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture)));

            [Command("milliseconds", "millisecond", "ms")]
            public static Task NowMillisecondsAsync(CommandContext context) => context.ReplyAsync(Formatter.InlineCode(DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString(CultureInfo.InvariantCulture)));
        }
    }
}
