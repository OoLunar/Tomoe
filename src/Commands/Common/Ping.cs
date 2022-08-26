using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace OoLunar.Tomoe.Commands.Common
{
    public sealed class Ping : BaseCommandModule
    {
        [Command("ping"), Description("Sends the latency between the bot and the Discord API.")]
        public Task PingAsync(CommandContext context) => context.RespondAsync($"Pong! Latency is {context.Client.Ping}ms.");
    }
}
