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

        /// <summary>
        /// Sends the latency of the bot's connection to Discord.
        /// </summary>
        [Command("confirm")]
        public async ValueTask ExecuteAsync(CommandContext context) => await _procrastinator.ChooseAsync(
            context,
            "Are you sure you want to confirm?",
            ["Yes", "No"],
            async data => await context.RespondAsync("You have confirmed!")
        );
    }
}
