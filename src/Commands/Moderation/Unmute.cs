using System;
using System.Linq;
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
	public class Unmute : BaseCommandModule
	{
		public Database Database { private get; set; }
		[Command("unmute"), RequireGuild, RequireBotPermissions(Permissions.ManageRoles), RequireUserPermissions(Permissions.ManageMessages), Aliases("unsilence", "unshut", "unzip"), Description("Removes the `Muted` role from the victim, allowing them to interact with the guild again."), Punishment(false)]
		public async Task ByUser(CommandContext context, DiscordUser victim, [RemainingText] string unmuteReason = Constants.MissingReason)
		{
			// Test if the guild is in the database. Bot owner might've removed it on accident, and we don't want the bot to fail completely if the guild is missing.
			Guild guild = await Database.Guilds.FirstOrDefaultAsync(guild => guild.Id == context.Guild.Id);
			if (guild == null)
			{
				_ = await Program.SendMessage(context, Constants.GuildNotInDatabase);
				return;
			}

			// GetRole is used in case the role id is 0 (default value) and will either return the Discord role or null
			DiscordRole muteRole = guild.MuteRole.GetRole(context.Guild);
			if (muteRole == null)
			{
				_ = await Program.SendMessage(context, Constants.MissingRole);
				return;
			}

			DiscordMember guildVictim = await victim.Id.GetMember(context.Guild);

			// Get databaseVictim or create it if they don't exist
			GuildUser databaseVictim = guild.Users.FirstOrDefault(user => user.Id == victim.Id);
			if (databaseVictim == null)
			{
				databaseVictim = new(victim.Id);
				if (guildVictim != null)
				{
					databaseVictim.Roles = guildVictim.Roles.Except(new[] { context.Guild.EveryoneRole }).Select(role => role.Id).ToList();
				}
			}
			databaseVictim.IsMuted = false;

			// If the user is in the guild, assign the muted role
			bool sentDm = false;
			if (guildVictim != null)
			{
				await guildVictim.RevokeRoleAsync(muteRole, unmuteReason);
				// If the user isn't a bot, attempt to dm them to make them aware of their punishment
				if (!guildVictim.IsBot)
				{
					try
					{
						_ = await guildVictim.SendMessageAsync($"You've been unmuted from {Formatter.Bold(context.Guild.Name)}. Reason: {Formatter.BlockCode(Formatter.Strip(unmuteReason))}Context: {context.Message.JumpLink}");
						sentDm = true;
					}
					catch (Exception) { }
				}
			}

			_ = await Database.SaveChangesAsync();
			_ = await Program.SendMessage(context, $"{victim.Mention} has been unmuted{(sentDm ? '.' : " (Failed to dm).")}");
		}

		public static async Task ByProgram(DiscordGuild discordGuild, DiscordUser victim, Uri jumplink, string unmuteReason = Constants.MissingPermissions)
		{
			using IServiceScope scope = Program.ServiceProvider.CreateScope();
			Database database = scope.ServiceProvider.GetService<Database>();
			// Test if the guild is in the database. Bot owner might've removed it on accident, and we don't want the bot to fail completely if the guild is missing.
			Guild guild = await database.Guilds.FirstOrDefaultAsync(guild => guild.Id == discordGuild.Id);
			if (guild == null) return;

			// GetRole is used in case the role id is 0 (default value) and will either return the Discord role or null
			DiscordRole muteRole = guild.MuteRole.GetRole(discordGuild);
			if (muteRole == null) return;

			DiscordMember guildVictim = await victim.Id.GetMember(discordGuild);

			// Get databaseVictim or create it if they don't exist
			GuildUser databaseVictim = guild.Users.FirstOrDefault(user => user.Id == victim.Id);
			if (databaseVictim == null)
			{
				databaseVictim = new(victim.Id);
				if (guildVictim != null)
				{
					databaseVictim.Roles = guildVictim.Roles.Except(new[] { discordGuild.EveryoneRole }).Select(role => role.Id).ToList();
				}
			}
			databaseVictim.IsMuted = false;

			// If the user is in the guild, assign the muted role
			if (guildVictim != null)
			{
				await guildVictim.RevokeRoleAsync(muteRole, unmuteReason);
				// If the user isn't a bot, attempt to dm them to make them aware of their punishment
				if (!guildVictim.IsBot)
				{
					try
					{
						_ = await guildVictim.SendMessageAsync($"You've been unmuted from {Formatter.Bold(discordGuild.Name)}. Reason: {Formatter.BlockCode(Formatter.Strip(unmuteReason))}Context: {jumplink}");
					}
					catch (Exception) { }
				}
			}

			_ = await database.SaveChangesAsync();
		}
	}
}
