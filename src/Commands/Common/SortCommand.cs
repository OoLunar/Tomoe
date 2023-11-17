using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus.Commands.Processors.TextCommands.Attributes;
using DSharpPlus.Commands.Trees;
using DSharpPlus.Commands.Trees.Attributes;

namespace OoLunar.Tomoe.Commands.Common
{
    public sealed class SortCommand
    {
        [Command("sort")]
        public static async Task ExecuteAsync(CommandContext context, [RemainingText] string text)
        {
            char splitChar = !text.Contains('\n') ? ' ' : '\n';
            List<string> words = new(text.Split(splitChar));
            words.Sort();
            await context.RespondAsync(string.Join(splitChar, words));
        }
    }
}
