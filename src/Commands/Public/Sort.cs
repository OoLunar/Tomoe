namespace Tomoe.Commands.Public
{
    using DSharpPlus.CommandsNext;
    using DSharpPlus.CommandsNext.Attributes;
    using System.Linq;
    using System.Threading.Tasks;

    public class Sort : BaseCommandModule
    {
        [Command("sort"), Description("Organized a line seperated list alphabetically.")]
        public async Task Overload(CommandContext context, [RemainingText, Description("A line seperated list.")] string list) => await Program.SendMessage(context, string.Join('\n', list.Split('\n').OrderBy(x => x)));
    }
}
