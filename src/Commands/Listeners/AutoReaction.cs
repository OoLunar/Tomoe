using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.DependencyInjection;
using Tomoe.Db;

namespace Tomoe.Utils
{
	public class AutoReactionListener
	{
		public static async Task Handler(DiscordClient client, MessageCreateEventArgs eventArgs)
		{
			using IServiceScope scope = Program.ServiceProvider.CreateScope();
			Database database = scope.ServiceProvider.GetService<Database>();
			foreach (AutoReaction autoReaction in database.AutoReactions.Where(autoReaction => autoReaction.GuildId == eventArgs.Guild.Id && autoReaction.ChannelId == eventArgs.Channel.Id))
			{
				await eventArgs.Message.CreateReactionAsync(DiscordEmoji.FromName(client, autoReaction.EmojiName, true));
			}
		}
	}
}
