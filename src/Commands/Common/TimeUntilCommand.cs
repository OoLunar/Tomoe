using System;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ArgumentModifiers;
using DSharpPlus.Commands.Converters;
using DSharpPlus.Commands.Processors.TextCommands;
using Humanizer;
using Microsoft.Extensions.DependencyInjection;

namespace OoLunar.Tomoe.Commands.Common
{
    /// <summary>
    /// "Time until in 4 hours: 4 hours, 43 minutes, 33 seconds"
    /// </summary>
    /// <remarks>
    /// I'm only getting 4 and a half hours of sleep tonight.
    /// </remarks>
    public static class TimeUntilCommand
    {
        private static readonly DateTimeOffsetConverter _dateTimeArgumentConverter = new();

        /// <summary>
        /// Calculates how long you have to wait until a specific time.
        /// </summary>
        /// <param name="when">When you want to know how long you have to wait.</param>
        [Command("time_until")]
        public static async ValueTask ExecuteAsync(CommandContext context, [RemainingText] string? when = null)
        {
            if (string.IsNullOrWhiteSpace(when))
            {
                await context.RespondAsync("Invalid timestamp: `null`");
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
                PrefixLength = 0,
                User = context.User
            };

            converterContext.NextArgument();
            if (!(await _dateTimeArgumentConverter.ConvertAsync(converterContext)).IsDefined(out DateTimeOffset dateTimeOffset) || dateTimeOffset == default)
            {
                await context.RespondAsync($"Invalid timestamp: `{when}`");
                return;
            }

            TimeSpan offset = (await context.GetTimeZoneAsync()).BaseUtcOffset;
            DateTimeOffset untilDate = dateTimeOffset.ToOffset(offset).Subtract(offset);
            TimeSpan untilTime = untilDate - DateTimeOffset.UtcNow;
            await context.RespondAsync($"Time until {Formatter.Timestamp(untilDate)}: {untilTime.Humanize(3)}");
        }
    }
}
