using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace Tomoe.Commands.Public
{
	public class Repo : BaseCommandModule
	{
		private const string _COMMAND_NAME = "repo";
		private const string _COMMAND_DESC = "Sends the source code for Tomoe.";

		[Command(_COMMAND_NAME), Description(_COMMAND_DESC)]
		[Aliases(new string[] { "github", "gh", "gitlab", "repository" })]
		public async Task Get(CommandContext context) => Program.SendMessage(context, Utils.Config.RepositoryLink);
	}
}
