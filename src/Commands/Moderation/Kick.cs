using System;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tomoe.Commands.Moderation.Attributes;
using Tomoe.Db;

namespace Tomoe.Commands.Moderation
{
	public class Kick : BaseCommandModule
	{
		public Database Database { private get; set; }
		[Command("kick"), RequireGuild, RequireUserPermissions(Permissions.BanMembers), RequireBotPermissions(Permissions.BanMembers), Aliases("boot", "yeet"), Description("Kicks the victim from the guild, sending them off with a dm."), Punishment(true)]
		public async Task ByUser(CommandContext context, DiscordMember victim, [RemainingText] string kickReason = Constants.MissingReason)
		{
			// Test if the guild is in the database. Bot owner might've removed it on accident, and we don't want the bot to fail completely if the guild is missing.
			Guild guild = await Database.Guilds.FirstOrDefaultAsync(guild => guild.Id == context.Guild.Id);
			if (guild == null)
			{
				_ = await Program.SendMessage(context, Constants.GuildNotInDatabase);
				return;
			}

			// If the user is in the guild, and if the user isn't a bot, attempt to dm them to make them aware of their punishment
			bool sentDm = false;
			if (victim != null && !victim.IsBot)
			{
				try
				{
					_ = await victim.SendMessageAsync($"You've been kicked from {Formatter.Bold(context.Guild.Name)}. Reason: {Formatter.BlockCode(Formatter.Strip(kickReason))}Context: {context.Message.JumpLink}");
					sentDm = true;
				}
				catch (Exception) { }
			}

			await victim.RemoveAsync(kickReason);

			if (guild.ProgressiveStrikes)
			{
				Strike strike = new();
				strike.GuildId = context.Guild.Id;
				strike.IssuerId = context.User.Id;
				strike.JumpLinks.Add(context.Message.JumpLink);
				strike.Reasons.Add(kickReason);
				strike.VictimId = victim.Id;
				strike.VictimMessaged = sentDm;
				_ = Database.Strikes.Add(strike);
				_ = await Database.SaveChangesAsync();
				await Strikes.ProgressiveStrike(context.Guild, victim, strike);
			}

			_ = await Program.SendMessage(context, $"{victim.Mention} has been kicked{(sentDm ? '.' : " (Failed to dm).")}");
		}

		public static async Task ByProgram(DiscordGuild discordGuild, DiscordMember victim, Uri jumplink, [RemainingText] string kickReason = Constants.MissingReason)
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

			// Test if the guild is in the database. Bot owner might've removed it on accident, and we don't want the bot to fail completely if the guild is missing.
			Guild guild = await database.Guilds.FirstOrDefaultAsync(guild => guild.Id == discordGuild.Id);
			if (guild != null && guild.ProgressiveStrikes)
			{
				Strike strike = new();
				strike.GuildId = discordGuild.Id;
				strike.IssuerId = Program.Client.CurrentUser.Id;
				strike.JumpLinks.Add(jumplink);
				strike.Reasons.Add(kickReason);
				strike.VictimId = victim.Id;
				strike.VictimMessaged = sentDm;
				_ = database.Strikes.Add(strike);
				_ = await database.SaveChangesAsync();
				await Strikes.ProgressiveStrike(discordGuild, victim, strike);
			}
		}
	}
}
