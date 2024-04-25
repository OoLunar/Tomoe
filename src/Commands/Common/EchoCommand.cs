using System.Threading.Tasks;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ArgumentModifiers;
using DSharpPlus.Commands.Trees;
using DSharpPlus.Commands.Trees.Metadata;

namespace OoLunar.Tomoe.Commands.Common
{
    /// <summary>
    /// Now repeat after me.
    /// </summary>
    public static class EchoCommand
    {
        /// <summary>
        /// Repeats the provided without any modifications.
        /// </summary>
        /// <remarks>
        /// This command will never ping any user or role.
        /// </remarks>
        /// <param name="message">What text the bot should repeat.</param>
        [Command("echo"), TextAlias("repeat", "say")]
        public static ValueTask ExecuteAsync(CommandContext context, [RemainingText] string message) => context.RespondAsync(message);
    }
}
