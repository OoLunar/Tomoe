using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Microsoft.Extensions.DependencyInjection;
using Tomoe.Db;

namespace Tomoe.Commands.Public
{
	public class ReactionRoles : BaseCommandModule
	{
		// TODO: Make "last message in channel" overload
		// TODO: Make "fix reactions" overload
		// TODO: Make "rr in bulk" overload

		[Command("reaction_roles"), Description("Assigns a role to the user(s) who react to a certain message."), Aliases("rr", "reaction_role", "reactionroles", "reactionrole"), RequireUserPermissions(Permissions.ManageRoles | Permissions.ManageMessages)]
		public async Task Overload(CommandContext context, DiscordMessage message, DiscordEmoji emoji, DiscordRole role)
		{
			using IServiceScope scope = Program.ServiceProvider.CreateScope();
			Database database = scope.ServiceProvider.GetService<Database>();
			ReactionRole databaseReactionRole = database.ReactionRoles.FirstOrDefault(databaseReactionRole => databaseReactionRole.GuildId == context.Guild.Id && databaseReactionRole.MessageId == message.Id && databaseReactionRole.EmojiName == (emoji.Id == 0 ? emoji.GetDiscordName() : emoji.Id.ToString()));
			if (databaseReactionRole != null)
			{
				_ = await Program.SendMessage(context, $"Message {message.Id} already has the emoji {emoji} assigned to it!");
				return;
			}
			ReactionRole reactionRole = new();
			reactionRole.GuildId = context.Guild.Id;
			reactionRole.MessageId = message.Id;
			reactionRole.RoleId = role.Id;
			reactionRole.EmojiName = emoji.Id == 0 ? emoji.GetDiscordName() : emoji.Id.ToString();
			_ = database.ReactionRoles.Add(reactionRole);
			_ = await database.SaveChangesAsync();
			await message.CreateReactionAsync(emoji);
			_ = await Program.SendMessage(context, $"Role {role.Mention} will be added to the users role list when they react with {emoji}!");
		}
	}
}
