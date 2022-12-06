using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DSharpPlus;
using OoLunar.DSharpPlus.CommandAll.Attributes;
using OoLunar.DSharpPlus.CommandAll.Commands;

namespace OoLunar.Tomoe.Commands.Common
{
    public sealed class SourceCodeCommand : BaseCommand
    {
        private static readonly string _sourceCodeUrl = typeof(Program).Assembly.GetCustomAttributes<AssemblyMetadataAttribute>().First(attribute => attribute.Key == "RepositoryUrl").Value!;

        [Command("source_code", "repository", "source", "code")]
        public static Task ExecuteAsync(CommandContext context) => context.ReplyAsync($"You can find my source code here: {Formatter.EmbedlessUrl(new(_sourceCodeUrl))}");
    }
}
