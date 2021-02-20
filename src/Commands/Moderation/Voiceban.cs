using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;

using Tomoe.Commands.Moderation.Attributes;

namespace Tomoe.Commands.Moderation
{
	public class VoiceBan : BaseCommandModule
	{
		[Command("voiceban"), Description("Prevents the victim from joining voice channels. Good to use when someone is switching between channels or spamming unmutting and muting."), RequireBotPermissions(Permissions.ManageRoles), RequireUserPermissions(Permissions.MuteMembers), Aliases("voice_ban", "vb"), Punishment]
		public async Task User(CommandContext context, DiscordUser victim, [RemainingText] string voiceBanReason = Constants.MissingReason)
		{
			DiscordRole voiceBanRole = Program.Database.Guild.VoiceBanRole(context.Guild.Id).GetRole(context.Guild);
			if (voiceBanRole == null)
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
					if (!guildVictim.IsBot) _ = await guildVictim.SendMessageAsync($"You've been voicebanned by {Formatter.Bold(context.User.Mention)} from {Formatter.Bold(context.Guild.Name)}. This means you cannot join voice channels. Reason: {Formatter.BlockCode(Formatter.Strip(voiceBanReason))}");
					sentDm = true;
				}
				catch (UnauthorizedException) { }
				await guildVictim.GrantRoleAsync(voiceBanRole, voiceBanReason);
			}

			Program.Database.User.IsVoiceBanned(context.Guild.Id, victim.Id, true);
			_ = await Program.SendMessage(context, $"{victim.Mention} has been voicebanned{(sentDm ? '.' : " (Failed to DM).")} Reason: {Formatter.BlockCode(Formatter.Strip(voiceBanReason))}", null, new UserMention(victim.Id));
		}
	}
}
