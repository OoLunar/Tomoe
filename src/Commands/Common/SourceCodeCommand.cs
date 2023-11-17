using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Commands.Trees;
using DSharpPlus.Commands.Trees.Attributes;
using DSharpPlus.Commands.Processors.TextCommands.Attributes;

namespace OoLunar.Tomoe.Commands.Common
{
    public sealed class SourceCodeCommand
    {
        private static readonly string _sourceCodeUrl = typeof(Program).Assembly.GetCustomAttributes<AssemblyMetadataAttribute>().First(attribute => attribute.Key == "RepositoryUrl").Value!;

        [Command("source_code"), TextAlias("repository", "source", "code", "repo")]
        public static async Task ExecuteAsync(CommandContext context) => await context.RespondAsync($"You can find my source code here: {Formatter.EmbedlessUrl(new(_sourceCodeUrl))}");
    }
}
