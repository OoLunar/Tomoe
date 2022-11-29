using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace Tomoe.Commands.Common
{
	public class Ping : BaseCommandModule
	{
		[Command("ping"), Description("Checks the latency between the bot and the Discord API Websocket. Best used to see if the bot is lagging.")]
		public Task PingAsync(CommandContext context) => context.RespondAsync($"Pong! Latency is {context.Client.Ping}ms");
	}
}
