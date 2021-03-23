using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace Tomoe.Commands.Listeners
{
	public class OnReady
	{
		/// <summary>
		/// Changes the bot status whenever everything is ready.
		/// </summary>
		/// <param name="client">Used to change the bot's status.</param>
		/// <param name="_eventArgs">Unused <see cref="ReadyEventArgs"/>.</param>
		/// <returns></returns>
		public static async Task Handler(DiscordClient client, ReadyEventArgs _eventArgs) => await client.UpdateStatusAsync(new DiscordActivity("for bad things", ActivityType.Watching), UserStatus.Online);
	}
}
