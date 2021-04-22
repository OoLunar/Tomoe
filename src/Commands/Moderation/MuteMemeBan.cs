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
		public async Task Antimeme(CommandContext context, DiscordUser victim, [RemainingText] string antimemeReason = Constants.MissingReason) => await Punish(context, victim, antimemeReason, RoleAction.Antimeme, (await Database.GuildConfigs.Where(guildConfig => guildConfig.Id == context.Guild.Id).DefaultIfEmpty(new GuildConfig(context.Guild.Id)).SingleAsync()).AntimemeRole.GetRole(context.Guild), $"You've been antimemed from {Formatter.Bold(context.Guild.Name)}. Reason: {Formatter.BlockCode(Formatter.Strip(antimemeReason))}Context: {context.Message.JumpLink}\nNote: Antimeme prevents you from reacting to messages, sending embeds, uploading files, streaming to voice channels, and adds the push-to-talk restriction to voice channels.");

		[Command("mute"), RequireGuild, RequireBotPermissions(Permissions.ManageRoles), RequireUserPermissions(Permissions.ManageMessages), Aliases("silence", "shut", "zip"), Description("Grants the victim the `Muted` role, which prevents them from sending messages, reacting to messages and speaking in voice channels."), Punishment(false)]
		public async Task Mute(CommandContext context, DiscordUser victim, [RemainingText] string muteReason = Constants.MissingReason) => await Punish(context, victim, muteReason, RoleAction.Mute, (await Database.GuildConfigs.Where(guildConfig => guildConfig.Id == context.Guild.Id).DefaultIfEmpty(new GuildConfig(context.Guild.Id)).SingleAsync()).MuteRole.GetRole(context.Guild), $"You've been muted from {Formatter.Bold(context.Guild.Name)}. Reason: {Formatter.BlockCode(Formatter.Strip(muteReason))}Context: {context.Message.JumpLink}\nNote: A mute prevents you from having any sort of interaction with the guild. It makes the entire guild readonly.");

		[Command("voiceban"), RequireGuild, RequireBotPermissions(Permissions.ManageRoles), RequireUserPermissions(Permissions.ManageMessages), Aliases("voice_ban", "vb"), Description("Grants the victim the `Voicebanned` role, which prevents them from connecting to voice channels."), Punishment(false)]
		public async Task Voiceban(CommandContext context, DiscordUser victim, [RemainingText] string voicebanReason = Constants.MissingReason) => await Punish(context, victim, voicebanReason, RoleAction.Voiceban, (await Database.GuildConfigs.Where(guildConfig => guildConfig.Id == context.Guild.Id).DefaultIfEmpty(new GuildConfig(context.Guild.Id)).SingleAsync()).VoicebanRole.GetRole(context.Guild), $"You've been voicebanned from {Formatter.Bold(context.Guild.Name)}. Reason: {Formatter.BlockCode(Formatter.Strip(voicebanReason))}Context: {context.Message.JumpLink}\nNote: A voiceban prevents you from connecting to voice channels.");

		public async Task Punish(CommandContext context, DiscordUser victim, string punishReason, RoleAction roleAction, DiscordRole discordRole, string dmMessage)
		{
			if (discordRole == null)
			{
				_ = await Program.SendMessage(context, Constants.MissingRole);
				return;
			}

			bool sentDm = await ByProgram(context.Guild, victim, context.User, punishReason, roleAction, discordRole, dmMessage);
			_ = await Program.SendMessage(context, $"{victim.Mention} has been {roleAction.Humanize()}{(sentDm ? '.' : " (Failed to dm).")}");
		}

		public static async Task<bool> ByProgram(DiscordGuild discordGuild, DiscordUser victim, DiscordUser issuer, string punishReason, RoleAction roleAction, DiscordRole discordRole, string dmMessage)
		{
			DiscordMember discordVictim = await victim.Id.GetMember(discordGuild);

			using IServiceScope scope = Program.ServiceProvider.CreateScope();
			Database database = scope.ServiceProvider.GetService<Database>();
			GuildConfig guildConfig = await database.GuildConfigs.Where(guildConfig => guildConfig.Id == discordGuild.Id).DefaultIfEmpty(new GuildConfig(discordGuild.Id)).SingleAsync();
			GuildUser databaseVictim = await database.GuildUsers.Where(guildUser => guildUser.UserId == victim.Id && guildUser.GuildId == discordGuild.Id).DefaultIfEmpty(new GuildUser(victim.Id)).SingleAsync();

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
						_ = await discordVictim.SendMessageAsync(dmMessage);
						sentDm = true;
					}
					catch (Exception) { }
				}
			}

			string rolename = roleAction switch
			{
				RoleAction.Mute => "muted",
				RoleAction.Antimeme => "antimemed",
				RoleAction.Voiceban => "voicebanned",
				_ => "punished"
			};

			await ModLogs.Record(discordGuild.Id, roleAction.ToString(), $"{victim.Mention} has been {rolename}{(sentDm ? '.' : " (Failed to dm).")} Reason: {punishReason}");
			return sentDm;
		}
	}
}
