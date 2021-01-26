using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

using Tomoe.Utils;

namespace Tomoe.Commands.Public
{
	public class Raw : BaseCommandModule
	{
		private static readonly Logger _logger = new("Commands.Public.Raw");
		[Command("raw"), Description("Gets the raw version of the message provided."), Aliases("source")]
		public async Task Overload(CommandContext context, [Description("The message id or jumplink to the message.")] DiscordMessage message)
		{
			_logger.Debug($"Executing in channel {context.Channel.Id} on guild {context.Guild.Id}");
			if (message.Content == string.Empty && message.Embeds.Count != 0)
			{
				_logger.Trace("Refusing to get contents of just an embed...");
				_ = Program.SendMessage(context, Constants.RawEmbed);
			}
			else
			{
				_logger.Trace($"Escaping characters...");
				_logger.Trace("Escaped characters!");
				_ = Program.SendMessage(context, $"{Formatter.Sanitize(message.Content)}{(message.Embeds.Count != 0 ? '\n' + Constants.RawEmbed : null)}");
				_logger.Trace("Message sent!");
			}
		}
	}
}
