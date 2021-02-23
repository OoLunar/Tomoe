using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace Tomoe.Commands.Public
{
	public class Support : BaseCommandModule
	{
		[Command("support"), Description("Sends the support Discord invite."), Aliases(new[] { "discord", "guild" })]
		public async Task Overload(CommandContext context) => await Program.SendMessage(context, Formatter.EmbedlessUrl(new("https://discord.gg/Y6JmYTNcGg")));
	}
}
