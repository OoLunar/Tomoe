using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.EventArgs;
using Serilog;

namespace Tomoe.Commands.Listeners
{
	public class GuildAvailable
	{
		private static readonly ILogger _logger = Log.ForContext<GuildAvailable>();

		/// <summary>
		/// Used to add the guild to the database and log when the guild is available.
		/// </summary>
		/// <param name="_client">Unused <see cref="DiscordClient"/>.</param>
		/// <param name="eventArgs">Used to get the guild id and guild name.</param>
		/// <returns></returns>
		public static Task Handler(DiscordClient _client, GuildCreateEventArgs eventArgs)
		{
			GuildDownloadCompleted.MemberCount += eventArgs.Guild.MemberCount;
			_logger.Information($"\"{eventArgs.Guild.Name}\" ({eventArgs.Guild.Id}) is ready! Handling {eventArgs.Guild.MemberCount} members.");
			return Task.CompletedTask;
		}
	}
}
