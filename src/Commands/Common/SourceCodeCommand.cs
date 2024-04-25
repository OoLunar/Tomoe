using System.Threading.Tasks;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Trees;
using DSharpPlus.Commands.Trees.Metadata;

namespace OoLunar.Tomoe.Commands.Common
{
    /// <summary>
    /// Part of me honestly hopes that nobody hosts Tomoe except for me (Lunar). Please just make PRs to the repository instead 😭
    /// </summary>
    public static class SourceCodeCommand
    {
        /// <summary>
        /// Sends a link to the repository which contains the code for the bot.
        /// </summary>
        [Command("source_code"), TextAlias("repository", "source", "code", "repo")]
        public static ValueTask ExecuteAsync(CommandContext context) => context.RespondAsync($"You can find my source code here: <{ThisAssembly.Project.RepositoryUrl}>");
    }
}
