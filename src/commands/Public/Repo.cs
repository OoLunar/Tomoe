using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace Tomoe.Commands.Public
{
	public class Repo : BaseCommandModule
	{
		[Command("repo"), Description("Sends the source code for Tomoe.")]
		[Aliases(new[] { "github", "gh", "gitlab", "repository", "source" })]
		public async Task Get(CommandContext context) => Program.SendMessage(context, Utils.Config.RepositoryLink);
	}
}
