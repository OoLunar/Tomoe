using System.Linq;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

using Humanizer;
using Microsoft.EntityFrameworkCore;
using Tomoe.Db;
using Tomoe.Types;

namespace Tomoe.Commands.Moderation
{
	[Group("config")]
	public class Config : BaseCommandModule
	{
		#region MuteCommand

		[Command("mute"), Description("Sets up or assigns the mute role."), RequireUserPermissions(Permissions.ManageGuild), RequireGuild]
		public async Task Mute(CommandContext context, DiscordRole muteRole)
		{
			Guild guild = await Program.Database.Guilds.FirstOrDefaultAsync(guild => guild.Id == context.Guild.Id);
			DiscordRole previousMuteRole = guild.MuteRole.GetRole(context.Guild);
			if (previousMuteRole == null)
			{
				guild.MuteRole = muteRole.Id;
				await FixPermissions(context.Guild, muteRole, PermissionType.Mute);
				_ = await Program.SendMessage(context, $"{muteRole.Mention} is now set as the mute role.");

				return;
			}

			DiscordMessage discordMessage = await Program.SendMessage(context, $"Previous mute role was {previousMuteRole.Mention}. Do you want to overwrite it with {muteRole.Mention}?");
			_ = new Queue(discordMessage, context.User, new(async eventArgs =>
			{
				if (eventArgs.Emoji == Queue.ThumbsUp)
				{
					guild.MuteRole = muteRole.Id;
					_ = await discordMessage.ModifyAsync($"{Formatter.Strike(discordMessage.Content)}\n{Formatter.Bold("[Notice: Fixing role permissions...]")}");
					await FixPermissions(context.Guild, muteRole, PermissionType.Mute);
					_ = await discordMessage.ModifyAsync($"{muteRole.Mention} is now set as the mute role.");
				}
				else if (eventArgs.Emoji == Queue.ThumbsDown)
				{
					_ = await discordMessage.ModifyAsync($"{Formatter.Strike(discordMessage.Content)}\n{Formatter.Bold("[Notice: Fixing role permissions...]")}");
					await FixPermissions(context.Guild, previousMuteRole, PermissionType.Mute);
					_ = await discordMessage.ModifyAsync($"{Formatter.Strike(discordMessage.Content)}\n{Formatter.Bold("[Notice: Mute role has not been changed.]")}");
				}
			}));
		}


		[Command("mute"), RequireUserPermissions(Permissions.ManageGuild), RequireBotPermissions(Permissions.ManageRoles), RequireGuild]
		public async Task Mute(CommandContext context)
		{
			Guild guild = await Program.Database.Guilds.FirstOrDefaultAsync(guild => guild.Id == context.Guild.Id);
			DiscordRole previousMuteRole = guild.MuteRole.GetRole(context.Guild);
			if (previousMuteRole == null) // Should only be executed if there was no previous mute role id, or if the role cannot be found.
			{
				await CreateRole(context.Guild, null, PermissionType.Mute);
				return;
			}

			DiscordMessage discordMessage = await Program.SendMessage(context, $"Previous mute role was {previousMuteRole.Mention}. Do you want to overwrite it?");
			_ = new Queue(discordMessage, context.User, new(async eventArgs =>
			{
				if (eventArgs.Emoji == Queue.ThumbsUp) await CreateRole(context.Guild, discordMessage, PermissionType.Mute);
				else if (eventArgs.Emoji == Queue.ThumbsDown)
				{
					await FixPermissions(context.Guild, previousMuteRole, PermissionType.Mute);
					_ = await Program.SendMessage(context, $"Roles were left untouched.");
				}
			}));
		}

		#endregion MuteCommand
		#region AntimemeCommand

		[Command("antimeme"), Description("Sets up or assigns the antimeme role."), RequireUserPermissions(Permissions.ManageGuild), RequireGuild, Aliases("anti_meme", "nomeme", "no_meme", "memeban", "meme_ban")]
		public async Task Antimeme(CommandContext context, DiscordRole antimemeRole)
		{
			Guild guild = await Program.Database.Guilds.FirstOrDefaultAsync(guild => guild.Id == context.Guild.Id);
			DiscordRole previousAntimemeRole = guild.AntimemeRole.GetRole(context.Guild);
			if (previousAntimemeRole == null)
			{
				guild.AntimemeRole = antimemeRole.Id;
				await FixPermissions(context.Guild, antimemeRole, PermissionType.Antimeme);
				_ = await Program.SendMessage(context, $"{antimemeRole.Mention} is now set as the antimeme role.");
				return;
			}

			DiscordMessage discordMessage = await Program.SendMessage(context, $"Previous antimeme role was {previousAntimemeRole.Mention}. Do you want to overwrite it with {antimemeRole.Mention}?");
			_ = new Queue(discordMessage, context.User, new(async eventArgs =>
			{
				if (eventArgs.Emoji == Queue.ThumbsUp)
				{
					guild.AntimemeRole = antimemeRole.Id;
					_ = await discordMessage.ModifyAsync($"{Formatter.Strike(discordMessage.Content)}\n{Formatter.Bold("[Notice: Fixing role permissions...]")}");
					await FixPermissions(context.Guild, antimemeRole, PermissionType.Antimeme);
					_ = await discordMessage.ModifyAsync($"{antimemeRole.Mention} is now set as the antimeme role.");
				}
				else if (eventArgs.Emoji == Queue.ThumbsDown)
				{
					_ = await discordMessage.ModifyAsync($"{Formatter.Strike(discordMessage.Content)}\n{Formatter.Bold("[Notice: Fixing role permissions...]")}");
					await FixPermissions(context.Guild, antimemeRole, PermissionType.Antimeme);
					_ = await discordMessage.ModifyAsync($"{Formatter.Strike(discordMessage.Content)}\n{Formatter.Bold("[Notice: Antimeme role has not been changed.]")}");
				}
			}));
		}

		[Command("antimeme"), RequireUserPermissions(Permissions.ManageGuild), RequireBotPermissions(Permissions.ManageRoles), RequireGuild]
		public async Task Antimeme(CommandContext context)
		{
			Guild guild = await Program.Database.Guilds.FirstOrDefaultAsync(guild => guild.Id == context.Guild.Id);
			DiscordRole previousAntimemeRole = guild.AntimemeRole.GetRole(context.Guild);
			if (previousAntimemeRole == null) // Should only be executed if there was no previous mute role id, or if the role cannot be found.
			{
				await CreateRole(context.Guild, null, PermissionType.Antimeme);
				return;
			}

			DiscordMessage discordMessage = await Program.SendMessage(context, $"Previous antimeme role was {previousAntimemeRole.Mention}. Do you want to overwrite it?");
			_ = new Queue(discordMessage, context.User, new(async eventArgs =>
			{
				if (eventArgs.Emoji == Queue.ThumbsUp) await CreateRole(context.Guild, discordMessage, PermissionType.Antimeme);
				else if (eventArgs.Emoji == Queue.ThumbsDown)
				{
					await FixPermissions(context.Guild, previousAntimemeRole, PermissionType.Antimeme);
					_ = await Program.SendMessage(context, $"Roles were left untouched.");
				}
			}));
		}

		#endregion AntimemeCommand
		#region VoiceBanCommand

		[Command("voiceban"), Description("Sets up or assigns the voiceban role."), RequireUserPermissions(Permissions.ManageGuild), RequireGuild, Aliases("voice_ban", "vb")]
		public async Task VoiceBan(CommandContext context, DiscordRole voiceBanRole)
		{
			Guild guild = await Program.Database.Guilds.FirstOrDefaultAsync(guild => guild.Id == context.Guild.Id);
			DiscordRole previousVoiceBanRole = guild.VoiceBanRole.GetRole(context.Guild);
			if (previousVoiceBanRole == null)
			{
				guild.VoiceBanRole = voiceBanRole.Id;
				await FixPermissions(context.Guild, voiceBanRole, PermissionType.VoiceBan);
				_ = await Program.SendMessage(context, $"{voiceBanRole.Mention} is now set as the voiceban role.");
				return;
			}

			DiscordMessage discordMessage = await Program.SendMessage(context, $"Previous voiceban role was {previousVoiceBanRole.Mention}. Do you want to overwrite it with {voiceBanRole.Mention}?");
			_ = new Queue(discordMessage, context.User, new(async eventArgs =>
			{
				if (eventArgs.Emoji == Queue.ThumbsUp)
				{
					guild.VoiceBanRole = voiceBanRole.Id;
					_ = await discordMessage.ModifyAsync($"{Formatter.Strike(discordMessage.Content)}\n{Formatter.Bold("[Notice: Fixing role permissions...]")}");
					await FixPermissions(context.Guild, voiceBanRole, PermissionType.VoiceBan);
					_ = await discordMessage.ModifyAsync($"{voiceBanRole.Mention} is now set as the voiceban role.");
				}
				else if (eventArgs.Emoji == Queue.ThumbsDown)
				{
					_ = await discordMessage.ModifyAsync($"{Formatter.Strike(discordMessage.Content)}\n{Formatter.Bold("[Notice: Fixing role permissions...]")}");
					await FixPermissions(context.Guild, voiceBanRole, PermissionType.VoiceBan);
					_ = await discordMessage.ModifyAsync($"{Formatter.Strike(discordMessage.Content)}\n{Formatter.Bold("[Notice: Voiceban role has not been changed.]")}");
				}
			}));
		}

		[Command("voiceban"), RequireUserPermissions(Permissions.ManageGuild), RequireBotPermissions(Permissions.ManageRoles), RequireGuild]
		public async Task VoiceBan(CommandContext context)
		{
			Guild guild = await Program.Database.Guilds.FirstOrDefaultAsync(guild => guild.Id == context.Guild.Id);
			DiscordRole previousVoiceBanRole = guild.VoiceBanRole.GetRole(context.Guild);
			if (previousVoiceBanRole == null)
			{
				await CreateRole(context.Guild, null, PermissionType.VoiceBan);
				return;
			}

			DiscordMessage discordMessage = await Program.SendMessage(context, $"Previous voiceban role was {previousVoiceBanRole.Mention}. Do you want to overwrite it?");
			_ = new Queue(discordMessage, context.User, new(async eventArgs =>
			{
				if (eventArgs.Emoji == Queue.ThumbsUp) await CreateRole(context.Guild, discordMessage, PermissionType.VoiceBan);
				else if (eventArgs.Emoji == Queue.ThumbsDown)
				{
					await FixPermissions(context.Guild, previousVoiceBanRole, PermissionType.VoiceBan);
					_ = await Program.SendMessage(context, $"Roles were left untouched.");
				}
			}));
		}

		#endregion VoiceBanCommand

		public static async Task CreateRole(DiscordGuild discordGuild, DiscordMessage message, PermissionType permissionType)
		{
			if (message == null) message = await message.ModifyAsync($"Creating {permissionType.Humanize()} role...");
			Guild databaseGuild;
			DiscordRole role = null;
			switch (permissionType)
			{
				case PermissionType.Mute:
					databaseGuild = await Program.Database.Guilds.FirstOrDefaultAsync(guild => guild.Id == discordGuild.Id);
					role = await discordGuild.CreateRoleAsync("Muted", Permissions.None, DiscordColor.Gray, false, false, "Allows people to be muted.");
					databaseGuild.MuteRole = role.Id;
					break;
				case PermissionType.Antimeme:
					databaseGuild = await Program.Database.Guilds.FirstOrDefaultAsync(guild => guild.Id == discordGuild.Id);
					role = await discordGuild.CreateRoleAsync("Antimemed", Permissions.None, DiscordColor.Gray, false, false, "Allows people to meme no longer.");
					databaseGuild.AntimemeRole = role.Id;
					break;
				case PermissionType.VoiceBan:
					databaseGuild = await Program.Database.Guilds.FirstOrDefaultAsync(guild => guild.Id == discordGuild.Id);
					role = await discordGuild.CreateRoleAsync("Voicebanned", Permissions.None, DiscordColor.Gray, false, false, "Allows people to be banned from voice channels.");
					databaseGuild.VoiceBanRole = role.Id;
					break;
			}
			message = await message.ModifyAsync($"{Formatter.Strike(message.Content)}\nFixing channel permissions...");
			await FixPermissions(discordGuild, role, permissionType);
			_ = await message.ModifyAsync($"{Formatter.Strike(message.Content)}\nDone! {permissionType.Humanize()} role is now {role.Mention}!");
		}

		public static async Task FixPermissions(DiscordChannel channel, DiscordRole role, PermissionType permissionType)
		{
			Permissions textChannelPerms = permissionType switch
			{
				PermissionType.Mute => Permissions.SendMessages | Permissions.AddReactions,
				PermissionType.Antimeme => Permissions.AttachFiles | Permissions.AddReactions | Permissions.EmbedLinks | Permissions.UseExternalEmojis,
				PermissionType.VoiceBan => Permissions.None,
				_ => Permissions.None
			};

			Permissions voiceChannelPerms = permissionType switch
			{
				PermissionType.Mute => Permissions.Speak | Permissions.Stream,
				PermissionType.Antimeme => Permissions.Stream | Permissions.UseVoiceDetection,
				PermissionType.VoiceBan => Permissions.UseVoice,
				_ => Permissions.None
			};

			Permissions categoryPerms = textChannelPerms | voiceChannelPerms;

			switch (channel.Type)
			{
				case ChannelType.Voice:
					await channel.AddOverwriteAsync(role, Permissions.None, voiceChannelPerms, "Disallows users to connect to the voicechat.");
					break;
				case ChannelType.Category:
					await channel.AddOverwriteAsync(role, Permissions.None, categoryPerms, "Disallows users to connect to the voicechannels.");
					break;
				default:
					await channel.AddOverwriteAsync(role, Permissions.None, textChannelPerms, $"Configuring permissions for {permissionType.Humanize()} role.");
					break;
			}
		}

		public static async Task FixPermissions(DiscordGuild guild, DiscordRole role, PermissionType permissionType)
		{
			foreach (DiscordChannel channel in guild.Channels.Values)
			{
				await FixPermissions(channel, role, permissionType);
				await Task.Delay(50);
			}
		}

		public enum PermissionType
		{
			Mute,
			Antimeme,
			VoiceBan
		}

		[Command("anti_invite"), RequireUserPermissions(Permissions.ManageGuild), RequireGuild, Aliases("antiinvite", "antinvite")]
		public async Task AntiInvite(CommandContext context, bool isEnabled = true)
		{
			Guild guild = await Program.Database.Guilds.FirstOrDefaultAsync(guild => guild.Id == context.Guild.Id);
			guild.AntiInvite = isEnabled;
			_ = await Program.SendMessage(context, "Anti-Invite is now enabled!");
		}
	}
}
