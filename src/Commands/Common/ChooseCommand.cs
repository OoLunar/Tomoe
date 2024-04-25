using System;
using System.Threading.Tasks;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Trees;
using DSharpPlus.Commands.Trees.Metadata;

namespace OoLunar.Tomoe.Commands.Common
{
    /// <summary>
    /// Helps you decide between multiple choices.
    /// </summary>
    public static class ChooseCommand
    {
        /// <summary>
        /// Randomly selects a choice from the provided list.
        /// </summary>
        /// <remarks>
        /// This command is not rigged.
        /// </remarks>
        /// <param name="choices">The choices to choose from.</param>
        [Command("choose"), TextAlias("pick", "select", "decide")]
        public static ValueTask ExecuteAsync(CommandContext context, params string[] choices) => context.RespondAsync(choices[Random.Shared.Next(choices.Length)]);
    }
}
