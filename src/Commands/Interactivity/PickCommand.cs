using System.Threading.Tasks;
using DSharpPlus.Commands;
using OoLunar.Tomoe.Interactivity.Moments.Pick;

namespace OoLunar.Tomoe.Commands.Interactivity
{
    public static partial class InteractivityCommand
    {
        [Command("pick")]
        public static async ValueTask PickAsync(CommandContext context, string question, params string[] choices)
        {
            if (choices.Length < 2)
            {
                await context.RespondAsync("You need to provide at least two choices.");
                return;
            }

            string? choice = await context.PickAsync(question, choices);
            await context.RespondAsync(choice is null
                ? "Times up! You didn't pick anything."
                : $"You picked: {choice}"
            );
        }
    }
}
