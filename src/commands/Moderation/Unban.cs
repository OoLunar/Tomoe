using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using Tomoe.Utils;

namespace Tomoe.Commands.Moderation
{
	public class Unban : BaseCommandModule
	{
		private const string _COMMAND_NAME = "unban";
		private const string _COMMAND_DESC = "Unbans people from the guild.";
		private const string _VICTIM_DESC = "The person to be unbanned.";
		private const string _UNBAN_REASON = "(Optional) The reason why the person is being unbanned.";
		private static readonly Logger Logger = new("Commands.Moderation.Unban");

		[Command(_COMMAND_NAME), Description(_COMMAND_DESC), RequireUserPermissions(Permissions.BanMembers), RequireBotPermissions(Permissions.BanMembers)]
		public async Task UnbanUser(CommandContext context, [Description(_VICTIM_DESC)] DiscordUser victim, [Description(_UNBAN_REASON), RemainingText] string unbanReason = Program.MissingReason)
		{
			IReadOnlyList<DiscordBan> guildBans = await context.Guild.GetBansAsync();
			if (guildBans.Count == 0 || guildBans.Any(discordBan => discordBan.User != victim))
			{
				_ = Program.SendMessage(context, $"{victim.Mention} isn't banned!");
				return;
			}

			bool sentDm = true;

			try
			{
				await context.Guild.UnbanMemberAsync(victim, unbanReason ?? Program.MissingReason);
				DiscordMember guildVictim = await context.Guild.GetMemberAsync(victim.Id);
				_ = await guildVictim.SendMessageAsync($"You've been unbanned by **{context.User.Mention}** from **{context.Guild.Name}**. Reason: ```\n{unbanReason.Filter() ?? Program.MissingReason}\n```");
			}
			catch (NotFoundException)
			{
				sentDm = false;
			}
			catch (UnauthorizedException)
			{
				sentDm = false;
			}
			_ = Program.SendMessage(context, $"{victim.Mention} has been unbanned{(sentDm ? '.' : " (Failed to DM).")} Reason: ```\n{unbanReason.Filter() ?? Program.MissingReason}\n```", ExtensionMethods.FilteringAction.CodeBlocksIgnore, new List<IMention>() { new UserMention(victim.Id) });
		}


		public static async Task ByAssignment(CommandContext context, DiscordUser victim)
		{
			IReadOnlyList<DiscordBan> guildBans = await context.Guild.GetBansAsync();
			if (guildBans.Count == 0 || !guildBans.Any(discordBan => discordBan.User.Id == victim.Id))
			{
				await Logger.Debug($"No bans found, skipping unbanning of {victim.Id}");
				return;
			}

			try
			{
				await context.Guild.UnbanMemberAsync(victim.Id, "Tempban complete!");
			}
			catch (NotFoundException) { }
			catch (UnauthorizedException) { }
		}
	}
}
