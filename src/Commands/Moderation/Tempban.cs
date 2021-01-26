using System;
using System.Globalization;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;

using Tomoe.Commands.Moderation.Attributes;
using Tomoe.Database.Interfaces;

namespace Tomoe.Commands.Moderation
{
	public class Temban : BaseCommandModule
	{
		[Command("tempban"), Description("Temporarily bans someone from the server."), RequireGuild, RequireBotPermissions(Permissions.BanMembers), RequireUserPermissions(Permissions.BanMembers), Aliases("temp_ban"), Punishment(true)]
		public async Task User(CommandContext context, DiscordUser victim, ExpandedTimeSpan banTime, int pruneDays = 7, [RemainingText] string banReason = Program.MissingReason)
		{
			if (pruneDays < 7) pruneDays = 7;

			bool sentDm = false;
			DiscordMember guildVictim = victim.GetMember(context.Guild);
			if (guildVictim != null && !guildVictim.IsBot) try
				{
					_ = await guildVictim.SendMessageAsync($"You've been tempbanned by **{context.User.Mention}** from **{context.Guild.Name}** for **{banTime}**. Reason: {Formatter.BlockCode(Formatter.Strip(banReason))}");
				}
				catch (UnauthorizedException) { }
			await context.Guild.BanMemberAsync(victim.Id, pruneDays, banReason);
			Program.Database.Assignments.Create(AssignmentType.TempBan, context.Guild.Id, context.Channel.Id, context.Message.Id, victim.Id, DateTime.Now + banTime.TimeSpan, DateTime.Now, $"{victim.Id} tempbanned in {context.Guild.Id}");
			_ = Program.SendMessage(context, $"{victim.Mention} has been temporarily banned{(sentDm ? '.' : " (Failed to DM).")} Reason: {Formatter.BlockCode(Formatter.Strip(banReason))}", null, new UserMention(victim.Id));
		}
	}
}
