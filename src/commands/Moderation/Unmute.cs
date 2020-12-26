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
		[Command("unmute"), Description("Unmutes an individual.")]
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
				_ = Program.SendMessage(context, Program.MissingMuteRole);
				return;
			}

			DiscordRole muteRole = context.Guild.GetRole(muteRoleId.Value);
			if (muteRole == null)
			{
				_ = Program.SendMessage(context, Program.MissingMuteRole);
				return;
			}

			bool sentDm = true;

			try
			{
				DiscordMember guildVictim = await context.Guild.GetMemberAsync(victim.Id);
				try
				{
					if (guildVictim.Hierarchy > context.Guild.CurrentMember.Hierarchy)
					{
						_ = Program.SendMessage(context, Program.Hierarchy);
						return;
					}
					else if (guildVictim.IsBot)
					{
						_ = await guildVictim.SendMessageAsync($"You've been unmuted by **{context.User.Mention}** from **{context.Guild.Name}**. Reason: ```\n{unmuteReason.Filter()}\n```");
					}
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
	}
}
