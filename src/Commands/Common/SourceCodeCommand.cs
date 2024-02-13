using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.TextCommands.Attributes;
using DSharpPlus.Commands.Trees;
using DSharpPlus.Commands.Trees.Attributes;

namespace OoLunar.Tomoe.Commands.Common
{
    public sealed class SourceCodeCommand
    {
        private static readonly string _sourceCodeUrl = typeof(Program).Assembly.GetCustomAttributes<AssemblyMetadataAttribute>().First(attribute => attribute.Key == "RepositoryUrl").Value!;

        [Command("source_code"), TextAlias("repository", "source", "code", "repo")]
        public static ValueTask ExecuteAsync(CommandContext context) => context.RespondAsync($"You can find my source code here: {Formatter.EmbedlessUrl(new(_sourceCodeUrl))}");
    }
}
