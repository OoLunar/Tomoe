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
	public class Temban : BaseCommandModule
	{
		[Command("tempban")]
		[Description("Temporarily bans someone from the server.")]
		[RequireGuild]
		[RequireBotPermissions(Permissions.BanMembers)]
		[RequireUserPermissions(Permissions.BanMembers)]
		public async Task BanUser(CommandContext context, DiscordUser victim, ExpandedTimeSpan banTime, int pruneDays = 7, [RemainingText] string banReason = Program.MissingReason)
		{
			if (victim == context.Client.CurrentUser)
			{
				_ = Program.SendMessage(context, Program.SelfAction);
				return;
			}

			if (pruneDays < 7)
			{
				pruneDays = 7;
			}
			bool sentDm = true;
			try
			{
				DiscordMember guildVictim = await context.Guild.GetMemberAsync(victim.Id);
				if (guildVictim.Hierarchy > context.Guild.CurrentMember.Hierarchy)
				{
					_ = Program.SendMessage(context, Program.Hierarchy);
					return;
				}
				else if (!guildVictim.IsBot)
				{
					_ = await guildVictim.SendMessageAsync($"You've been tempbanned by **{context.User.Mention}** from **{context.Guild.Name}** for **{banTime.ToString()}. Reason: ```\n{banReason.Filter() ?? Program.MissingReason}\n```");
				}
			}
			catch (NotFoundException)
			{
				sentDm = false;
			}
			catch (BadRequestException)
			{
				sentDm = false;
			}
			catch (UnauthorizedException)
			{
				sentDm = false;
			}
			await context.Guild.BanMemberAsync(victim.Id, pruneDays, banReason ?? Program.MissingReason);
			Program.Database.Assignments.Create(AssignmentType.TempBan, context.Guild.Id, context.Channel.Id, context.Message.Id, victim.Id, DateTime.Now + banTime.TimeSpan, DateTime.Now, $"{victim.Id} tempbanned in {context.Guild.Id}");
			_ = Program.SendMessage(context, $"{victim.Mention} has been temporarily banned{(sentDm ? '.' : " (Failed to DM).")} Reason: ```\n{banReason.Filter(ExtensionMethods.FilteringAction.CodeBlocksZeroWidthSpace) ?? Program.MissingReason}```\n", ExtensionMethods.FilteringAction.CodeBlocksIgnore, new List<IMention>() { new UserMention(victim.Id) });
		}
	}
}
