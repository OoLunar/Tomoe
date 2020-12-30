using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Tomoe.Utils;

namespace Tomoe.Commands.Public
{
	public class Ping : BaseCommandModule
	{
		private static Logger Logger = new Logger("Commands.Public.Ping");

		[Command("ping"), Description("Checks the latency between the bot and the Discord API Websocket. Best used to see if the bot is lagging.")]
		public async Task Pong(CommandContext context)
		{
			Logger.Debug($"Executing in channel {context.Channel.Id} on guild {context.Guild.Id}");
			Program.SendMessage(context, $"Pong! Latency is {context.Client.Ping}ms");
			Logger.Trace("Message sent!");
		}
	}
}
