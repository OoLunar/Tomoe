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
	public class Temban : BaseCommandModule
	{
		[Command("tempban"), Description("Temporarily bans someone from the server."), RequireGuild, RequireBotPermissions(Permissions.BanMembers), RequireUserPermissions(Permissions.BanMembers), Aliases("temp_ban"), Punishment(true)]
		public async Task BanUser(CommandContext context, DiscordUser victim, ExpandedTimeSpan banTime, int pruneDays = 7, [RemainingText] string banReason = Program.MissingReason)
		{
			if (pruneDays < 7) pruneDays = 7;
			bool sentDm = false;
			DiscordMember guildVictim = await context.Guild.GetMemberAsync(victim.Id);
			if (guildVictim != null && !guildVictim.IsBot) try
				{
					await guildVictim.SendMessageAsync($"You've been tempbanned by **{context.User.Mention}** from **{context.Guild.Name}** for **{banTime.ToString()}. Reason: ```\n{banReason ?? Program.MissingReason}\n```");
				}
				catch (UnauthorizedException) { }

			await context.Guild.BanMemberAsync(victim.Id, pruneDays, banReason ?? Program.MissingReason);
			Program.Database.Assignments.Create(AssignmentType.TempBan, context.Guild.Id, context.Channel.Id, context.Message.Id, victim.Id, DateTime.Now + banTime.TimeSpan, DateTime.Now, $"{victim.Id} tempbanned in {context.Guild.Id}");
			_ = Program.SendMessage(context, $"{victim.Mention} has been temporarily banned{(sentDm ? '.' : " (Failed to DM).")} Reason: ```\n{banReason ?? Program.MissingReason}```\n", null, new UserMention(victim.Id));
		}
	}
}
