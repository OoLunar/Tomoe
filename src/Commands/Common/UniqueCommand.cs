using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ArgumentModifiers;
using DSharpPlus.Commands.Trees.Metadata;

namespace OoLunar.Tomoe.Commands.Common
{
    /// <summary>
    /// I'm unique, just like everyone else.
    /// </summary>
    public static class UniqueCommand
    {
        /// <summary>
        /// Removes duplicate entries from a list of items separated by newlines.
        /// </summary>
        /// <param name="input">The list of items to deduplicate.</param>
        [Command("unique"), TextAlias("uniq", "dedupe", "distinct", "deduplicate")]
        public static ValueTask UniqueMethod(CommandContext context, [RemainingText] string input)
        {
            List<string> list = [];
            foreach (string item in input.Split('\n'))
            {
                if (!list.Contains(item))
                {
                    list.Add(item);
                }
            }

            return context.RespondAsync(string.Join('\n', list));
        }
    }
}
