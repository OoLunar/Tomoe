using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Tomoe.Utils;

namespace Tomoe.Commands.Public
{
	public class Repository : BaseCommandModule
	{
		private static readonly Logger Logger = new("Commands.Public.Repository");
		[Command("repository"), Description("Sends the source code for Tomoe."), Aliases(new[] { "github", "gh", "gitlab", "repo", "source" })]
		public async Task Get(CommandContext context)
		{
			Logger.Debug($"Executing in channel {context.Channel.Id} on guild {context.Guild.Id}");
			_ = Program.SendMessage(context, Utils.Config.RepositoryLink);
			Logger.Trace("Message sent!");
		}
	}
}
