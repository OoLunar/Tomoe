using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace Tomoe.Commands.Public
{
	public class Invite : BaseCommandModule
	{
		[Command("invite"), Description("Sends the link to add Tomoe to a guild without an embed."), Aliases("link")]
		public async Task Overload(CommandContext context) => await Program.SendMessage(context, Formatter.EmbedlessUrl(new(Utils.Config.InviteLink)));
	}
}
