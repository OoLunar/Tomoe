using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using Tomoe.Commands.Moderation.Attributes;

namespace Tomoe.Commands.Moderation
{
	public class Unban : BaseCommandModule
	{
		[Command("unban"), Description("Unbans people from the guild."), RequireUserPermissions(Permissions.BanMembers), RequireBotPermissions(Permissions.BanMembers), Punishment]
		public async Task User(CommandContext context, [Description("The person to be unbanned.")] DiscordUser victim, [Description("(Optional) The reason why the person is being unbanned."), RemainingText] string unbanReason = Constants.MissingReason)
		{
			IReadOnlyList<DiscordBan> guildBans = await context.Guild.GetBansAsync();
			if (guildBans.Count == 0 || guildBans.All(discordBan => discordBan.User != victim))
			{
				_ = await Program.SendMessage(context, $"{victim.Mention} isn't banned!");
				return;
			}
			await context.Guild.UnbanMemberAsync(victim, unbanReason ?? Constants.MissingReason);

			bool sentDm = true;
			DiscordMember guildVictim = context.Guild.Members[victim.Id];
			if (guildVictim != null && !guildVictim.IsBot) try
				{
					_ = await guildVictim.SendMessageAsync($"You've been unbanned by {Formatter.Bold(context.User.Mention)} from {Formatter.Bold(context.Guild.Name)}. Reason: {Formatter.BlockCode(Formatter.Strip(unbanReason))}");
				}
				catch (UnauthorizedException) { }
			_ = await Program.SendMessage(context, $"{victim.Mention} has been unbanned{(sentDm ? '.' : " (Failed to DM).")} Reason: {Formatter.BlockCode(Formatter.Strip(unbanReason))}", null, new UserMention(victim.Id));
		}


		public static async Task ByAssignment(CommandContext context, DiscordUser victim)
		{
			IReadOnlyList<DiscordBan> guildBans = await context.Guild.GetBansAsync();
			if (guildBans.Count == 0 || guildBans.All(discordBan => discordBan.User != victim)) return;

			try
			{
				await context.Guild.UnbanMemberAsync(victim.Id, "Tempban complete!");
			}
			catch (NotFoundException) { }
			catch (UnauthorizedException) { }
		}
	}
}
