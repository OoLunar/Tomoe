using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tomoe.Commands.Moderation.Attributes;
using Tomoe.Db;

namespace Tomoe.Commands.Moderation
{
	public class Unmute : BaseCommandModule
	{
		public Database Database { private get; set; }

		[Command("unmute"), Description("Unmutes an individual."), Aliases("unsilence"), Punishment]
		public async Task User(CommandContext context, DiscordUser victim, [RemainingText] string unmuteReason = Constants.MissingReason)
		{
			DiscordRole muteRole = null;
			Guild guild = await Database.Guilds.FirstOrDefaultAsync(guild => guild.Id == context.Guild.Id);
			if (guild != null) muteRole = guild.MuteRole.GetRole(context.Guild);
			if (muteRole == null)
			{
				_ = await Program.SendMessage(context, Constants.MissingRole);
				return;
			}

			bool sentDm = false;
			DiscordMember guildVictim = context.Guild.Members[victim.Id];
			if (guildVictim != null)
			{
				if (!guildVictim.IsBot)
				{
					try
					{
						_ = await guildVictim.SendMessageAsync($"You've been unmuted by {Formatter.Bold(context.User.Mention)} from {Formatter.Bold(context.Guild.Name)}. Reason: {Formatter.BlockCode(Formatter.Strip(unmuteReason))}");
						sentDm = true;
					}
					catch (UnauthorizedException) { }
				}
				await guildVictim.RevokeRoleAsync(muteRole, unmuteReason);
			}

			GuildUser user = guild.Users.FirstOrDefault(user => user.Id == victim.Id);
			if (user != null) user.IsMuted = false;

			_ = await Program.SendMessage(context, $"{victim.Mention} has been unmuted{(sentDm ? '.' : " (Failed to DM).")} Reason: {Formatter.BlockCode(Formatter.Strip(unmuteReason))}", null, new UserMention(victim.Id));
		}

		public static async Task ByAssignment(CommandContext context, DiscordUser victim)
		{
			using IServiceScope scope = context.Services.CreateScope();
			Database database = scope.ServiceProvider.GetService<Database>();
			Guild guild = await database.Guilds.FirstOrDefaultAsync(guild => guild.Id == context.Guild.Id);
			if (guild == null) return;
			GuildUser user = guild.Users.FirstOrDefault(user => user.Id == victim.Id);
			if (user != null) user.IsMuted = false;

			DiscordRole muteRole = guild.MuteRole.GetRole(context.Guild);
			if (muteRole == null) return;

			DiscordMember guildVictim = context.Guild.Members[victim.Id];
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
