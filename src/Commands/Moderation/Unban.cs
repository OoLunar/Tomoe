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
	public class Unban : BaseCommandModule
	{
		public Database Database { private get; set; }
		[Command("unban"), RequireGuild, RequireUserPermissions(Permissions.BanMembers), RequireBotPermissions(Permissions.BanMembers), Aliases("fuck_come_back", "fuck_comeback", "fuckcome_back"), Description("Unbans the victim from the guild, allowing them to rejoin."), Punishment(false)]
		public async Task ByUser(CommandContext context, DiscordUser victim, [RemainingText] string unbanReason = Constants.MissingReason)
		{
			bool sentDm = await ByProgram(context.Guild, victim, context.Message.JumpLink, unbanReason);
			_ = await Program.SendMessage(context, $"{victim.Mention} has been unbanned{(sentDm ? '.' : " (Failed to dm).")}");
		}

		public static async Task<bool> ByProgram(DiscordGuild discordGuild, DiscordUser victim, Uri jumplink, [RemainingText] string unbanReason = Constants.MissingReason)
		{
			await discordGuild.UnbanMemberAsync(victim.Id, unbanReason);

			using IServiceScope scope = Program.ServiceProvider.CreateScope();
			Database database = scope.ServiceProvider.GetService<Database>();
			DiscordMember guildVictim = await victim.Id.GetMember(discordGuild);

			// If the user is in the guild, and if the user isn't a bot, attempt to dm them to make them aware of their punishment
			bool sentDm = false;
			if (guildVictim != null && !guildVictim.IsBot)
			{
				try
				{
					_ = await guildVictim.SendMessageAsync($"You've been unbanned from {Formatter.Bold(discordGuild.Name)}. Reason: {Formatter.BlockCode(Formatter.Strip(unbanReason))}Context: {jumplink}");
					sentDm = true;
				}
				catch (Exception) { }
			}

			return sentDm;
		}
	}
}
