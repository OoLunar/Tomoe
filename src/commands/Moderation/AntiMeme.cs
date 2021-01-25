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
		[Command("antimeme"), Description("Prevents the victim from linking embeds, sending files or reacting to messages. All they can do is send and read messages. This is the command to use when someone is constantly spamming reactions onto messages or sending a bunch of images."), RequireBotPermissions(Permissions.ManageRoles), RequireUserPermissions(Permissions.ManageMessages), Aliases(new[] { "anti_meme", "meme_ban", "memeban", "nomeme", "no_meme" })]
		public async Task Permanently(CommandContext context, DiscordUser victim, [RemainingText] string antiMemeReason = Program.MissingReason)
		{
			if (victim == context.Client.CurrentUser)
			{
				_ = Program.SendMessage(context, Program.SelfAction);
				return;
			}

			ulong? antiMemeRoleId = Program.Database.Guild.AntiMemeRole(context.Guild.Id);
			if (!antiMemeRoleId.HasValue)
			{
				_ = Program.SendMessage(context, Program.MissingRole);
				return;
			}

			DiscordRole antiMemeRole = context.Guild.GetRole(antiMemeRoleId.Value);
			if (antiMemeRole == null)
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
					if (guildVictim.Hierarchy > (await context.Guild.GetMemberAsync(context.Client.CurrentUser.Id)).Hierarchy || guildVictim.Hierarchy >= context.Member.Hierarchy)
					{
						_ = Program.SendMessage(context, Program.Hierarchy);
						return;
					}
					else if (!guildVictim.IsBot) _ = await guildVictim.SendMessageAsync($"You've been antimemed by **{context.User.Mention}** from **{context.Guild.Name}**. This means you cannot link embeds, send files or react. All you can do is send and read messages. Reason: ```\n{antiMemeReason.Filter()}\n```");

				}
				catch (UnauthorizedException)
				{
					sentDm = false;
				}
				await guildVictim.GrantRoleAsync(antiMemeRole, antiMemeReason);
			}
			catch (NotFoundException)
			{
				sentDm = false;
			}

			Program.Database.User.IsAntiMemed(context.Guild.Id, victim.Id, true);
			_ = Program.SendMessage(context, $"{victim.Mention} has been antimemed{(sentDm ? '.' : " (Failed to DM).")} Reason: ```\n{antiMemeReason.Filter()}\n```", null, new UserMention(victim.Id));
		}
	}
}
