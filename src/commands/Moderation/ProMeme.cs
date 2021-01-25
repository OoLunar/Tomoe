using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using Tomoe.Commands.Moderation.Attributes;

namespace Tomoe.Commands.Moderation
{
	public class UnmemeBan : BaseCommandModule
	{
		[Command("promeme"), Description("Unmutes an individual."), Aliases("pro_meme", "unmemeban", "unmeme_ban", "un_memeban", "un_meme_ban", "tempnomeme", "temp_no_meme", "temp_nomeme", "tempno_meme"), Punishment(true)]
		public async Task Individual(CommandContext context, DiscordUser victim, [RemainingText] string unmemeBanReason = Program.MissingReason)
		{
			DiscordRole antimemeRole = Program.Database.Guild.AntiMemeRole(context.Guild.Id).GetRole(context.Guild);
			if (antimemeRole == null)
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
					if (!guildVictim.IsBot) await guildVictim.SendMessageAsync($"You've been unmeme banned by **{context.User.Mention}** from **{context.Guild.Name}**. Reason: ```\n{unmemeBanReason}\n```");
					sentDm = true;
				}
				catch (UnauthorizedException) { }
				await guildVictim.RevokeRoleAsync(antimemeRole, unmemeBanReason);
			}
			Program.Database.User.IsAntiMemed(context.Guild.Id, victim.Id, false);
			_ = Program.SendMessage(context, $"{victim.Mention} is no longer antimemed{(sentDm ? '.' : " (Failed to DM).")} Reason: ```\n{unmemeBanReason}\n```", null, new UserMention(victim.Id));
		}

		public static async Task ByAssignment(CommandContext context, DiscordUser victim)
		{
			Program.Database.User.IsAntiMemed(context.Guild.Id, victim.Id, false);

			DiscordRole muteRole = Program.Database.Guild.MuteRole(context.Guild.Id).GetRole(context.Guild);
			if (muteRole == null) return;

			DiscordMember guildVictim = await context.Guild.GetMemberAsync(victim.Id);
			if (guildVictim != null)
			{
				try
				{
					if (!guildVictim.IsBot) _ = await guildVictim.SendMessageAsync($"You've been unmuted by **{context.User.Mention}** from **{context.Guild.Name}**. Reason: ```\nTempmute complete!\n```");
				}
				catch (UnauthorizedException) { }
				await guildVictim.RevokeRoleAsync(muteRole, "Tempmute complete!");
			}
		}
	}
}
