using System;
using System.ComponentModel;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.Entities;

namespace Tomoe.Converters
{
    [DisplayName("Guid")]
    public class GuidConverter : IArgumentConverter<Guid>
    {
        public Task<Optional<Guid>> ConvertAsync(string value, CommandContext ctx) => Task.FromResult(Guid.TryParse(value, out Guid result) ? Optional.FromValue(result) : Optional.FromNoValue<Guid>());
    }
}