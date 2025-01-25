using System.Threading.Tasks;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ArgumentModifiers;
using OoLunar.Tomoe.Interactivity.Moments.Confirm;

namespace OoLunar.Tomoe.Commands.Interactivity
{
    public static partial class InteractivityCommand
    {
        [Command("confirm")]
        public static async ValueTask ConfirmAsync(CommandContext context, [RemainingText] string question)
        {
            bool? choose = await context.ConfirmAsync(question);
            await context.RespondAsync(choose is null
                ? "Times up! You didn't pick anything."
                : $"You chose: {choose}"
            );
        }
    }
}
