using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

using Tomoe.Utils;

namespace Tomoe.Commands.Public
{
	public class Invite : BaseCommandModule
	{
		private static readonly Logger _logger = new("Commands.Public.Invite");

		[Command("invite"), Description("Sends the link to add Tomoe to a guild without an embed."), Aliases("link")]
		public async Task Mention(CommandContext context)
		{
			_logger.Debug($"Executing in channel {context.Channel.Id} on guild {context.Guild.Id}");
			_logger.Trace($"Invite link: {Utils.Config.InviteLink}");
			_ = Program.SendMessage(context, Formatter.EmbedlessUrl(new(Utils.Config.InviteLink)));
			_logger.Trace("Message sent!");
		}
	}
}
