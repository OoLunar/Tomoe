using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;

using Tomoe.Commands.Moderation.Attributes;

namespace Tomoe.Commands.Moderation
{
	public class UnmemeBan : BaseCommandModule
	{
		[Command("promeme"), Description("Unmutes an individual."), Aliases("pro_meme", "unmemeban", "unmeme_ban", "un_memeban", "un_meme_ban", "tempnomeme", "temp_no_meme", "temp_nomeme", "tempno_meme"), Punishment]
		public async Task User(CommandContext context, DiscordUser victim, [RemainingText] string promemeReason = Constants.MissingReason)
		{
			DiscordRole antimemeRole = Program.Database.Guild.AntimemeRole(context.Guild.Id).GetRole(context.Guild);
			if (antimemeRole == null)
			{
				_ = Program.SendMessage(context, Constants.MissingRole);
				return;
			}

			bool sentDm = false;
			DiscordMember guildVictim = victim.GetMember(context.Guild);
			if (guildVictim != null)
			{
				try
				{
					if (!guildVictim.IsBot) _ = await guildVictim.SendMessageAsync($"You've been promemed by {Formatter.Bold(context.User.Mention)} from {Formatter.Bold(context.Guild.Name)}. Reason: {Formatter.BlockCode(Formatter.Strip(promemeReason))}");
					sentDm = true;
				}
				catch (UnauthorizedException) { }
				await guildVictim.RevokeRoleAsync(antimemeRole, promemeReason);
			}
			Program.Database.User.IsAntiMemed(context.Guild.Id, victim.Id, false);
			_ = Program.SendMessage(context, $"{victim.Mention} is no longer antimemed{(sentDm ? '.' : " (Failed to DM).")} Reason: {Formatter.BlockCode(Formatter.Strip(promemeReason))}", null, new UserMention(victim.Id));
		}

		public static async Task ByAssignment(CommandContext context, DiscordUser victim)
		{
			Program.Database.User.IsAntiMemed(context.Guild.Id, victim.Id, false);

			DiscordRole muteRole = Program.Database.Guild.MuteRole(context.Guild.Id).GetRole(context.Guild);
			if (muteRole == null) return;

			DiscordMember guildVictim = victim.GetMember(context.Guild);
			if (guildVictim != null)
			{
				try
				{
					if (!guildVictim.IsBot) _ = await guildVictim.SendMessageAsync($"You've been unmuted by {Formatter.Bold(context.User.Mention)} from {Formatter.Bold(context.Guild.Name)}. Reason: {Formatter.BlockCode("Tempmute complete!")}");
				}
				catch (UnauthorizedException) { }
				await guildVictim.RevokeRoleAsync(muteRole, "Tempmute complete!");
			}
		}
	}
}
