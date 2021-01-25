using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using Tomoe.Database.Interfaces;
using Tomoe.Commands.Moderation.Attributes;

namespace Tomoe.Commands.Moderation
{
	public class TempmemeBan : BaseCommandModule
	{
		[Command("tempantimeme"), Description("Temporarily antimemes the victim."), RequireBotPermissions(Permissions.ManageRoles), RequireUserPermissions(Permissions.ManageMessages), Aliases("temp_antimeme", "tempanti_meme", "temp_anti_meme", "tempmemeban", "temp_memeban", "temp_meme_ban", "tempmeme_ban"), Punishment(true)]
		public async Task Temp(CommandContext context, DiscordUser victim, ExpandedTimeSpan muteTime, [RemainingText] string memeBanReason = Program.MissingReason)
		{
			DiscordRole antiMemeRole = Program.Database.Guild.AntiMemeRole(context.Guild.Id).GetRole(context.Guild);
			if (antiMemeRole == null)
			{
				_ = Program.SendMessage(context, Program.MissingRole);
				return;
			}

			bool sentDm = false;
			DiscordMember guildVictim = await context.Guild.GetMemberAsync(victim.Id);
			if (guildVictim != null)
			{
				try
				{
					if (!guildVictim.IsBot) await guildVictim.SendMessageAsync($"You've been temporarily meme banned by **{context.User.Mention}** from **{context.Guild.Name}** for {muteTime.TimeSpan}. This means you cannot link embeds, send files or react. All you can do is send and read messages. Reason: ```\n{memeBanReason}\n```");
					sentDm = true;
				}
				catch (UnauthorizedException) { }
				await guildVictim.GrantRoleAsync(antiMemeRole, memeBanReason);
			}
			Program.Database.User.IsAntiMemed(context.Guild.Id, victim.Id, true);
			Program.Database.Assignments.Create(AssignmentType.TempMute, context.Guild.Id, context.Channel.Id, context.Message.Id, victim.Id, DateTime.Now + muteTime.TimeSpan, DateTime.Now, $"{victim.Id} tempmuted in {context.Guild.Id}");
			_ = Program.SendMessage(context, $"{victim.Mention} has been muted{(sentDm ? '.' : " (Failed to DM).")} Reason: ```\n{memeBanReason}\n```", null, new UserMention(victim.Id));
		}
	}
}
