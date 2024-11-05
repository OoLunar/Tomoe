using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Trees;
using OoLunar.Tomoe.Interactivity;

namespace OoLunar.Tomoe.Commands.Common
{
    public class InteractivityCommands
    {
        [Command("confirm")]
        public async ValueTask ExecuteAsync(CommandContext context)
        {
            bool? result = await context.ConfirmAsync("Are you sure you want to confirm?");
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
            string? result = await context.PromptAsync("What is your favorite color?");
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
            string? result = await context.ChooseAsync("Choose your favorite color", ["Red", "Green", "Blue"]);
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
            IReadOnlyList<string> result = await context.ChooseMultipleAsync("Choose your favorite colors", ["Red", "Green", "Blue"]);
            if (result.Count == 0)
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
