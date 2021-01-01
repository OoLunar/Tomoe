using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;

namespace Tomoe.Commands.Moderation
{
	public class MemeBan : BaseCommandModule
	{
		[Command("memeban"), Description("Prevents the victim from linking embeds, sending files or reacting to messages. All they can do is send and read messages. This is the command to use when someone is constantly spamming reactions onto messages or sending a bunch of images."), RequireBotPermissions(Permissions.ManageRoles), RequireUserPermissions(Permissions.ManageMessages), Aliases(new[] { "meme_ban", "antimeme", "anti_meme", "nomeme", "no_meme" })]
		public async Task Permanently(CommandContext context, DiscordUser victim, [RemainingText] string memeBanReason = Program.MissingReason)
		{
			if (victim == context.Client.CurrentUser)
			{
				_ = Program.SendMessage(context, Program.SelfAction);
				return;
			}

			ulong? noMemeRoleId = Program.Database.Guild.NoMemeRole(context.Guild.Id);
			if (!noMemeRoleId.HasValue)
			{
				_ = Program.SendMessage(context, Program.MissingRole);
				return;
			}

			DiscordRole noMemeRole = context.Guild.GetRole(noMemeRoleId.Value);
			if (noMemeRole == null)
			{
				_ = Program.SendMessage(context, Program.MissingRole);
				return;
			}

			bool sentDm = true;

			try
			{
				DiscordMember guildVictim = await context.Guild.GetMemberAsync(victim.Id);
				try
				{
					if (guildVictim.Hierarchy > context.Guild.CurrentMember.Hierarchy)
					{
						_ = Program.SendMessage(context, Program.Hierarchy);
						return;
					}
					else if (!guildVictim.IsBot)
					{
						_ = await guildVictim.SendMessageAsync($"You've been meme banned by **{context.User.Mention}** from **{context.Guild.Name}**. This means you cannot link embeds, send files or react. All you can do is send and read messages. Reason: ```\n{memeBanReason.Filter()}\n```");
					}
				}
				catch (UnauthorizedException)
				{
					sentDm = false;
				}
				await guildVictim.GrantRoleAsync(noMemeRole, memeBanReason);
			}
			catch (NotFoundException)
			{
				sentDm = false;
			}

			Program.Database.User.IsNoMemed(context.Guild.Id, victim.Id, true);
			_ = Program.SendMessage(context, $"{victim.Mention} has been meme banned{(sentDm ? '.' : " (Failed to DM).")} Reason: ```\n{memeBanReason.Filter()}\n```", ExtensionMethods.FilteringAction.CodeBlocksIgnore, new List<IMention>() { new UserMention(victim.Id) });
		}
	}
}
