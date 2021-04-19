using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Tomoe.Db;

namespace Tomoe.Commands.Listeners
{
	public class MessageReactionAdded
	{
		private static readonly ILogger _logger = Log.ForContext<GuildAvailable>();

		/// <summary>
		/// Used to add the guild to the database and log when the guild is available.
		/// </summary>
		/// <param name="_client">Unused <see cref="DiscordClient"/>.</param>
		/// <param name="eventArgs">Used to get the guild id and guild name.</param>
		/// <returns></returns>
		public static async Task Handler(DiscordClient _client, MessageReactionAddEventArgs eventArgs)
		{
			if (eventArgs.User.IsBot)
			{
				return;
			}
			string emojiName = eventArgs.Emoji.Id == 0 ? eventArgs.Emoji.GetDiscordName() : eventArgs.Emoji.Id.ToString();

			using IServiceScope scope = Program.ServiceProvider.CreateScope();
			Database database = scope.ServiceProvider.GetService<Database>();
			ReactionRole reactionRole = database.ReactionRoles.FirstOrDefault(databaseReactionRole => databaseReactionRole.GuildId == eventArgs.Guild.Id && databaseReactionRole.MessageId == eventArgs.Message.Id && databaseReactionRole.EmojiName == (eventArgs.Emoji.Id == 0 ? eventArgs.Emoji.GetDiscordName() : eventArgs.Emoji.Id.ToString()));
			if (reactionRole == null)
			{
				return;
			}
			DiscordRole discordRole = eventArgs.Guild.GetRole(reactionRole.RoleId);
			// if the discord role has been removed, remove the reaction role from the database.
			if (discordRole == null)
			{
				_ = database.ReactionRoles.Remove(reactionRole);
				_ = await database.SaveChangesAsync();
				return;
			}
			await (await eventArgs.User.Id.GetMember(eventArgs.Guild)).GrantRoleAsync(discordRole);
		}
	}
}
