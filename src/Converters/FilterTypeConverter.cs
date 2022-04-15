using System.ComponentModel;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.Entities;
using Humanizer;
using Tomoe.Enums;

namespace Tomoe.Converters
{
    [DisplayName("Filter Type")]
    public class FilterTypeConverter : IArgumentConverter<FilterType>
    {
        public Task<Optional<FilterType>> ConvertAsync(string value, CommandContext ctx)
        {
            FilterType? result = (FilterType?)value.DehumanizeTo(typeof(FilterType), OnNoMatch.ReturnsNull);
            return Task.FromResult(result == null ? Optional.FromNoValue<FilterType>() : Optional.FromValue(result.Value));
        }
    }
}
