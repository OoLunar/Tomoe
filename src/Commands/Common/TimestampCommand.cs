using System;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Converters;
using DSharpPlus.Commands.Processors.TextCommands.Attributes;
using DSharpPlus.Commands.Trees;
using DSharpPlus.Commands.Trees.Attributes;

namespace OoLunar.Tomoe.Commands.Common
{
    public static class TimestampCommand
    {
        private static readonly TimeSpanConverter _timeSpanArgumentConverter = new();
        private static readonly DateTimeOffsetConverter _dateTimeArgumentConverter = new();

        [Command("timestamp")]
        public static async ValueTask ExecuteAsync(CommandContext context, TimestampFormat format = TimestampFormat.LongDateTime, [RemainingText] string? when = null)
        {
            if (when is null)
            {
                await context.RespondAsync($"Timestamp: {Formatter.Timestamp(DateTime.UtcNow, format)}");
            }
            else if ((await _timeSpanArgumentConverter.ExecuteAsync(context, when.Trim())).IsDefined(out TimeSpan timeSpan) && timeSpan != default)
            {
                await context.RespondAsync($"Timestamp: {Formatter.Timestamp(DateTime.UtcNow + timeSpan, format)}");
            }
            else if ((await _dateTimeArgumentConverter.ExecuteAsync(context, when.Trim())).IsDefined(out DateTimeOffset dateTime) && dateTime != default)
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
