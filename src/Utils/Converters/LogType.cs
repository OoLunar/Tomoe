namespace Tomoe.Utils.Converters
{
    using DSharpPlus.CommandsNext;
    using DSharpPlus.CommandsNext.Converters;
    using DSharpPlus.Entities;
    using Humanizer;
    using System.Threading.Tasks;
    using static Tomoe.Commands.Moderation.ModLogs;

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
                return Task.FromResult(Optional.FromValue(LogType.Unknown));
            }
        }
    }
}
