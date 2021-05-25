namespace Tomoe.Commands.Public
{
    using DSharpPlus.SlashCommands;
    using System.Linq;
    using System.Threading.Tasks;

    public class Sort : SlashCommandModule
    {
        [SlashCommand("sort", "Organized a line seperated list alphabetically.")]
        public async Task Overload(InteractionContext context, [Option("List", "A line seperated list.")] string list) => await Program.SendMessage(context, string.Join('\n', list.Split('\n').OrderBy(x => x)));
    }
}
