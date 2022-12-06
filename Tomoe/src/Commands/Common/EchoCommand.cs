using System.Threading.Tasks;
using OoLunar.DSharpPlus.CommandAll.Attributes;
using OoLunar.DSharpPlus.CommandAll.Commands;

namespace OoLunar.Tomoe.Commands.Common
{
    public sealed class EchoCommand : BaseCommand
    {
        [Command("echo", "repeat")]
        public static Task ExecuteAsync(CommandContext context, [RemainingText] string message) => context.ReplyAsync(message);
    }
}
