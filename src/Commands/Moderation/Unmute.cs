using System.Threading.Tasks;
using System.Linq;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;

using Tomoe.Commands.Moderation.Attributes;
using Tomoe.Db;

namespace Tomoe.Commands.Moderation
{
	public class Unmute : BaseCommandModule
	{
		[Command("unmute"), Description("Unmutes an individual."), Aliases("unsilence"), Punishment]
		public async Task User(CommandContext context, DiscordUser victim, [RemainingText] string unmuteReason = Constants.MissingReason)
		{
			Guild guild = Program.Database.Guilds.First(guild => guild.Id == context.Guild.Id);
			DiscordRole muteRole = guild.MuteRole.GetRole(context.Guild);
			if (muteRole == null)
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
					if (!guildVictim.IsBot) _ = await guildVictim.SendMessageAsync($"You've been unmuted by {Formatter.Bold(context.User.Mention)} from {Formatter.Bold(context.Guild.Name)}. Reason: {Formatter.BlockCode(Formatter.Strip(unmuteReason))}");
				}
				catch (UnauthorizedException) { }
				await guildVictim.RevokeRoleAsync(muteRole, unmuteReason);
			}

			GuildUser user = guild.Users.First(user => user.Id == victim.Id);
			user.IsMuted = false;

			_ = await Program.SendMessage(context, $"{victim.Mention} has been unmuted{(sentDm ? '.' : " (Failed to DM).")} Reason: {Formatter.BlockCode(Formatter.Strip(unmuteReason))}", null, new UserMention(victim.Id));
		}

		public static async Task ByAssignment(CommandContext context, DiscordUser victim)
		{
			Guild guild = Program.Database.Guilds.First(guild => guild.Id == context.Guild.Id);
			GuildUser user = guild.Users.First(user => user.Id == victim.Id);
			user.IsMuted = false;

			DiscordRole muteRole = guild.MuteRole.GetRole(context.Guild);
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
