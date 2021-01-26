using System.Threading.Tasks;

using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

using Tomoe.Utils;

namespace Tomoe.Commands.Public
{
	public class GuildIcon : BaseCommandModule
	{
		private static readonly Logger _logger = new("Commands.Public.GuildIcon");

		[Command("guildicon"), Description("Gets the guild's icon."), Aliases("guild_icon", "guildpfp", "guild_pfp")]
		public async Task Overload(CommandContext context)
		{
			_logger.Debug($"Executing in channel {context.Channel.Id} on guild {context.Guild.Id}");
			string guildIcon = context.Guild.IconUrl ?? "No custom icon set.";
			_logger.Trace($"{context.Guild.Id}'s guild icon: {guildIcon}");
			_ = Program.SendMessage(context, guildIcon.Replace(".jpg", ".png?size=1024"));
			_logger.Trace("Message sent!");
		}
	}
}
