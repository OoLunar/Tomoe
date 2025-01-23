using System.Threading.Tasks;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ArgumentModifiers;
using OoLunar.Tomoe.Interactivity;

namespace OoLunar.Tomoe.Commands.Interactivity
{
    public static partial class InteractivityCommand
    {
        [Command("choose")]
        public static async ValueTask PromptAsync(CommandContext context, [RemainingText] string question)
        {
            string? choose = await context.PromptAsync(question);
            await context.RespondAsync(choose is null
                ? "Times up! You didn't pick anything."
                : $"You wrote: {choose}"
            );
        }
    }
}
