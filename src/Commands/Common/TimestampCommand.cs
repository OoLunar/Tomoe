using System;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandAll.Commands;
using DSharpPlus.CommandAll.Commands.Attributes;
using DSharpPlus.CommandAll.Converters;
using DSharpPlus.CommandAll.Processors.TextCommands;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.DependencyInjection;

namespace OoLunar.Tomoe.Commands.Common
{
    public sealed class TimestampCommand
    {
        private static readonly TimeSpanConverter _timeSpanArgumentConverter = new();
        private static readonly DateTimeConverter _dateTimeArgumentConverter = new();

        [Command("timestamp")]
        public static async Task ExecuteAsync(CommandContext context, TimestampFormat format = TimestampFormat.LongDateTime, string? when = null)
        {
            if (when is null)
            {
                await context.RespondAsync($"Timestamp: {Formatter.Timestamp(DateTime.UtcNow, format)}");
                return;
            }

            MessageCreateEventArgs messageCreateEventArgs = TextCommandUtilities.CreateFakeMessageEventArgs(context, when);
            TextConverterContext converterContext = new()
            {
                Channel = context.Channel,
                Command = context.Command,
                Extension = context.Extension,
                RawArguments = when,
                ServiceScope = context.ServiceProvider.CreateAsyncScope(),
                Splicer = context.Extension.GetProcessor<TextCommandProcessor>().Configuration.TextArgumentSplicer,
                User = context.User
            };
            converterContext.NextTextArgument();

            if ((await _timeSpanArgumentConverter.ConvertAsync(converterContext, messageCreateEventArgs)).IsDefined(out TimeSpan timeSpan))
            {
                await context.RespondAsync($"Timestamp: {Formatter.Timestamp(DateTime.UtcNow + timeSpan, format)}");
            }
            else if ((await _dateTimeArgumentConverter.ConvertAsync(converterContext, messageCreateEventArgs)).IsDefined(out DateTime dateTime))
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