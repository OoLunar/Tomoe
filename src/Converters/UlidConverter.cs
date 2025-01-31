using System;
using System.Threading.Tasks;
using DSharpPlus.Commands.Converters;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Entities;

namespace OoLunar.Tomoe.Converters
{
    public sealed class UlidConverter : ITextArgumentConverter<Ulid>, ISlashArgumentConverter<Ulid>
    {
        public string ReadableName => "Ulid";
        public DiscordApplicationCommandOptionType ParameterType => DiscordApplicationCommandOptionType.String;
        public ConverterInputType RequiresText => ConverterInputType.Always;

        public Task<Optional<Ulid>> ConvertAsync(ConverterContext context) => Ulid.TryParse(context.Argument as string, out Ulid result)
            ? Task.FromResult(Optional.FromValue(result))
            : Task.FromResult(Optional.FromNoValue<Ulid>());
    }
}
