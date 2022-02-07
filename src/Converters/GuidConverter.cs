using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.Entities;
using System;
using System.Threading.Tasks;

namespace Tomoe.Converters
{
    public class GuidConverter : IArgumentConverter<Guid>
    {
        public Task<Optional<Guid>> ConvertAsync(string value, CommandContext ctx) => Task.FromResult(Guid.TryParse(value, out Guid result) ? Optional.FromValue(result) : Optional.FromNoValue<Guid>());
    }
}