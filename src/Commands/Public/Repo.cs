using System.Threading.Tasks;

using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

using Tomoe.Utils;

namespace Tomoe.Commands.Public
{
	public class Repository : BaseCommandModule
	{
		private static readonly Logger _logger = new("Commands.Public.Repository");
		[Command("repository"), Description("Sends the source code for Tomoe."), Aliases("github", "gh", "gitlab", "repo")]
		public async Task Get(CommandContext context)
		{
			_logger.Debug($"Executing in channel {context.Channel.Id} on guild {context.Guild.Id}");
			_ = Program.SendMessage(context, Utils.Config.RepositoryLink);
			_logger.Trace("Message sent!");
		}
	}
}
