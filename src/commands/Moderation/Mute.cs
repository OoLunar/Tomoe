using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;

namespace Tomoe.Commands.Moderation
{
	public class Mute : BaseCommandModule
	{
		[Command("mute"), Description("Mutes a person permanently."), RequireBotPermissions(Permissions.ManageRoles), RequireUserPermissions(Permissions.ManageMessages), Aliases("silence")]
		public async Task Permanently(CommandContext context, DiscordUser victim, [RemainingText] string muteReason = Program.MissingReason)
		{
			if (victim == context.Client.CurrentUser)
			{
				_ = Program.SendMessage(context, Program.SelfAction);
				return;
			}

			ulong? muteRoleId = Program.Database.Guild.MuteRole(context.Guild.Id);
			if (!muteRoleId.HasValue)
			{
				_ = Program.SendMessage(context, Program.MissingRole);
				return;
			}

			DiscordRole muteRole = context.Guild.GetRole(muteRoleId.Value);
			if (muteRole == null)
			{
				_ = Program.SendMessage(context, Program.MissingRole);
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
					else if (!guildVictim.IsBot)
					{
						_ = await guildVictim.SendMessageAsync($"You've been muted by **{context.User.Mention}** from **{context.Guild.Name}**. Reason: ```\n{muteReason.Filter()}\n```");
					}
				}
				catch (UnauthorizedException)
				{
					sentDm = false;
				}
				await guildVictim.GrantRoleAsync(muteRole, muteReason);
			}
			catch (NotFoundException)
			{
				sentDm = false;
			}

			Program.Database.User.IsMuted(context.Guild.Id, victim.Id, true);
			_ = Program.SendMessage(context, $"{victim.Mention} has been muted{(sentDm ? '.' : " (Failed to DM).")} Reason: ```\n{muteReason.Filter()}\n```", ExtensionMethods.FilteringAction.CodeBlocksIgnore, new List<IMention>() { new UserMention(victim.Id) });
		}
	}
}
