using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ArgumentModifiers;
using DSharpPlus.Commands.Trees;

namespace OoLunar.Tomoe.Commands.Common
{
    /// <summary>
    /// Why can't this sort out all of my problems?
    /// </summary>
    public static class SortCommand
    {
        /// <summary>
        /// Sorts the given text alphabetically.
        /// </summary>
        /// <param name="text">The text to sort.</param>
        [Command("sort")]
        public static ValueTask ExecuteAsync(CommandContext context, [FromCode] string text)
        {
            char splitChar = !text.Contains('\n') ? ' ' : '\n';
            List<string> words = new(text.Split(splitChar));
            words.Sort();
            return context.RespondAsync(string.Join(splitChar, words));
        }
    }
}
