using System;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace Tomoe.Commands.Common
{
    public class Flip : BaseCommandModule
    {
        [Command("flip"), Description("A simple heads or tails command."), Aliases("choose", "pick")]
        public async Task FlipAsync(CommandContext context) => await context.RespondAsync(Random.Shared.Next(0, 2) == 0 ? "Heads" : "Tails");

        [Command("flip")]
        public async Task FlipAsync(CommandContext context, [Description("Provide a list of options to choose from.")] params string[] choices) => await context.RespondAsync(choices[Random.Shared.Next(0, choices.Length)]);
    }
}
