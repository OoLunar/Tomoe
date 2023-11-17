using System;
using System.Threading.Tasks;
using DSharpPlus.Commands.Trees;
using DSharpPlus.Commands.Trees.Attributes;
using DSharpPlus.Commands.Processors.TextCommands.Attributes;

namespace OoLunar.Tomoe.Commands.Common
{
    public sealed class ChooseCommand
    {
        [Command("choose"), TextAlias("pick", "select", "decide")]
        public static async Task ExecuteAsync(CommandContext context, params string[] choices) => await context.RespondAsync(choices[Random.Shared.Next(choices.Length)]);
    }
}
