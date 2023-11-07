using System;
using System.Threading.Tasks;
using DSharpPlus.CommandAll.Commands;
using DSharpPlus.CommandAll.Commands.Attributes;
using DSharpPlus.CommandAll.Processors.TextCommands.Attributes;

namespace OoLunar.Tomoe.Commands.Common
{
    public sealed class FlipCommand
    {
        [Command("flip"), TextAlias("random")]
        public static async Task ExecuteAsync(CommandContext context)
        {
            await context.DelayResponseAsync();
            await Task.Delay(TimeSpan.FromSeconds(3));
            await context.EditResponseAsync(Random.Shared.Next(2) == 0 ? "Heads" : "Tails");
        }
    }
}
