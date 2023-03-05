using System.Threading.Tasks;
using DSharpPlus.CommandAll.Attributes;
using DSharpPlus.CommandAll.Commands;

namespace OoLunar.Tomoe.Commands.Common
{
    public sealed class PingCommand : BaseCommand
    {
        [Command("ping", "pong")]
        public static Task ExecuteAsync(CommandContext context) => context.ReplyAsync($"Pong! Latency is {context.Client.Ping}ms.");
    }
}
