using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Tomoe.Utils;

namespace Tomoe.Commands.Public
{
	public class Invite : BaseCommandModule
	{
		private static readonly Logger Logger = new("Commands.Public.Invite");

		[Command("invite"), Description("Sends the link to add Tomoe to a guild without an embed."), Aliases("link")]
		public async Task Mention(CommandContext context)
		{
			Logger.Debug($"Executing in channel {context.Channel.Id} on guild {context.Guild.Id}");
			Logger.Trace($"Invite link: {Utils.Config.InviteLink}");
			_ = Program.SendMessage(context, $"<{Utils.Config.InviteLink}>");
			Logger.Trace("Message sent!");
		}
	}
}
