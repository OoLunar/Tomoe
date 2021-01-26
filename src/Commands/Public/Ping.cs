using System.Threading.Tasks;

using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

using Tomoe.Utils;

namespace Tomoe.Commands.Public
{
	public class Ping : BaseCommandModule
	{
		private static readonly Logger _logger = new("Commands.Public.Ping");

		[Command("ping"), Description("Checks the latency between the bot and the Discord API Websocket. Best used to see if the bot is lagging.")]
		public async Task Overload(CommandContext context)
		{
			_logger.Debug($"Executing in channel {context.Channel.Id} on guild {context.Guild.Id}");
			_ = Program.SendMessage(context, $"Pong! Latency is {context.Client.Ping}ms");
			_logger.Trace("Message sent!");
		}
	}
}
