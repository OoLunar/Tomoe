using System.Linq;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;

using Tomoe.Commands.Moderation.Attributes;
using Tomoe.Db;

namespace Tomoe.Commands.Moderation
{
	public class Mute : BaseCommandModule
	{
		[Command("mute"), Description("Mutes a person permanently."), RequireBotPermissions(Permissions.ManageRoles), RequireUserPermissions(Permissions.ManageMessages), Aliases("silence"), Punishment]
		public async Task User(CommandContext context, DiscordUser victim, [RemainingText] string muteReason = Constants.MissingReason)
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
					if (!guildVictim.IsBot) _ = await guildVictim.SendMessageAsync($"You've been muted by {Formatter.Bold(context.User.Mention)} from {Formatter.Bold(context.Guild.Name)}. Reason: {Formatter.BlockCode(Formatter.Strip(muteReason))}");
					sentDm = true;
				}
				catch (UnauthorizedException) { }
				await guildVictim.GrantRoleAsync(muteRole, muteReason);
			}

			GuildUser user = guild.Users.First(user => user.Id == victim.Id);
			user.IsMuted = true;

			_ = await Program.SendMessage(context, $"{victim.Mention} has been muted{(sentDm ? '.' : " (Failed to DM).")} Reason: {Formatter.BlockCode(Formatter.Strip(muteReason))}", null, new UserMention(victim.Id));
		}
	}
}
