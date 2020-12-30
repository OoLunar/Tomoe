using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using Tomoe.Utils;

namespace Tomoe.Commands.Public
{
	public class GuildIcon : BaseCommandModule
	{
		private static readonly Logger Logger = new Logger("Commands.Public.GuildIcon");

		[Command("guildicon"), Description("Gets the guild's icon."), Aliases(new[] { "guild_icon", "guildpfp", "guild_pfp" })]
		public async Task Get(CommandContext context)
		{
			Logger.Debug($"Executing in channel {context.Channel.Id} on guild {context.Guild.Id}");
			string guildIcon = context.Guild.IconUrl ?? "No custom icon set.";
			Logger.Trace($"{context.Guild.Id}'s guild icon: {guildIcon}");
			_ = Program.SendMessage(context, guildIcon.Replace(".jpg", ".png?size=512"));
			Logger.Trace("Message sent!");
		}
	}
}
