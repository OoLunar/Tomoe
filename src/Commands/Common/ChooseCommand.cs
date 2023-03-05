using System;
using System.Threading.Tasks;
using DSharpPlus.CommandAll.Attributes;
using DSharpPlus.CommandAll.Commands;

namespace OoLunar.Tomoe.Commands.Common
{
    public sealed class ChooseCommand : BaseCommand
    {
        [Command("choose", "pick")]
        public static Task ExecuteAsync(CommandContext context, params string[] choices) => context.ReplyAsync(choices[Random.Shared.Next(choices.Length)]);
    }
}
