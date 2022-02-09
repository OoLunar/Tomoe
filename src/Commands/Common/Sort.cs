using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System.Linq;
using System.Threading.Tasks;

namespace Tomoe.Commands.Common
{
    public class Sort : BaseCommandModule
    {
        [Command("sort"), Description("Organized a line seperated list alphabetically.")]
        public async Task SortAsync(CommandContext context, [Description("Whether to remove extra whitespace per line.")] bool trim = true, [Description("A line seperated list.")] params string[] list) => await context.RespondAsync(string.Join('\n', trim ? list.Select(item => item.Trim()).OrderBy(x => x) : list.OrderBy(x => x)));
    }
}