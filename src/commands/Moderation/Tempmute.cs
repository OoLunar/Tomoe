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
	public class TempMute : BaseCommandModule
	{
		[Command("tempmute"), Description("Mutes a person temporarily."), RequireBotPermissions(Permissions.ManageRoles), RequireUserPermissions(Permissions.ManageMessages), Aliases("temp_mute", "tempsilence", "temp_silence"), Punishment(true)]
		public async Task Temp(CommandContext context, DiscordUser victim, ExpandedTimeSpan muteTime, [RemainingText] string muteReason = Program.MissingReason)
		{
			DiscordRole muteRole = Program.Database.Guild.MuteRole(context.Guild.Id).GetRole(context.Guild);
			if (muteRole == null)
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
					if (!guildVictim.IsBot) _ = await guildVictim.SendMessageAsync($"You've been tempmuted by **{context.User.Mention}** from **{context.Guild.Name}** for {muteTime.TimeSpan}. Reason: ```\n{muteReason}\n```");
				}
				catch (UnauthorizedException) { }
				await guildVictim.GrantRoleAsync(muteRole, muteReason);
			}

			Program.Database.User.IsMuted(context.Guild.Id, victim.Id, true);
			Program.Database.Assignments.Create(AssignmentType.TempMute, context.Guild.Id, context.Channel.Id, context.Message.Id, victim.Id, DateTime.Now + muteTime.TimeSpan, DateTime.Now, $"{victim.Id} tempmuted in {context.Guild.Id}");
			_ = Program.SendMessage(context, $"{victim.Mention} has been muted{(sentDm ? '.' : " (Failed to DM).")} Reason: ```\n{muteReason}\n```", null, new UserMention(victim.Id));
		}
	}
}
