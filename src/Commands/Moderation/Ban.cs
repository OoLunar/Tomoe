using System;
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
	public class Ban : BaseCommandModule
	{
		[Command("ban"), RequireGuild, RequireUserPermissions(Permissions.BanMembers), RequireBotPermissions(Permissions.BanMembers), Aliases("fuck_off", "fuckoff"), Description("Permanently bans the victim from the guild, sending them off with a dm."), Punishment(false)]
		public async Task ByUser(CommandContext context, [Description("Who to ban.")] DiscordUser victim, [Description("Why is the victim being banned?"), RemainingText] string banReason = Constants.MissingReason)
		{
			try
			{
				if ((await context.Guild.GetBansAsync()).Any(guildUser => guildUser.User.Id == victim.Id))
				{
					_ = await Program.SendMessage(context, Formatter.Bold($"[Error]: {victim.Mention} is already banned!"));
					return;
				}
				_ = await Program.SendMessage(context, $"{victim.Mention} has been banned{(await ByProgram(context.Guild, victim, context.User, context.Message.JumpLink, banReason) ? '.' : " (Failed to dm).")}");
			}
			catch (UnauthorizedException)
			{
				_ = await Program.SendMessage(context, Formatter.Bold($"[Error]: I cannot ban {victim.Mention} due to permissions!"));
			}
		}

		public static async Task<bool> ByProgram(DiscordGuild discordGuild, DiscordUser victim, DiscordUser issuer, Uri jumplink, [RemainingText] string banReason = Constants.MissingReason)
		{
			bool sentDm = await (await victim.Id.GetMember(discordGuild)).TryDmMember($"You've been banned from {Formatter.Bold(discordGuild.Name)}. Reason: {Formatter.BlockCode(Formatter.Strip(banReason))}Context: {jumplink}");
			await discordGuild.BanMemberAsync(victim.Id, 0, banReason);
			await ModLogs.Record(discordGuild.Id, "Ban", $"{victim.Mention} has been banned{(sentDm ? '.' : " (Failed to dm).")} by {issuer.Mention} Reason: {banReason}");
			return sentDm;
		}
	}
}
