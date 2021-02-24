using System.Linq;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using Microsoft.EntityFrameworkCore;
using Tomoe.Commands.Moderation.Attributes;
using Tomoe.Db;

namespace Tomoe.Commands.Moderation
{
	public class Promeme : BaseCommandModule
	{
		public Database Database { private get; set; }

		[Command("promeme"), Description("Unmutes an individual."), Aliases("pro_meme", "unmemeban", "unmeme_ban", "un_memeban", "un_meme_ban", "tempnomeme", "temp_no_meme", "temp_nomeme", "tempno_meme"), Punishment]
		public async Task User(CommandContext context, DiscordUser victim, [RemainingText] string promemeReason = Constants.MissingReason)
		{
			DiscordRole antimemeRole = null;
			Guild guild = await Database.Guilds.FirstOrDefaultAsync(guild => guild.Id == context.Guild.Id);
			if (guild != null) antimemeRole = guild.AntimemeRole.GetRole(context.Guild);
			if (antimemeRole == null)
			{
				_ = await Program.SendMessage(context, Constants.MissingRole);
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

			GuildUser user = guild.Users.FirstOrDefault(user => user.Id == victim.Id);
			if (user == null) user.IsAntimemed = false;

			_ = await Program.SendMessage(context, $"{victim.Mention} is no longer antimemed{(sentDm ? '.' : " (Failed to DM).")} Reason: {Formatter.BlockCode(Formatter.Strip(promemeReason))}", null, new UserMention(victim.Id));
		}

		public static async Task ByAssignment(CommandContext context, DiscordUser victim)
		{
			Guild guild = await Program.Database.Guilds.FirstOrDefaultAsync(guild => guild.Id == context.Guild.Id);
			GuildUser user = guild.Users.FirstOrDefault(user => user.Id == victim.Id);
			if (user == null) user.IsAntimemed = false;

			DiscordRole antimemeRole = guild.AntimemeRole.GetRole(context.Guild);
			if (antimemeRole == null) return;

			DiscordMember guildVictim = victim.GetMember(context.Guild);
			if (guildVictim != null)
			{
				try
				{
					if (!guildVictim.IsBot) _ = await guildVictim.SendMessageAsync($"You've been promemed by {Formatter.Bold(context.User.Mention)} from {Formatter.Bold(context.Guild.Name)}. Reason: {Formatter.BlockCode("TempAntimeme complete!")}");
				}
				catch (UnauthorizedException) { }
				await guildVictim.RevokeRoleAsync(antimemeRole, "TempAntimeme complete!");
			}
		}
	}
}
