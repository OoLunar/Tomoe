using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace Tomoe.Commands.Public
{
	public class Support : BaseCommandModule
	{
		[Command("support"), Description("Sends the support Discord invite."), Aliases(new[] { "discord", "guild" })]
		public async Task Get(CommandContext context) => Program.SendMessage(context, "https://discord.gg/Y6JmYTNcGg");
	}
}
