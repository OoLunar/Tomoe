using System.Linq;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

using Tomoe.Db;
using Tomoe.Utils;

namespace Tomoe.Commands.Listeners
{
	public class GuildAvailable
	{
		private static readonly Logger _logger = new("Commands.Listeners.GuildAvailable");

		public static async Task Handler(DiscordClient _client, GuildCreateEventArgs eventArgs) => _logger.Info($"\"{eventArgs.Guild.Name}\" ({eventArgs.Guild.Id}) is ready! Handling {eventArgs.Guild.MemberCount} members.");
	}
}
