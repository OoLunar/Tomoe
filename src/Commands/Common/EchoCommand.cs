using System.Threading.Tasks;
using DSharpPlus.CommandAll.Commands;
using DSharpPlus.CommandAll.Commands.Attributes;
using DSharpPlus.CommandAll.Processors.TextCommands.Attributes;

namespace OoLunar.Tomoe.Commands.Common
{
    public sealed class EchoCommand
    {
        [Command("echo"), TextAlias("repeat", "say")]
        public static async Task ExecuteAsync(CommandContext context, [RemainingText] string message) => await context.RespondAsync(message);
    }
}
