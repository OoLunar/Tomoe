using System;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Tomoe.Commands.Moderation.Attributes;
using Tomoe.Db;

namespace Tomoe.Commands.Moderation
{
	public class Unban : BaseCommandModule
	{
		public Database Database { private get; set; }
		[Command("unban"), RequireGuild, RequireUserPermissions(Permissions.BanMembers), RequireBotPermissions(Permissions.BanMembers), Aliases("fuck_come_back", "fuck_comeback", "fuckcome_back"), Description("Unbans the victim from the guild, allowing them to rejoin."), Punishment(false)]
		public async Task ByUser(CommandContext context, [Description("Who to unban.")] DiscordUser victim, [Description("Why is the vicitm being unbanned."), RemainingText] string unbanReason = Constants.MissingReason)
		{
			bool sentDm = await ByProgram(context.Guild, victim, context.Message.JumpLink, unbanReason);
			_ = await Program.SendMessage(context, $"{victim.Mention} has been unbanned{(sentDm ? '.' : " (Failed to dm).")}");
		}

		public static async Task<bool> ByProgram(DiscordGuild discordGuild, DiscordUser victim, Uri jumplink, [RemainingText] string unbanReason = Constants.MissingReason)
		{
			await discordGuild.UnbanMemberAsync(victim.Id, unbanReason);
			bool sentDm = await (await victim.Id.GetMember(discordGuild)).TryDmMember($"You've been unbanned from {Formatter.Bold(discordGuild.Name)}. Reason: {Formatter.BlockCode(Formatter.Strip(unbanReason))}Context: {jumplink}");
			await ModLogs.Record(discordGuild.Id, "Unban", $"{victim.Mention} has been unbanned{(sentDm ? '.' : " (Failed to dm).")} Reason: {unbanReason}");
			return sentDm;
		}
	}
}
