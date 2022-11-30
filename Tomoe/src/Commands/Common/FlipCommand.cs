using System;
using System.Threading.Tasks;
using OoLunar.DSharpPlus.CommandAll.Attributes;
using OoLunar.DSharpPlus.CommandAll.Commands;

namespace OoLunar.Tomoe.Commands.Common
{
    public sealed class FlipCommand : BaseCommand
    {
        [Command("flip")]
        public static async Task FlipAsync(CommandContext context)
        {
            await context.DelayAsync();
            await Task.Delay(TimeSpan.FromSeconds(3));
            await context.ReplyAsync(new() { Content = Random.Shared.Next(2) == 0 ? "Heads" : "Tails" });
        }
    }
}
