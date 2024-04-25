using System;
using System.Threading.Tasks;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Trees;
using DSharpPlus.Commands.Trees.Metadata;

namespace OoLunar.Tomoe.Commands.Common
{
    public static class ChooseCommand
    {
        [Command("choose"), TextAlias("pick", "select", "decide")]
        public static ValueTask ExecuteAsync(CommandContext context, params string[] choices) => context.RespondAsync(choices[Random.Shared.Next(choices.Length)]);
    }
}
