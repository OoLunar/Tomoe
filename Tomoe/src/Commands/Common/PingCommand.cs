using System.Threading.Tasks;
using OoLunar.DSharpPlus.CommandAll.Attributes;
using OoLunar.DSharpPlus.CommandAll.Commands;

namespace OoLunar.Tomoe.Commands.Common
{
    public sealed class PingCommand : BaseCommand
    {
        [Command("ping")]
        public static Task ExecuteAsync(CommandContext context) => context.ReplyAsync(new() { Content = $"Pong! Latency is {context.Client.Ping}ms." });
    }
}
