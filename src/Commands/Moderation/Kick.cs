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
	public class Kick : BaseCommandModule
	{
		[Command("kick"), RequireGuild, RequireUserPermissions(Permissions.BanMembers), RequireBotPermissions(Permissions.BanMembers), Aliases("boot", "yeet"), Description("Kicks the victim from the guild, sending them off with a dm."), Punishment(true)]
		public async Task ByUser(CommandContext context, DiscordMember victim, [RemainingText] string kickReason = Constants.MissingReason)
		{
			bool sentDm = await ByProgram(context.Guild, victim, context.User, context.Message.JumpLink, kickReason);
			_ = await Program.SendMessage(context, $"{victim.Mention} has been kicked{(sentDm ? '.' : " (Failed to dm).")}");
		}

		public static async Task<bool> ByProgram(DiscordGuild discordGuild, DiscordMember victim, DiscordUser issuer, Uri jumplink, [RemainingText] string kickReason = Constants.MissingReason)
		{
			using IServiceScope scope = Program.ServiceProvider.CreateScope();
			Database database = scope.ServiceProvider.GetService<Database>();
			// If the user is in the guild, and if the user isn't a bot, attempt to dm them to make them aware of their punishment
			bool sentDm = false;
			if (victim != null && !victim.IsBot)
			{
				try
				{
					_ = await victim.SendMessageAsync($"You've been kicked from {Formatter.Bold(discordGuild.Name)}. Reason: {Formatter.BlockCode(Formatter.Strip(kickReason))}Context: {jumplink}");
					sentDm = true;
				}
				catch (Exception) { }
			}

			await victim.RemoveAsync(kickReason);
			await ModLogs.Record(discordGuild.Id, "Kick", $"{victim.Mention} has been kicked{(sentDm ? '.' : " (Failed to dm).")} Reason: {kickReason}");
			return sentDm;
		}
	}
}
