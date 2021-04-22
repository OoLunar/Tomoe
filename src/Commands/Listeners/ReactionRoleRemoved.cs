using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.DependencyInjection;
using Tomoe.Db;

namespace Tomoe.Commands.Listeners
{
	public class ReactionRoleRemoved
	{
		public static async Task Handler(DiscordClient client, MessageReactionRemoveEventArgs eventArgs)
		{
			if (eventArgs.User.Id == client.CurrentUser.Id)
			{
				return;
			}
			using IServiceScope scope = Program.ServiceProvider.CreateScope();
			Database database = scope.ServiceProvider.GetService<Database>();

			string emojiName = eventArgs.Emoji.Id == 0 ? eventArgs.Emoji.GetDiscordName() : eventArgs.Emoji.Id.ToString();
			ReactionRole reactionRole = database.ReactionRoles.FirstOrDefault(databaseReactionRole
				=> databaseReactionRole.GuildId == eventArgs.Guild.Id
				&& databaseReactionRole.MessageId == eventArgs.Message.Id
				&& databaseReactionRole.EmojiName == emojiName
			);
			// Reaction role doesn't exist, meaning it's just a random reaction.
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

			// Get the user and remove their reaction role.
			await (await eventArgs.User.Id.GetMember(eventArgs.Guild)).RevokeRoleAsync(discordRole);
		}
	}
}
