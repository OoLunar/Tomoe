using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ArgumentModifiers;
using DSharpPlus.Commands.Trees;

namespace OoLunar.Tomoe.Commands.Common
{
    public static class SortCommand
    {
        [Command("sort")]
        public static ValueTask ExecuteAsync(CommandContext context, [RemainingText] string text)
        {
            char splitChar = !text.Contains('\n') ? ' ' : '\n';
            List<string> words = new(text.Split(splitChar));
            words.Sort();
            return context.RespondAsync(string.Join(splitChar, words));
        }
    }
}
