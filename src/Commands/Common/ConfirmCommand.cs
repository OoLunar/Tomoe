using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Trees;
using OoLunar.Tomoe.Interactivity;

namespace OoLunar.Tomoe.Commands.Common
{
    /// <summary>
    /// Ping! Pong!
    /// </summary>
    public class ConfirmCommand
    {
        private readonly Procrastinator _procrastinator;

        public ConfirmCommand(Procrastinator procrastinator) => _procrastinator = procrastinator;

        [Command("confirm")]
        public async ValueTask ExecuteAsync(CommandContext context)
        {
            bool? result = await _procrastinator.ConfirmAsync(context, "Are you sure you want to confirm?");
            if (result is null)
            {
                await context.RespondAsync("Timed out!");
            }
            else if (result.Value)
            {
                await context.RespondAsync("Confirmed!");
            }
            else
            {
                await context.RespondAsync("Cancelled!");
            }
        }

        [Command("prompt")]
        public async ValueTask PromptAsync(CommandContext context)
        {
            string? result = await _procrastinator.PromptAsync(context, "What is your favorite color?");
            if (result is null)
            {
                await context.RespondAsync("Timed out!");
            }
            else
            {
                await context.RespondAsync($"You answered: {result}");
            }
        }

        [Command("pick")]
        public async ValueTask ChooseAsync(CommandContext context)
        {
            string? result = await _procrastinator.ChooseAsync(context, "Choose your favorite color", ["Red", "Green", "Blue"]);
            if (result is null)
            {
                await context.RespondAsync("Timed out!");
            }
            else
            {
                await context.RespondAsync($"You chose: {result}");
            }
        }

        [Command("multipick")]
        public async ValueTask ChooseMultipleAsync(CommandContext context)
        {
            IReadOnlyList<string>? result = await _procrastinator.ChooseMultipleAsync(context, "Choose your favorite colors", ["Red", "Green", "Blue"]);
            if (result is null)
            {
                await context.RespondAsync("Timed out!");
            }
            else
            {
                await context.RespondAsync($"You chose: {string.Join(", ", result)}");
            }
        }
    }
}
