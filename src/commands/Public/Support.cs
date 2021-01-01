using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Tomoe.Utils;

namespace Tomoe.Commands.Public
{
	public class Support : BaseCommandModule
	{
		private static readonly Logger Logger = new Logger("Commands.Public.Support");

		[Command("support"), Description("Sends the support Discord invite."), Aliases(new[] { "discord", "guild" })]
		public async Task Get(CommandContext context)
		{
			if (!context.Channel.IsPrivate) Logger.Debug($"Executing in channel {context.Channel.Id} on guild {context.Guild.Id}");
			_ = Program.SendMessage(context, "https://discord.gg/Y6JmYTNcGg");
			Logger.Trace("Message sent!");
		}
	}
}
