using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System.Linq;
using System.Threading.Tasks;

namespace Tomoe.Commands.Common
{
    public class Sort : BaseCommandModule
    {
        [Command("sort"), Description("Organized a line seperated list alphabetically.")]
        public async Task Overload(CommandContext context, bool trim = true, [Description("A line seperated list.")] params string[] list) => await context.RespondAsync(string.Join('\n', trim ? list.Select(item => item.Trim()).OrderBy(x => x) : list.OrderBy(x => x)));
    }
}