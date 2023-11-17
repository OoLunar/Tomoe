using System;
using System.Threading.Tasks;
using DSharpPlus.Commands.Processors.TextCommands.Attributes;
using DSharpPlus.Commands.Trees;
using DSharpPlus.Commands.Trees.Attributes;

namespace OoLunar.Tomoe.Commands.Common
{
    public sealed class FlipCommand
    {
        [Command("flip"), TextAlias("random")]
        public static async Task ExecuteAsync(CommandContext context)
        {
            await context.DeferResponseAsync();
            await Task.Delay(TimeSpan.FromSeconds(3));
            await context.EditResponseAsync(Random.Shared.Next(2) == 0 ? "Heads" : "Tails");
        }
    }
}
