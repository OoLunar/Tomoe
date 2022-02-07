using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.Entities;
using Humanizer;
using System.Threading.Tasks;
using Tomoe.Enums;


namespace Tomoe.Converters
{
    public class FilterTypeConverter : IArgumentConverter<FilterType>
    {
        public Task<Optional<FilterType>> ConvertAsync(string value, CommandContext ctx)
        {
            FilterType? result = (FilterType?)EnumDehumanizeExtensions.DehumanizeTo(value, typeof(FilterType), OnNoMatch.ReturnsNull);
            return Task.FromResult(result == null ? Optional.FromNoValue<FilterType>() : Optional.FromValue(result.Value));
        }
    }
}