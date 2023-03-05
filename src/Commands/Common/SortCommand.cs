using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus.CommandAll.Attributes;
using DSharpPlus.CommandAll.Commands;

namespace OoLunar.Tomoe.Commands.Common
{
    public sealed class SortCommand : BaseCommand
    {
        [Command("sort")]
        public static Task ExecuteAsync(CommandContext context, [RemainingText] string text)
        {
            List<string> words = new(text.Split('\n'));
            words.Sort();
            return context.ReplyAsync(string.Join('\n', words));
        }
    }
}
