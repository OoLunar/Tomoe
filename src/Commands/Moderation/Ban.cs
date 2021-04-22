using System;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Microsoft.Extensions.DependencyInjection;
using Tomoe.Commands.Moderation.Attributes;
using Tomoe.Db;

namespace Tomoe.Commands.Moderation
{
	public class Ban : BaseCommandModule
	{
		[Command("ban"), RequireGuild, RequireUserPermissions(Permissions.BanMembers), RequireBotPermissions(Permissions.BanMembers), Aliases("fuck_off", "fuckoff"), Description("Permanently bans the victim from the guild, sending them off with a dm."), Punishment(false)]
		public async Task ByUser(CommandContext context, DiscordUser victim, [RemainingText] string banReason = Constants.MissingReason)
		{
			bool sentDm = await ByProgram(context.Guild, victim, context.User, context.Message.JumpLink, banReason);
			_ = await Program.SendMessage(context, $"{victim.Mention} has been banned{(sentDm ? '.' : " (Failed to dm).")}");
		}

		public static async Task<bool> ByProgram(DiscordGuild discordGuild, DiscordUser victim, DiscordUser issuer, Uri jumplink, [RemainingText] string banReason = Constants.MissingReason)
		{
			using IServiceScope scope = Program.ServiceProvider.CreateScope();
			Database database = scope.ServiceProvider.GetService<Database>();
			DiscordMember guildVictim = await victim.Id.GetMember(discordGuild);

			// If the user is in the guild, and if the user isn't a bot, attempt to dm them to make them aware of their punishment
			bool sentDm = false;
			if (guildVictim != null && !guildVictim.IsBot)
			{
				try
				{
					_ = await guildVictim.SendMessageAsync($"You've been banned from {Formatter.Bold(discordGuild.Name)}. Reason: {Formatter.BlockCode(Formatter.Strip(banReason))}Context: {jumplink}");
					sentDm = true;
				}
				catch (Exception) { }
			}

			await discordGuild.BanMemberAsync(victim.Id, 0, banReason);
			await ModLogs.Record(discordGuild.Id, "Ban", $"{victim.Mention} has been banned{(sentDm ? '.' : " (Failed to dm).")} by {issuer.Mention} Reason: {banReason}");
			return sentDm;
		}
	}
}
