using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;

using Tomoe.Commands.Moderation.Attributes;

namespace Tomoe.Commands.Moderation
{
	public class Mute : BaseCommandModule
	{
		[Command("mute"), Description("Mutes a person permanently."), RequireBotPermissions(Permissions.ManageRoles), RequireUserPermissions(Permissions.ManageMessages), Aliases("silence"), Punishment(true)]
		public async Task User(CommandContext context, DiscordUser victim, [Description("(Optional) Should prompt to confirm with the self mute")] bool confirmed = false, [RemainingText] string muteReason = Program.MissingReason)
		{
			DiscordRole muteRole = Program.Database.Guild.MuteRole(context.Guild.Id).GetRole(context.Guild);
			if (muteRole == null)
			{
				_ = Program.SendMessage(context, Program.MissingRole);
				return;
			}

			bool sentDm = false;
			DiscordMember guildVictim = victim.GetMember(context.Guild);
			if (guildVictim != null)
			{
				try
				{
					if (!guildVictim.IsBot) await guildVictim.SendMessageAsync($"You've been muted by **{context.User.Mention}** from **{context.Guild.Name}**. Reason: {Formatter.BlockCode(Formatter.Strip(muteReason))}");
					sentDm = true;
				}
				catch (UnauthorizedException) { }
				await guildVictim.GrantRoleAsync(muteRole, muteReason);
			}
			Program.Database.User.IsMuted(context.Guild.Id, victim.Id, true);
			_ = Program.SendMessage(context, $"{victim.Mention} has been muted{(sentDm ? '.' : " (Failed to DM).")} Reason: {Formatter.BlockCode(Formatter.Strip(muteReason))}", null, new UserMention(victim.Id));
		}
	}
}
