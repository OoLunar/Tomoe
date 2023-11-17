using System.Threading.Tasks;
using DSharpPlus.Commands.Trees;
using DSharpPlus.Commands.Trees.Attributes;
using DSharpPlus.Commands.Processors.TextCommands.Attributes;

namespace OoLunar.Tomoe.Commands.Common
{
    public sealed class PingCommand
    {
        [Command("ping"), TextAlias("pong")]
        public static async Task ExecuteAsync(CommandContext context) => await context.RespondAsync($"Pong! Latency is {context.Client.Ping}ms.");
    }
}
