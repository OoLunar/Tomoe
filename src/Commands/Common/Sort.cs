using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace Tomoe.Commands.Common
{
    public class Sort : BaseCommandModule
    {
        [Command("sort"), Description("Organized a line seperated list alphabetically.")]
        public async Task SortAsync(CommandContext context, [Description("A line seperated list."), RemainingText] string list) => await context.RespondAsync(string.Join("\n", list.Split('\n').Select(x => x.Trim()).OrderBy(x => x)));
    }
}