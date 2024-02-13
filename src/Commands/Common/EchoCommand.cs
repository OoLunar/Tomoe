using System.Threading.Tasks;
using DSharpPlus.Commands;
using DSharpPlus.Commands.Processors.TextCommands.Attributes;
using DSharpPlus.Commands.Trees;
using DSharpPlus.Commands.Trees.Attributes;

namespace OoLunar.Tomoe.Commands.Common
{
    public sealed class EchoCommand
    {
        [Command("echo"), TextAlias("repeat", "say")]
        public static ValueTask ExecuteAsync(CommandContext context, [RemainingText] string message) => context.RespondAsync(message);
    }
}
