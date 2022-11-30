using System;
using System.Threading.Tasks;
using OoLunar.DSharpPlus.CommandAll.Attributes;
using OoLunar.DSharpPlus.CommandAll.Commands;

namespace OoLunar.Tomoe.Commands.Common
{
    public sealed class ChooseCommand : BaseCommand
    {
        [Command("choose")]
        public static Task ChooseAsync(CommandContext context, params string[] choices) => context.ReplyAsync(new() { Content = choices[Random.Shared.Next(choices.Length)] });
    }
}
