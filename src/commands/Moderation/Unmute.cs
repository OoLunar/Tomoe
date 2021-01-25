using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using Tomoe.Commands.Moderation.Attributes;

namespace Tomoe.Commands.Moderation
{
	public class Unmute : BaseCommandModule
	{
		[Command("unmute"), Description("Unmutes an individual."), Aliases("unsilence")]
		public async Task Individual(CommandContext context, DiscordUser victim, [RemainingText] string unmuteReason = Program.MissingReason)
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
					if (!guildVictim.IsBot) _ = await guildVictim.SendMessageAsync($"You've been unmuted by **{context.User.Mention}** from **{context.Guild.Name}**. Reason: ```\n{unmuteReason}\n```");
				}
				catch (UnauthorizedException) { }
				await guildVictim.RevokeRoleAsync(muteRole, unmuteReason);
			}
			Program.Database.User.IsMuted(context.Guild.Id, victim.Id, false);
			_ = Program.SendMessage(context, $"{victim.Mention} has been unmuted{(sentDm ? '.' : " (Failed to DM).")} Reason: ```\n{unmuteReason}\n```", null, new UserMention(victim.Id));
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
