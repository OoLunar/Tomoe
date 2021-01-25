using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using Tomoe.Database.Interfaces;

namespace Tomoe.Commands.Moderation
{
	public class TempmemeBan : BaseCommandModule
	{
		[Command("tempantimeme"), Description("Temporarily antimemes the victim."), RequireBotPermissions(Permissions.ManageRoles), RequireUserPermissions(Permissions.ManageMessages), Aliases(new[] { "temp_antimeme", "tempanti_meme", "temp_anti_meme", "tempmemeban", "temp_memeban", "temp_meme_ban", "tempmeme_ban", })]
		public async Task Temp(CommandContext context, DiscordUser victim, ExpandedTimeSpan muteTime, [RemainingText] string memeBanReason = Program.MissingReason)
		{
			if (victim == context.Client.CurrentUser)
			{
				_ = Program.SendMessage(context, Program.SelfAction);
				return;
			}

			ulong? noMemeRoleId = Program.Database.Guild.AntiMemeRole(context.Guild.Id);
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
					if (guildVictim.Hierarchy > (await context.Guild.GetMemberAsync(context.Client.CurrentUser.Id)).Hierarchy || guildVictim.Hierarchy >= context.Member.Hierarchy)
					{
						_ = Program.SendMessage(context, Program.Hierarchy);
						return;
					}
					else if (!guildVictim.IsBot) _ = await guildVictim.SendMessageAsync($"You've been temporarily meme banned by **{context.User.Mention}** from **{context.Guild.Name}** for {muteTime.TimeSpan}. This means you cannot link embeds, send files or react. All you can do is send and read messages. Reason: ```\n{memeBanReason.Filter()}\n```");
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
			Program.Database.User.IsAntiMemed(context.Guild.Id, victim.Id, true);
			Program.Database.Assignments.Create(AssignmentType.TempMute, context.Guild.Id, context.Channel.Id, context.Message.Id, victim.Id, DateTime.Now + muteTime.TimeSpan, DateTime.Now, $"{victim.Id} tempmuted in {context.Guild.Id}");
			_ = Program.SendMessage(context, $"{victim.Mention} has been muted{(sentDm ? '.' : " (Failed to DM).")} Reason: ```\n{memeBanReason.Filter()}\n```", null, new UserMention(victim.Id));
		}
	}
}
