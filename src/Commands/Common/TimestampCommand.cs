using System;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ArgumentModifiers;
using DSharpPlus.Commands.Converters;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Commands.Trees;
using Microsoft.Extensions.DependencyInjection;

namespace OoLunar.Tomoe.Commands.Common
{
    /// <summary>
    /// Timestamps are a way to represent a point in time. Unix timestamps are great! Discord timestamps are not.
    /// </summary>
    public static class TimestampCommand
    {
        private static readonly TimeSpanConverter _timeSpanArgumentConverter = new();
        private static readonly DateTimeOffsetConverter _dateTimeArgumentConverter = new();

        /// <summary>
        /// Creates a timestamp for the current time or a time in the future. Timestamps are localized to the each user's timezone, making it easier to coordinate events.
        /// </summary>
        /// <param name="format">The format of the timestamp to display.</param>
        /// <param name="when">When the timestamp should be created. If not provided, the current time is used.</param>
        [Command("timestamp")]
        public static async ValueTask ExecuteAsync(CommandContext context, TimestampFormat format = TimestampFormat.LongDateTime, [RemainingText] string? when = null)
        {
            if (string.IsNullOrWhiteSpace(when))
            {
                await context.RespondAsync($"Timestamp: {Formatter.Timestamp(DateTime.UtcNow, format)}");
                return;
            }

            when = when.Trim();
            TextConverterContext converterContext = new()
            {
                Channel = context.Channel,
                Command = context.Command,
                Extension = context.Extension,
                RawArguments = when,
                Message = null!,
                ServiceScope = context.ServiceProvider.CreateAsyncScope(),
                Splicer = context.Extension.GetProcessor<TextCommandProcessor>().Configuration.TextArgumentSplicer,
                User = context.User
            };

            if ((await _timeSpanArgumentConverter.ConvertAsync(converterContext)).IsDefined(out TimeSpan timeSpan) && timeSpan != default)
            {
                await context.RespondAsync($"Timestamp: {Formatter.Timestamp(DateTime.UtcNow + timeSpan, format)}");
            }
            else if ((await _dateTimeArgumentConverter.ConvertAsync(converterContext)).IsDefined(out DateTimeOffset dateTime) && dateTime != default)
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
