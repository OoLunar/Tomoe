using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;

namespace Tomoe.Commands.Moderation
{
	public class Unmute : BaseCommandModule
	{
		[Command("unmute"), Description("Unmutes an individual."), Aliases("unsilence")]
		public async Task Individual(CommandContext context, DiscordUser victim, [RemainingText] string unmuteReason = Program.MissingReason)
		{
			if (victim == context.Client.CurrentUser)
			{
				_ = Program.SendMessage(context, Program.SelfAction);
				return;
			}

			ulong? muteRoleId = Program.Database.Guild.MuteRole(context.Guild.Id);
			if (!muteRoleId.HasValue)
			{
				_ = Program.SendMessage(context, Program.MissingRole);
				return;
			}

			DiscordRole muteRole = context.Guild.GetRole(muteRoleId.Value);
			if (muteRole == null)
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
					else if (!guildVictim.IsBot) _ = await guildVictim.SendMessageAsync($"You've been unmuted by **{context.User.Mention}** from **{context.Guild.Name}**. Reason: ```\n{unmuteReason.Filter()}\n```");
				}
				catch (UnauthorizedException)
				{
					sentDm = false;
				}
				await guildVictim.RevokeRoleAsync(muteRole, unmuteReason);
			}
			catch (NotFoundException)
			{
				sentDm = false;
			}
			Program.Database.User.IsMuted(context.Guild.Id, victim.Id, false);
			_ = Program.SendMessage(context, $"{victim.Mention} has been unmuted{(sentDm ? '.' : " (Failed to DM).")} Reason: ```\n{unmuteReason.Filter()}\n```", ExtensionMethods.FilteringAction.CodeBlocksIgnore, new List<IMention>() { new UserMention(victim.Id) });
		}

		public static async Task ByAssignment(CommandContext context, DiscordUser victim)
		{
			Program.Database.User.IsMuted(context.Guild.Id, victim.Id, false);
			ulong? muteRoleId = Program.Database.Guild.MuteRole(context.Guild.Id);
			if (!muteRoleId.HasValue) return;
			DiscordRole muteRole = context.Guild.GetRole(muteRoleId.Value);
			if (muteRole == null) return;

			try
			{
				DiscordMember guildVictim = await context.Guild.GetMemberAsync(victim.Id);
				try
				{
					if (!guildVictim.IsBot) _ = await guildVictim.SendMessageAsync($"You've been unmuted by **{context.User.Mention}** from **{context.Guild.Name}**. Reason: ```\nTempmute complete!\n```");
				}
				catch (UnauthorizedException) { }
				await guildVictim.RevokeRoleAsync(muteRole, "Tempmute complete!");
			}
			catch (NotFoundException) { }
		}
	}
}
