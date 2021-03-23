using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;
using Tomoe.Db;

namespace Tomoe.Commands.Moderation
{
	[Group("reaction_roles"), RequireGuild, Description("Allows assigning and removing reaction roles to a message."), RequireBotPermissions(Permissions.ManageRoles), RequireUserPermissions(Permissions.AddReactions | Permissions.ManageMessages | Permissions.ManageRoles), Aliases("reactionroles", "rr")]
	public class ReactionRoles : BaseCommandModule
	{
		public Database Database { private get; set; }

		[Command("add")]
		public async Task Add(CommandContext context, DiscordEmoji discordEmoji, DiscordRole discordRole) => await Add(context, context.Message.ReferencedMessage, discordEmoji, discordRole);

		[Command("add"), Aliases("assign")]
		public async Task Add(CommandContext context, DiscordMessage discordMessage, DiscordEmoji discordEmoji, DiscordRole discordRole)
		{
			if (discordMessage == null)
			{
				_ = await Program.SendMessage(context, Formatter.Bold("[Error: No message provided!]"));
				return;
			}

			Guild guild = await Database.Guilds.FirstOrDefaultAsync(guild => guild.Id == context.Guild.Id);
			if (guild == null)
			{
				_ = await Program.SendMessage(context, Constants.GuildNotInDatabase);
				return;
			}

			if (discordRole.Position >= context.Guild.CurrentMember.Hierarchy)
			{
				_ = await Program.SendMessage(context, Formatter.Bold($"[Error: Failed to create reaction role, cannot assign {discordRole.Mention} role.]"));
				return;
			}

			ReactionRole reactionRole = guild.ReactionRoles.FirstOrDefault(rr => rr.MessageId == discordMessage.Id && (rr.IsDefaultEmoji ? (rr.EmojiName == discordEmoji.Name) : (rr.EmojiId == discordEmoji.Id)));
			if (reactionRole != null)
			{
				_ = await Program.SendMessage(context, $"Emoji {discordEmoji} is already in use on message {discordMessage.JumpLink}! Find a different emoji.");
				return;
			}

			reactionRole = new();
			reactionRole.RoleId = discordRole.Id;
			reactionRole.MessageId = discordMessage.Id;
			if (discordEmoji.Id == 0)
			{
				reactionRole.EmojiName = discordEmoji.Name;
				reactionRole.IsDefaultEmoji = true;
			}
			else
			{
				reactionRole.EmojiId = discordEmoji.Id;
			}

			guild.ReactionRoles.Add(reactionRole);
			await discordMessage.CreateReactionAsync(discordEmoji);
			_ = await Database.SaveChangesAsync();
			_ = await Program.SendMessage(context, $"Role {discordRole.Mention} will now be assigned whenever someone reacts with {discordEmoji} on {Formatter.EmbedlessUrl(discordMessage.JumpLink)}.");
		}

		[Command("remove"), Aliases("unassign")]
		public async Task Remove(CommandContext context, DiscordMessage discordMessage, DiscordEmoji discordEmoji)
		{
			Guild guild = await Database.Guilds.FirstOrDefaultAsync(guild => guild.Id == context.Guild.Id);
			if (guild == null)
			{
				_ = await Program.SendMessage(context, Constants.GuildNotInDatabase);
				return;
			}
			ReactionRole reactionRole = guild.ReactionRoles.FirstOrDefault(rr => rr.MessageId == discordMessage.Id && (rr.IsDefaultEmoji ? (rr.EmojiName == discordEmoji.Name) : (rr.EmojiId == discordEmoji.Id)));
			if (reactionRole == null)
			{
				_ = await Program.SendMessage(context, $"Reaction role with emoji {discordEmoji} on message {Formatter.EmbedlessUrl(discordMessage.JumpLink)} could not be found.");
				return;
			}

			await discordMessage.DeleteOwnReactionAsync(discordEmoji);
			_ = guild.ReactionRoles.Remove(reactionRole);
			_ = await Database.SaveChangesAsync();
			_ = await Program.SendMessage(context, "Reaction role successfully removed!");
		}
	}
}
