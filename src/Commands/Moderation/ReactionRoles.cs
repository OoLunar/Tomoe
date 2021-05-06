using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using Tomoe.Db;
using Tomoe.Utils.Types;

namespace Tomoe.Commands.Moderation
{
	[Group("reaction_roles"), Description("Assigns a role to the user(s) who react to a certain message."), Aliases("rr", "reaction_role", "reactionroles", "reactionrole"), RequireUserPermissions(Permissions.ManageRoles | Permissions.ManageMessages)]
	public class ReactionRoles : BaseCommandModule
	{
		public Database Database { private get; set; }

		[GroupCommand]
		public async Task ByMessage(CommandContext context, [Description("The message to put a reaction role on.")] DiscordMessage message, [Description("The emoji that's with the role.")] DiscordEmoji emoji, [Description("The role to assign to the user.")] DiscordRole role)
		{
			ReactionRole reactionRole = new();
			reactionRole.EmojiName = emoji.GetDiscordName();
			reactionRole.GuildId = context.Guild.Id;
			reactionRole.ChannelId = message.Channel.Id;
			reactionRole.MessageId = message.Id;
			reactionRole.RoleId = role.Id;

			ReactionRole databaseReactionRole = Database.ReactionRoles.FirstOrDefault(databaseReactionRole => databaseReactionRole.GuildId == reactionRole.GuildId && databaseReactionRole.MessageId == reactionRole.MessageId && databaseReactionRole.EmojiName == reactionRole.EmojiName);
			if (databaseReactionRole != null)
			{
				_ = await Program.SendMessage(context, Formatter.Bold($"[Error]: Message {message.Id} already has the emoji {emoji} assigned to it!"));
				return;
			}

			_ = Database.ReactionRoles.Add(reactionRole);
			_ = await Database.SaveChangesAsync();
			await message.CreateReactionAsync(emoji);
			_ = await Program.SendMessage(context, $"Reaction role created! When someone reacts with {emoji} on <{message.JumpLink}>, they'll be given the {role.Mention} role.");
		}

		[GroupCommand]
		public async Task ByReply(CommandContext context, [Description("The emoji that's with the role.")] DiscordEmoji emoji, [Description("The role to assign to the user.")] DiscordRole role)
		{
			if (context.Message.ReferencedMessage != null)
			{
				await ByMessage(context, context.Message.ReferencedMessage, emoji, role);
			}
			else
			{
				_ = await Program.SendMessage(context, Formatter.Bold("[Error]: Message reply required."));
			}
		}

		[Command("last"), Description("Assigns a role to the user(s) who react to the last message in the channel."), Aliases("last_message", "lastmessage")]
		public async Task LastMessage(CommandContext context, [Description("Which channel to get the last message from.")] DiscordChannel channel, [Description("The emoji that's with the role.")] DiscordEmoji emoji, [Description("The role to assign to the user.")] DiscordRole role) => await ByMessage(context, (await channel.GetMessagesAsync(1))[0], emoji, role);

		[Command("fix"), Description("Adds Tomoe's reactions back onto previous reaction role messages."), Aliases("repair", "rereact")]
		public async Task Fix(CommandContext context, [Description("Gets all reaction roles in this channel.")] DiscordChannel channel)
		{
			List<string> checklistItems = new();
			List<DiscordEmoji> emojis = new();
			ReactionRole[] reactionRoles = Database.ReactionRoles.Where(reactionRole => reactionRole.GuildId == context.Guild.Id && reactionRole.ChannelId == channel.Id).OrderBy(reactionRole => reactionRole.Id).ToArray();

			if (reactionRoles.Length == 0)
			{
				_ = await Program.SendMessage(context, Formatter.Bold("[Error]: No reaction roles found!"));
				return;
			}

			foreach (ReactionRole reactionRole in reactionRoles)
			{
				DiscordEmoji emoji = DiscordEmoji.FromName(context.Client, reactionRole.EmojiName, true);
				emojis.Add(emoji);
				checklistItems.Add($"React to {reactionRole.MessageId} with {emoji}");
				checklistItems.Add($"Assign <@&{reactionRole.RoleId}> to those who previously reacted.");
			}

			Checklist checklist = new(context, checklistItems.ToArray());

			for (int i = 0; i < reactionRoles.Length; i++)
			{
				ReactionRole reactionRole = reactionRoles[i];
				DiscordMessage message = await channel.GetMessageAsync(reactionRole.MessageId);
				await message.CreateReactionAsync(emojis[i]);
				await checklist.Check();
				DiscordRole discordRole = context.Guild.GetRole(reactionRole.RoleId);
				if (discordRole == null)
				{
					_ = Database.ReactionRoles.Remove(reactionRole);
					_ = await Database.SaveChangesAsync();
					await checklist.Fail();
				}

				foreach (DiscordUser discordUser in await message.GetReactionsAsync(emojis[i], 100000))
				{
					DiscordMember discordMember = await discordUser.Id.GetMember(context.Guild);
					if (discordMember != null && !discordMember.Roles.Contains(discordRole))
					{
						await discordMember.GrantRoleAsync(discordRole);
					}
				}
				await checklist.Check();
			}
			await checklist.Finalize("Reaction roles fixed! Everyone has recieved their roles!", false);
			checklist.Dispose();
		}

		[Command("list"), Description("Shows all the current reaction roles on a message"), Aliases("show", "ls")]
		public async Task List(CommandContext context, [Description("Gets all reaction roles on this message.")] DiscordMessage message)
		{
			StringBuilder stringBuilder = new();
			foreach (ReactionRole reactionRole in Database.ReactionRoles.Where(reactionRole => reactionRole.MessageId == message.Id && reactionRole.GuildId == context.Guild.Id))
			{
				_ = stringBuilder.AppendLine($"{DiscordEmoji.FromName(context.Client, reactionRole.EmojiName, true)} => {context.Guild.GetRole(reactionRole.RoleId).Mention}");
			}

			DiscordEmbedBuilder embedBuilder = new DiscordEmbedBuilder().GenerateDefaultEmbed(context, $"Reaction Roles on Message");
			embedBuilder.Title += ' ' + context.Message.JumpLink.ToString();
			if (stringBuilder.Length <= 2000)
			{
				embedBuilder.Description = stringBuilder.ToString();
				_ = await Program.SendMessage(context, null, embedBuilder.Build());
			}
			else
			{
				InteractivityExtension interactivity = context.Client.GetInteractivity();
				IEnumerable<Page> pages = interactivity.GeneratePagesInEmbed(stringBuilder.ToString(), DSharpPlus.Interactivity.Enums.SplitType.Line, embedBuilder);
				await interactivity.SendPaginatedMessageAsync(context.Channel, context.User, pages);
			}
		}

		[Command("remove"), Description("Deletes a reaction role from a message."), Aliases("delete", "rm", "del")]
		public async Task Remove(CommandContext context, [Description("Which message to remove the reaction role from.")] DiscordMessage message, [Description("The emoji that's with the role.")] DiscordEmoji emoji)
		{
			ReactionRole databaseReactionRole = Database.ReactionRoles.FirstOrDefault(databaseReactionRole => databaseReactionRole.GuildId == context.Guild.Id && databaseReactionRole.MessageId == message.Id && databaseReactionRole.EmojiName == (emoji.Id == 0 ? emoji.GetDiscordName() : emoji.Id.ToString()));
			if (databaseReactionRole != null)
			{
				_ = Database.ReactionRoles.Remove(databaseReactionRole);
				_ = await Program.SendMessage(context, $"Reaction role with {emoji} on <{message.JumpLink}>, will no longer be given the <@&{databaseReactionRole.RoleId}> role.");
			}
			else
			{
				_ = await Program.SendMessage(context, Formatter.Bold($"[Error]: No reaction role was found with {emoji} on message <{message.JumpLink}>"));
			}
		}
	}
}
