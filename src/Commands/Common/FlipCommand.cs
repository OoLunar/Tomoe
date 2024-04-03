using System;
using System.Threading.Tasks;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.TextCommands.Attributes;
using DSharpPlus.Commands.Trees;
using DSharpPlus.Commands.Trees.Attributes;

namespace OoLunar.Tomoe.Commands.Common
{
    public static class FlipCommand
    {
        [Command("flip"), TextAlias("random")]
        public static async ValueTask ExecuteAsync(CommandContext context)
        {
            await context.DeferResponseAsync();
            await Task.Delay(TimeSpan.FromSeconds(Random.Shared.Next(2, 8)));
            await context.RespondAsync(Random.Shared.Next(2) == 0 ? "Heads" : "Tails");
        }
    }
}
