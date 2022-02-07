using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.Entities;
using System.Globalization;
using System.Threading.Tasks;

namespace Tomoe.Converters
{
    public class IMentionConverter : IArgumentConverter<IMention>
    {
        // TODO: Fully implement this horrible piece of code that not even the creator loves.
        public Task<Optional<IMention>> ConvertAsync(string value, CommandContext ctx) => Task.FromResult(!value.Contains('<') ? Optional.FromNoValue<IMention>() : Optional.FromValue<IMention>(value[0..3] switch
        {
            "<@!" => new UserMention(ulong.Parse(value[3..^1], CultureInfo.InvariantCulture)),
            "<@&" => new RoleMention(ulong.Parse(value[3..^1], CultureInfo.InvariantCulture)),
            _ => new UserMention(ulong.Parse(value[3..^1], CultureInfo.InvariantCulture))
        }));
    }
}