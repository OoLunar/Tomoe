using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus.CommandAll.Commands;
using DSharpPlus.CommandAll.Commands.Attributes;
using DSharpPlus.CommandAll.Processors.TextCommands.Attributes;

namespace OoLunar.Tomoe.Commands.Common
{
    public sealed class SortCommand
    {
        [Command("sort")]
        public static async Task ExecuteAsync(CommandContext context, [RemainingText] string text)
        {
            List<string> words = new(text.Split('\n'));
            words.Sort();
            await context.RespondAsync(string.Join('\n', words));
        }
    }
}
