using System;
using System.Threading.Tasks;
using DSharpPlus.CommandAll.Attributes;
using DSharpPlus.CommandAll.Commands;

namespace OoLunar.Tomoe.Commands.Common
{
    public sealed class FlipCommand : BaseCommand
    {
        [Command("flip", "random")]
        public static async Task ExecuteAsync(CommandContext context)
        {
            await context.DelayAsync();
            await Task.Delay(TimeSpan.FromSeconds(3));
            await context.EditAsync(Random.Shared.Next(2) == 0 ? "Heads" : "Tails");
        }
    }
}
