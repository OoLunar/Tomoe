using System;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Commands.Converters;
using DSharpPlus.Commands.Trees;
using DSharpPlus.Commands.Trees.Attributes;

namespace OoLunar.Tomoe.Commands.Common
{
    public sealed class TimestampCommand
    {
        private static readonly TimeSpanConverter _timeSpanArgumentConverter = new();
        private static readonly DateTimeOffsetConverter _dateTimeArgumentConverter = new();

        [Command("timestamp")]
        public static async ValueTask ExecuteAsync(CommandContext context, TimestampFormat format = TimestampFormat.LongDateTime, string? when = null)
        {
            if (when is null)
            {
                await context.RespondAsync($"Timestamp: {Formatter.Timestamp(DateTime.UtcNow, format)}");
            }
            else if ((await _timeSpanArgumentConverter.ExecuteAsync(context, when)).IsDefined(out TimeSpan timeSpan))
            {
                await context.RespondAsync($"Timestamp: {Formatter.Timestamp(DateTime.UtcNow + timeSpan, format)}");
            }
            else if ((await _dateTimeArgumentConverter.ExecuteAsync(context, when)).IsDefined(out DateTimeOffset dateTime))
            {
                await context.RespondAsync($"Timestamp: {Formatter.Timestamp(dateTime, format)}");
            }
            else
            {
                await context.RespondAsync($"Invalid timestamp: `{when}`");
            }
        }
    }
}
