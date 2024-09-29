using System.Globalization;
using System.Threading.Tasks;
using DSharpPlus.Commands.Converters;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Entities;

namespace OoLunar.Tomoe.Converters
{
    public sealed class CultureInfoConverter : ISlashArgumentConverter<CultureInfo>, ITextArgumentConverter<CultureInfo>
    {
        public bool RequiresText => true;
        public string ReadableName => "Culture Info";
        public DiscordApplicationCommandOptionType ParameterType => DiscordApplicationCommandOptionType.String;

        public Task<Optional<CultureInfo>> ConvertAsync(ConverterContext context)
        {
            try
            {
                return context.Argument switch
                {
                    // String will be from text commands
                    string languageTag => Task.FromResult(Optional.FromValue(CultureInfo.GetCultureInfoByIetfLanguageTag(languageTag))),

                    // Int will be from slash commands, either through a choice provider or autocomplete
                    int cultureId => Task.FromResult(Optional.FromValue(CultureInfo.GetCultureInfo(cultureId))),

                    // What the fuck.
                    _ => Task.FromResult(Optional.FromNoValue<CultureInfo>())
                };
            }
            catch
            {
                return Task.FromResult(Optional.FromNoValue<CultureInfo>());
            }
        }
    }
}
