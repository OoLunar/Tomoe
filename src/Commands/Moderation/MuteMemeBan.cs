using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tomoe.Commands.Moderation.Attributes;
using Tomoe.Db;
using static Tomoe.Commands.Moderation.Config;

namespace Tomoe.Commands.Moderation
{
	public class MuteMemeBan : BaseCommandModule
	{
		public Database Database { private get; set; }

		[Command("antimeme"), RequireGuild, RequireBotPermissions(Permissions.ManageRoles), RequireUserPermissions(Permissions.ManageMessages), Aliases("anti_meme", "memeban", "meme_ban", "nomeme", "no_meme"), Description("Grants the victim the `Antimeme` role, which prevents them from reacting to messages, embedding links and uploading files. The voice channels, this prevents the victim from streaming and they must use push-to-talk. The intention of this role is to prevent abuse of Discord's rich messaging features, or when someone is being really annoying by conversating with every known method except with messages."), Punishment(false)]
		public async Task Antimeme(CommandContext context, DiscordUser victim, [RemainingText] string antimemeReason = Constants.MissingReason) => await Punish(context, victim, antimemeReason, (await Database.GuildConfigs.FirstOrDefaultAsync(guild => guild.Id == context.Guild.Id))?.AntimemeRole.GetRole(context.Guild), RoleAction.Antimeme, $"You've been antimemed from {Formatter.Bold(context.Guild.Name)}. Reason: {Formatter.BlockCode(Formatter.Strip(antimemeReason))}Context: {context.Message.JumpLink}\nNote: Antimeme prevents you from reacting to messages, sending embeds, uploading files, streaming to voice channels, and adds the push-to-talk restriction to voice channels.");

		[Command("mute"), RequireGuild, RequireBotPermissions(Permissions.ManageRoles), RequireUserPermissions(Permissions.ManageMessages), Aliases("silence", "shut", "zip"), Description("Grants the victim the `Muted` role, which prevents them from sending messages, reacting to messages and speaking in voice channels."), Punishment(false)]
		public async Task Mute(CommandContext context, DiscordUser victim, [RemainingText] string muteReason = Constants.MissingReason) => await Punish(context, victim, muteReason, (await Database.GuildConfigs.FirstOrDefaultAsync(guild => guild.Id == context.Guild.Id))?.MuteRole.GetRole(context.Guild), RoleAction.Mute, $"You've been muted from {Formatter.Bold(context.Guild.Name)}. Reason: {Formatter.BlockCode(Formatter.Strip(muteReason))}Context: {context.Message.JumpLink}\nNote: A mute prevents you from having any sort of interaction with the guild. It makes the entire guild readonly.");

		[Command("voiceban"), RequireGuild, RequireBotPermissions(Permissions.ManageRoles), RequireUserPermissions(Permissions.ManageMessages), Aliases("voice_ban", "vb"), Description("Grants the victim the `Voicebanned` role, which prevents them from connecting to voice channels."), Punishment(false)]
		public async Task Voiceban(CommandContext context, DiscordUser victim, [RemainingText] string voicebanReason = Constants.MissingReason) => await Punish(context, victim, voicebanReason, (await Database.GuildConfigs.FirstOrDefaultAsync(guild => guild.Id == context.Guild.Id))?.VoicebanRole.GetRole(context.Guild), RoleAction.Voiceban, $"You've been voicebanned from {Formatter.Bold(context.Guild.Name)}. Reason: {Formatter.BlockCode(Formatter.Strip(voicebanReason))}Context: {context.Message.JumpLink}\nNote: A voiceban prevents you from connecting to voice channels.");

		public async Task Punish(CommandContext context, DiscordUser victim, string punishReason, DiscordRole discordRole, RoleAction roleAction, string victimMessage)
		{
			if (discordRole == null)
			{
				// TODO: Make a MissingRoleException. Have CommandService handle it by prompting the user to create the role.
				_ = await Program.SendMessage(context, Constants.MissingRole);
				return;
			}

			DiscordMember discordVictim = await victim.Id.GetMember(context.Guild);

			// Get databaseVictim or create it if they don't exist
			GuildConfig guild = await Database.GuildConfigs.DefaultIfEmpty(new GuildConfig(context.Guild.Id)).SingleAsync();
			GuildUser databaseVictim = Database.GuildUsers.FirstOrDefault(user => user.UserId == victim.Id && user.GuildId == context.Guild.Id);
			if (databaseVictim == null)
			{
				databaseVictim = new(victim.Id);
				if (discordVictim != null)
				{
					databaseVictim.Roles = discordVictim.Roles.Except(new[] { context.Guild.EveryoneRole }).Select(role => role.Id).ToList();
				}
			}

			switch (roleAction)
			{
				case RoleAction.Antimeme:
					databaseVictim.IsAntimemed = true;
					break;
				case RoleAction.Mute:
					databaseVictim.IsMuted = true;
					break;
				case RoleAction.Voiceban:
					databaseVictim.IsVoicebanned = true;
					break;
				default:
					throw new ArgumentException("Unable to determine which punishment to set.");
			}

			// If the user is in the guild, assign the muted role
			bool sentDm = false;
			if (discordVictim != null)
			{
				await discordVictim.GrantRoleAsync(discordRole, punishReason);
				// If the user isn't a bot, attempt to dm them to make them aware of their punishment
				if (!discordVictim.IsBot)
				{
					try
					{
						_ = await discordVictim.SendMessageAsync(victimMessage);
						sentDm = true;
					}
					catch (Exception) { }
				}
			}

			if (guild.ProgressiveStrikes)
			{
				Strike strike = new();
				strike.GuildId = context.Guild.Id;
				strike.IssuerId = context.User.Id;
				strike.JumpLinks.Add(context.Message.JumpLink);
				strike.Reasons.Add(punishReason);
				strike.VictimId = victim.Id;
				strike.VictimMessaged = sentDm;
				_ = Database.Strikes.Add(strike);
				_ = await Database.SaveChangesAsync();
				await Strikes.ProgressiveStrike(context.Guild, victim, strike);
			}

			_ = await Program.SendMessage(context, $"{victim.Mention} has been {roleAction.Humanize()}{(sentDm ? '.' : " (Failed to dm).")}");
		}

		public static async Task ByProgram(DiscordGuild discordGuild, DiscordUser victim, Uri jumplink, string muteReason = Constants.MissingPermissions)
		{
			using IServiceScope scope = Program.ServiceProvider.CreateScope();
			Database database = scope.ServiceProvider.GetService<Database>();
			// Test if the guild is in the database. Bot owner might've removed it on accident, and we don't want the bot to fail completely if the guild is missing.
			GuildConfig guild = await database.GuildConfigs.FirstOrDefaultAsync(guild => guild.Id == discordGuild.Id);
			if (guild == null) return;

			// GetRole is used in case the role id is 0 (default value) and will either return the Discord role or null
			DiscordRole muteRole = guild.MuteRole.GetRole(discordGuild);
			if (muteRole == null) return;

			DiscordMember guildVictim = await victim.Id.GetMember(discordGuild);

			// Get databaseVictim or create it if they don't exist
			GuildUser databaseVictim = database.GuildUsers.FirstOrDefault(user => user.UserId == victim.Id);
			if (databaseVictim == null)
			{
				databaseVictim = new(victim.Id);
				if (guildVictim != null)
				{
					databaseVictim.Roles = guildVictim.Roles.Except(new[] { discordGuild.EveryoneRole }).Select(role => role.Id).ToList();
				}
			}
			databaseVictim.IsMuted = true;

			// If the user is in the guild, assign the muted role
			bool sentDm = false;
			if (guildVictim != null)
			{
				await guildVictim.GrantRoleAsync(muteRole, muteReason);
				// If the user isn't a bot, attempt to dm them to make them aware of their punishment
				if (!guildVictim.IsBot)
				{
					try
					{
						_ = await guildVictim.SendMessageAsync($"You've been muted from {Formatter.Bold(discordGuild.Name)}. Reason: {Formatter.BlockCode(Formatter.Strip(muteReason))}Context: {jumplink}\nNote: A mute prevents you from having any sort of interaction with the guild. It makes the entire guild readonly.");
						sentDm = true;
					}
					catch (Exception) { }
				}
			}

			if (guild.ProgressiveStrikes)
			{
				Strike strike = new();
				strike.GuildId = discordGuild.Id;
				strike.IssuerId = Program.Client.CurrentUser.Id;
				strike.JumpLinks.Add(jumplink);
				strike.Reasons.Add(muteReason);
				strike.VictimId = victim.Id;
				strike.VictimMessaged = sentDm;
				_ = database.Strikes.Add(strike);
				_ = await database.SaveChangesAsync();
				await Strikes.ProgressiveStrike(discordGuild, victim, strike);
			}
		}
	}
}
