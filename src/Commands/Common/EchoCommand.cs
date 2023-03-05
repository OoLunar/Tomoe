using System.Threading.Tasks;
using DSharpPlus.CommandAll.Attributes;
using DSharpPlus.CommandAll.Commands;

namespace OoLunar.Tomoe.Commands.Common
{
    public sealed class EchoCommand : BaseCommand
    {
        [Command("echo", "repeat")]
        public static Task ExecuteAsync(CommandContext context, [RemainingText] string message) => context.ReplyAsync(message);
    }
}
