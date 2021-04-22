using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Serilog;

namespace Tomoe.Commands.Listeners
{
	public class GuildDownloadCompleted
	{
		private static readonly ILogger _logger = Log.ForContext<GuildDownloadCompleted>();
		internal static int MemberCount;

		/// <summary>
		/// Changes the bot status whenever everything is ready.
		/// </summary>
		/// <param name="client">Used to change the bot's status.</param>
		/// <param name="_eventArgs">Unused <see cref="ReadyEventArgs"/>.</param>
		public static async Task Handler(DiscordClient client, GuildDownloadCompletedEventArgs eventArgs)
		{
			_logger.Information($"Ready! Handling {eventArgs.Guilds.Count} guilds and {MemberCount} guild members");
			await client.UpdateStatusAsync(new DiscordActivity("for bad things", ActivityType.Watching), UserStatus.Online);
		}
	}
}
