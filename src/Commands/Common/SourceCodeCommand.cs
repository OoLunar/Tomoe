using System.Threading.Tasks;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Trees;
using DSharpPlus.Commands.Trees.Metadata;

namespace OoLunar.Tomoe.Commands.Common
{
    public static class SourceCodeCommand
    {
        [Command("source_code"), TextAlias("repository", "source", "code", "repo")]
        public static ValueTask ExecuteAsync(CommandContext context) => context.RespondAsync($"You can find my source code here: <{ThisAssembly.Project.RepositoryUrl}>");
    }
}
