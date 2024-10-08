using System;
using System.Threading.Tasks;
using DSharpPlus.Commands.Converters;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Entities;

namespace OoLunar.Tomoe.Converters
{
    public sealed class TimeZoneInfoConverter : ISlashArgumentConverter<TimeZoneInfo>, ITextArgumentConverter<TimeZoneInfo>
    {
        public DiscordApplicationCommandOptionType ParameterType => DiscordApplicationCommandOptionType.String;
        public ConverterInputType RequiresText => ConverterInputType.Always;
        public string ReadableName => "Timezone";

        public Task<Optional<TimeZoneInfo>> ConvertAsync(ConverterContext context)
        {
            if (context.Argument is not string argument)
            {
                return Task.FromResult(Optional.FromNoValue<TimeZoneInfo>());
            }
            else if (TimeZoneInfo.TryFindSystemTimeZoneById(argument, out TimeZoneInfo? timeZoneInfo))
            {
                return Task.FromResult(Optional.FromValue(timeZoneInfo));
            }

            return Task.FromResult(Optional.FromNoValue<TimeZoneInfo>());
        }
    }
}
