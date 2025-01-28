using System.Threading.Tasks;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ArgumentModifiers;
using OoLunar.Tomoe.Interactivity.Moments.Prompt;

namespace OoLunar.Tomoe.Commands.Interactivity
{
    public static partial class InteractivityCommand
    {
        [Command("prompt")]
        public static async ValueTask PromptAsync(CommandContext context, [RemainingText] string question)
        {
            string? choose = await context.PromptAsync(question, "You have 30 seconds to respond.");
            await context.RespondAsync(choose is null
                ? "Times up! You didn't pick anything."
                : $"You wrote: {choose}"
            );
        }
    }
}
