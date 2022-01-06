using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.Entities;
using Humanizer;
using System;
using System.Threading.Tasks;
using static Tomoe.Api.Moderation;

namespace Tomoe.Utils.Converters
{
    public class LogTypeConverter : IArgumentConverter<LogType>
    {
        public Task<Optional<LogType>> ConvertAsync(string value, CommandContext ctx)
        {
            try
            {
                return Task.FromResult(Optional.FromValue(value.ToLowerInvariant().DehumanizeTo<LogType>()));
            }
            catch (NoMatchFoundException)
            {
                return Enum.TryParse(value, true, out LogType logType)
                    ? Task.FromResult(Optional.FromValue(logType))
                    : Task.FromResult(Optional.FromValue(LogType.Unknown));
            }
        }
    }
}