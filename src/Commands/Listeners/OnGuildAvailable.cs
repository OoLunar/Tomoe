using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.EventArgs;

using Tomoe.Utils;

namespace Tomoe.Commands.Listeners
{
	public class GuildAvailable
	{
		private static readonly Logger _logger = new("Commands.Listeners.GuildAvailable");

		public static async Task Handler(DiscordClient _client, GuildCreateEventArgs eventArgs)
		{
			if (!Program.Database.Guild.GuildIdExists(eventArgs.Guild.Id)) Program.Database.Guild.InsertGuildId(eventArgs.Guild.Id);
			_logger.Info($"\"{eventArgs.Guild.Name}\" ({eventArgs.Guild.Id}) is ready! Handling {eventArgs.Guild.MemberCount} members.");
		}
	}
}