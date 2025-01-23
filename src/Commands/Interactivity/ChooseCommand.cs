using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus.Commands;
using OoLunar.Tomoe.Interactivity;

namespace OoLunar.Tomoe.Commands.Interactivity
{
    [Command("interactivity")]
    public static partial class InteractivityCommand
    {
        [Command("choose")]
        public static async ValueTask ChooseAsync(CommandContext context, string question, params string[] choices)
        {
            if (choices.Length < 2)
            {
                await context.RespondAsync("You need to provide at least two choices.");
                return;
            }

            IReadOnlyList<string> choose = await context.ChooseAsync(question, choices);
            await context.RespondAsync(choose.Count == 0
                ? "Times up! You didn't pick anything."
                : $"You picked: {string.Join(", ", choose)}"
            );
        }
    }
}
