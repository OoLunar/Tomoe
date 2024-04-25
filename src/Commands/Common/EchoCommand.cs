using System.Threading.Tasks;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ArgumentModifiers;
using DSharpPlus.Commands.Trees;
using DSharpPlus.Commands.Trees.Metadata;

namespace OoLunar.Tomoe.Commands.Common
{
    public static class EchoCommand
    {
        [Command("echo"), TextAlias("repeat", "say")]
        public static ValueTask ExecuteAsync(CommandContext context, [RemainingText] string message) => context.RespondAsync(message);
    }
}
