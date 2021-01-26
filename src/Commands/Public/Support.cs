using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

using Tomoe.Utils;

namespace Tomoe.Commands.Public
{
	public class Support : BaseCommandModule
	{
		private static readonly Logger _logger = new("Commands.Public.Support");

		[Command("support"), Description("Sends the support Discord invite."), Aliases(new[] { "discord", "guild" })]
		public async Task Overload(CommandContext context)
		{
			if (!context.Channel.IsPrivate) _logger.Debug($"Executing in channel {context.Channel.Id} on guild {context.Guild.Id}");
			_ = Program.SendMessage(context, Formatter.EmbedlessUrl(new("https://discord.gg/Y6JmYTNcGg")));
			_logger.Trace("Message sent!");
		}
	}
}
