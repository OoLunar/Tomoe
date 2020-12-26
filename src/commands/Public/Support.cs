using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace Tomoe.Commands.Public
{
	public class Support : BaseCommandModule
	{
		private const string _COMMAND_NAME = "support";
		private const string _COMMAND_DESC = "Sends the support Discord invite.";

		[Command(_COMMAND_NAME), Description(_COMMAND_DESC), Aliases(new string[] { "discord", "guild" })]
		public async Task Get(CommandContext context) => Program.SendMessage(context, "https://discord.gg/Y6JmYTNcGg");
	}
}
