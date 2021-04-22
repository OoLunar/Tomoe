using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Microsoft.Extensions.DependencyInjection;
using Tomoe.Db;

namespace Tomoe.Commands.Moderation
{
	[Group("reaction_roles"), Description("Assigns a role to the user(s) who react to a certain message."), Aliases("rr", "reaction_role", "reactionroles", "reactionrole"), RequireUserPermissions(Permissions.ManageRoles | Permissions.ManageMessages)]
	public class ReactionRoles : BaseCommandModule
	{
		[GroupCommand]
		public async Task ByMessage(CommandContext context, DiscordMessage message, DiscordEmoji emoji, DiscordRole role)
		{
			using IServiceScope scope = Program.ServiceProvider.CreateScope();
			Database database = scope.ServiceProvider.GetService<Database>();

			ReactionRole reactionRole = new();
			reactionRole.EmojiName = emoji.Id == 0 ? emoji.GetDiscordName() : emoji.Id.ToString();
			reactionRole.GuildId = context.Guild.Id;
			reactionRole.ChannelId = message.Channel.Id;
			reactionRole.MessageId = message.Id;
			reactionRole.RoleId = role.Id;

			ReactionRole databaseReactionRole = database.ReactionRoles.FirstOrDefault(databaseReactionRole => databaseReactionRole.GuildId == reactionRole.GuildId && databaseReactionRole.MessageId == reactionRole.MessageId && databaseReactionRole.EmojiName == reactionRole.EmojiName);
			if (databaseReactionRole != null)
			{
				_ = await Program.SendMessage(context, Formatter.Bold($"[Error]: Message {message.Id} already has the emoji {emoji} assigned to it!"));
				return;
			}

			_ = database.ReactionRoles.Add(reactionRole);
			_ = await database.SaveChangesAsync();
			await message.CreateReactionAsync(emoji);
			_ = await Program.SendMessage(context, $"Reaction role created! When someone reacts with {emoji} on <{message.JumpLink}>, they'll be given the {role.Mention} role.");
		}

		[GroupCommand]
		public async Task ByReply(CommandContext context, DiscordEmoji emoji, DiscordRole role)
		{
			if (context.Message.ReferencedMessage != null)
			{
				await ByMessage(context, context.Message.ReferencedMessage, emoji, role);
			}
			else
			{
				_ = await Program.SendMessage(context, Formatter.Bold("[Error]: Message required."));
			}
		}

		[Command("last"), Description("Assigns a role to the user(s) who react to the last message in the channel."), Aliases("last_message", "lastmessage")]
		public async Task LastMessage(CommandContext context, DiscordChannel channel, DiscordEmoji emoji, DiscordRole role) => await ByMessage(context, (await channel.GetMessagesAsync(1))[0], emoji, role);

		[Command("fix"), Description("Adds Tomoe's reactions back onto previous reaction role messages."), Aliases("repair", "rereact")]
		public async Task Fix(CommandContext context, DiscordChannel channel)
		{
			using IServiceScope scope = Program.ServiceProvider.CreateScope();
			Database database = scope.ServiceProvider.GetService<Database>();
			bool fixedReactions = false;
			foreach (ReactionRole reactionRole in database.ReactionRoles.Where(reactionRole => reactionRole.GuildId == context.Guild.Id && reactionRole.ChannelId == channel.Id))
			{
				DiscordMessage message = await channel.GetMessageAsync(reactionRole.MessageId);
				await message.CreateReactionAsync(DiscordEmoji.FromName(context.Client, reactionRole.EmojiName, true));
				fixedReactions = true;
			}
			_ = fixedReactions
				? await Program.SendMessage(context, "Successfully fixed Tomoe's reactions!")
				: await Program.SendMessage(context, Formatter.Bold($"[Error]: Failed to fix reactions or no reaction roles were found in channel {channel.Mention}"));
		}

		[Command("remove"), Description("Deletes a reaction role from a message."), Aliases("delete", "rm", "del")]
		public async Task Remove(CommandContext context, DiscordMessage message, DiscordEmoji emoji)
		{
			using IServiceScope scope = Program.ServiceProvider.CreateScope();
			Database database = scope.ServiceProvider.GetService<Database>();

			ReactionRole databaseReactionRole = database.ReactionRoles.FirstOrDefault(databaseReactionRole => databaseReactionRole.GuildId == context.Guild.Id && databaseReactionRole.MessageId == message.Id && databaseReactionRole.EmojiName == (emoji.Id == 0 ? emoji.GetDiscordName() : emoji.Id.ToString()));
			if (databaseReactionRole != null)
			{
				_ = database.ReactionRoles.Remove(databaseReactionRole);
				_ = await Program.SendMessage(context, $"Reaction role with {emoji} on <{message.JumpLink}>, will no longer be given the <@&{databaseReactionRole.RoleId}> role.");
			}
			else
			{
				_ = await Program.SendMessage(context, Formatter.Bold($"[Error]: No reaction role was found with {emoji} on message <{message.JumpLink}>"));
			}
		}
	}
}
