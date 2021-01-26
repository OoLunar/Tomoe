using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;

using Tomoe.Commands.Moderation.Attributes;
using Tomoe.Utils;

namespace Tomoe.Commands.Moderation
{
	public class Unban : BaseCommandModule
	{
		private static readonly Logger _logger = new("Commands.Moderation.Unban");

		[Command("unban"), Description("Unbans people from the guild."), RequireUserPermissions(Permissions.BanMembers), RequireBotPermissions(Permissions.BanMembers), Punishment]
		public async Task User(CommandContext context, [Description("The person to be unbanned.")] DiscordUser victim, [Description("(Optional) The reason why the person is being unbanned."), RemainingText] string unbanReason = Constants.MissingReason)
		{
			IReadOnlyList<DiscordBan> guildBans = await context.Guild.GetBansAsync();
			if (guildBans.Count == 0 || guildBans.All(discordBan => discordBan.User != victim))
			{
				_ = Program.SendMessage(context, $"{victim.Mention} isn't banned!");
				return;
			}
			await context.Guild.UnbanMemberAsync(victim, unbanReason ?? Constants.MissingReason);

			bool sentDm = true;
			DiscordMember guildVictim = victim.GetMember(context.Guild);
			if (guildVictim != null && !guildVictim.IsBot) try
				{
					_ = await guildVictim.SendMessageAsync($"You've been unbanned by {Formatter.Bold(context.User.Mention)} from {Formatter.Bold(context.Guild.Name)}. Reason: {Formatter.BlockCode(Formatter.Strip(unbanReason))}");
				}
				catch (UnauthorizedException) { }
			_ = Program.SendMessage(context, $"{victim.Mention} has been unbanned{(sentDm ? '.' : " (Failed to DM).")} Reason: {Formatter.BlockCode(Formatter.Strip(unbanReason))}", null, new UserMention(victim.Id));
		}


		public static async Task ByAssignment(CommandContext context, DiscordUser victim)
		{
			IReadOnlyList<DiscordBan> guildBans = await context.Guild.GetBansAsync();
			if (guildBans.Count == 0 || guildBans.All(discordBan => discordBan.User != victim))
			{
				_logger.Debug($"No bans found, skipping unbanning of {victim.Id}");
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
