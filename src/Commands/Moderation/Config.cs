using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

using Humanizer;

using Tomoe.Types;
using Tomoe.Utils;

namespace Tomoe.Commands.Moderation
{
	[Group("config")]
	public class Config : BaseCommandModule
	{
		private static readonly Logger _logger = new("Commands.Config");

		#region MuteCommand

		[Command("mute"), Description("Sets up or assigns the mute role."), RequireUserPermissions(Permissions.ManageGuild), RequireGuild]
		public async Task Mute(CommandContext context, DiscordRole muteRole)
		{
			DiscordRole previousMuteRole = Program.Database.Guild.MuteRole(context.Guild.Id).GetRole(context.Guild);
			if (previousMuteRole == null)
			{
				Program.Database.Guild.MuteRole(context.Guild.Id, muteRole.Id);
				_logger.Trace($"Set {muteRole.Name} ({muteRole.Id}) as mute role for {context.Guild.Name} ({context.Guild.Id})!");
				await FixPermissions(context.Guild, muteRole, PermissionType.Mute);
				_ = await Program.SendMessage(context, $"{muteRole.Mention} is now set as the mute role.");

				return;
			}

			DiscordMessage discordMessage = await Program.SendMessage(context, $"Previous mute role was {previousMuteRole.Mention}. Do you want to overwrite it with {muteRole.Mention}?");
			_ = new Queue(discordMessage, context.User, new(async eventArgs =>
			{
				if (eventArgs.Emoji == Queue.ThumbsUp)
				{
					Program.Database.Guild.MuteRole(context.Guild.Id, muteRole.Id);
					_logger.Trace($"Set {muteRole.Name} ({muteRole.Id}) as mute role for {context.Guild.Name} ({context.Guild.Id})!");
					_ = await discordMessage.ModifyAsync($"{Formatter.Strike(discordMessage.Content)}\n{Formatter.Bold("[Notice: Fixing role permissions...]")}");
					await FixPermissions(context.Guild, muteRole, PermissionType.Mute);
					_ = await discordMessage.ModifyAsync($"{muteRole.Mention} is now set as the mute role.");
				}
				else if (eventArgs.Emoji == Queue.ThumbsDown)
				{
					_logger.Trace($"Fixed permissions for the mute role {previousMuteRole.Name} ({previousMuteRole.Id}) on {context.Guild.Name} ({context.Guild.Id})!");
					_ = await discordMessage.ModifyAsync($"{Formatter.Strike(discordMessage.Content)}\n{Formatter.Bold("[Notice: Fixing role permissions...]")}");
					await FixPermissions(context.Guild, previousMuteRole, PermissionType.Mute);
					_ = await discordMessage.ModifyAsync($"{Formatter.Strike(discordMessage.Content)}\n{Formatter.Bold("[Notice: Mute role has not been changed.]")}");
				}
			}));
		}


		[Command("mute"), RequireUserPermissions(Permissions.ManageGuild), RequireBotPermissions(Permissions.ManageRoles), RequireGuild]
		public async Task Mute(CommandContext context)
		{
			DiscordRole previousMuteRole = Program.Database.Guild.MuteRole(context.Guild.Id).GetRole(context.Guild);
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
					_logger.Trace($"Fixed permissions for the mute role {previousMuteRole.Name} ({previousMuteRole.Id}) on {context.Guild.Name} ({context.Guild.Id})!");
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
			DiscordRole previousAntiMemeRole = Program.Database.Guild.AntimemeRole(context.Guild.Id).GetRole(context.Guild);
			if (previousAntiMemeRole == null)
			{
				Program.Database.Guild.AntimemeRole(context.Guild.Id, antimemeRole.Id);
				_logger.Trace($"Set {antimemeRole.Name} ({antimemeRole.Id}) as the antimeme role for {context.Guild.Name} ({context.Guild.Id})!");
				await FixPermissions(context.Guild, antimemeRole, PermissionType.Antimeme);
				_ = await Program.SendMessage(context, $"{antimemeRole.Mention} is now set as the antimeme role.");
				return;
			}

			DiscordMessage discordMessage = await Program.SendMessage(context, $"Previous antimeme role was {previousAntiMemeRole.Mention}. Do you want to overwrite it with {antimemeRole.Mention}?");
			_ = new Queue(discordMessage, context.User, new(async eventArgs =>
			{
				if (eventArgs.Emoji == Queue.ThumbsUp)
				{
					Program.Database.Guild.AntimemeRole(context.Guild.Id, antimemeRole.Id);
					_logger.Trace($"Set {antimemeRole.Name} ({antimemeRole.Id}) as the antimeme role for {context.Guild.Name} ({context.Guild.Id})!");
					_ = await discordMessage.ModifyAsync($"{Formatter.Strike(discordMessage.Content)}\n{Formatter.Bold("[Notice: Fixing role permissions...]")}");
					await FixPermissions(context.Guild, antimemeRole, PermissionType.Antimeme);
					_ = await discordMessage.ModifyAsync($"{antimemeRole.Mention} is now set as the antimeme role.");
				}
				else if (eventArgs.Emoji == Queue.ThumbsDown)
				{
					_logger.Trace($"Fixed permissions for the antimeme role {previousAntiMemeRole.Name} ({previousAntiMemeRole.Id}) on {context.Guild.Name} ({context.Guild.Id})!");
					_ = await discordMessage.ModifyAsync($"{Formatter.Strike(discordMessage.Content)}\n{Formatter.Bold("[Notice: Fixing role permissions...]")}");
					await FixPermissions(context.Guild, antimemeRole, PermissionType.Antimeme);
					_ = await discordMessage.ModifyAsync($"{Formatter.Strike(discordMessage.Content)}\n{Formatter.Bold("[Notice: Antimeme role has not been changed.]")}");
				}
			}));
		}

		[Command("antimeme"), RequireUserPermissions(Permissions.ManageGuild), RequireBotPermissions(Permissions.ManageRoles), RequireGuild]
		public async Task Antimeme(CommandContext context)
		{
			DiscordRole previousAntimemeRole = Program.Database.Guild.AntimemeRole(context.Guild.Id).GetRole(context.Guild);
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
					_logger.Trace($"Fixed permissions for the antimeme role {previousAntimemeRole.Name} ({previousAntimemeRole.Id}) on {context.Guild.Name} ({context.Guild.Id})!");
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
			DiscordRole previousVoiceBanRole = Program.Database.Guild.VoiceBanRole(context.Guild.Id).GetRole(context.Guild);
			if (previousVoiceBanRole == null)
			{
				Program.Database.Guild.AntimemeRole(context.Guild.Id, voiceBanRole.Id);
				_logger.Trace($"Set {voiceBanRole.Name} ({voiceBanRole.Id}) as the voiceban role for {context.Guild.Name} ({context.Guild.Id})!");
				await FixPermissions(context.Guild, voiceBanRole, PermissionType.VoiceBan);
				_ = await Program.SendMessage(context, $"{voiceBanRole.Mention} is now set as the voiceban role.");
				return;
			}

			DiscordMessage discordMessage = await Program.SendMessage(context, $"Previous voiceban role was {previousVoiceBanRole.Mention}. Do you want to overwrite it with {voiceBanRole.Mention}?");
			_ = new Queue(discordMessage, context.User, new(async eventArgs =>
			{
				if (eventArgs.Emoji == Queue.ThumbsUp)
				{
					Program.Database.Guild.VoiceBanRole(context.Guild.Id, voiceBanRole.Id);
					_logger.Trace($"Set {voiceBanRole.Name} ({voiceBanRole.Id}) as the voiceban role for {context.Guild.Name} ({context.Guild.Id})!");
					_ = await discordMessage.ModifyAsync($"{Formatter.Strike(discordMessage.Content)}\n{Formatter.Bold("[Notice: Fixing role permissions...]")}");
					await FixPermissions(context.Guild, voiceBanRole, PermissionType.VoiceBan);
					_ = await discordMessage.ModifyAsync($"{voiceBanRole.Mention} is now set as the voiceban role.");
				}
				else if (eventArgs.Emoji == Queue.ThumbsDown)
				{
					_logger.Trace($"Fixed permissions for the voiceban role {previousVoiceBanRole.Name} ({previousVoiceBanRole.Id}) on {context.Guild.Name} ({context.Guild.Id})!");
					_ = await discordMessage.ModifyAsync($"{Formatter.Strike(discordMessage.Content)}\n{Formatter.Bold("[Notice: Fixing role permissions...]")}");
					await FixPermissions(context.Guild, voiceBanRole, PermissionType.VoiceBan);
					_ = await discordMessage.ModifyAsync($"{Formatter.Strike(discordMessage.Content)}\n{Formatter.Bold("[Notice: Voiceban role has not been changed.]")}");
				}
			}));
		}

		[Command("voiceban"), RequireUserPermissions(Permissions.ManageGuild), RequireBotPermissions(Permissions.ManageRoles), RequireGuild]
		public async Task VoiceBan(CommandContext context)
		{
			DiscordRole previousVoiceBanRole = Program.Database.Guild.VoiceBanRole(context.Guild.Id).GetRole(context.Guild);
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
					_logger.Trace($"Fixed permissions for the voiceban role {previousVoiceBanRole.Name} ({previousVoiceBanRole.Id}) on {context.Guild.Name} ({context.Guild.Id})!");
					await FixPermissions(context.Guild, previousVoiceBanRole, PermissionType.VoiceBan);
					_ = await Program.SendMessage(context, $"Roles were left untouched.");
				}
			}));
		}

		#endregion VoiceBanCommand

		public static async Task CreateRole(DiscordGuild guild, DiscordMessage message, PermissionType permissionType)
		{
			if (message == null) message = await message.ModifyAsync($"Creating {permissionType.Humanize()} role...");
			DiscordRole role = null;
			switch (permissionType)
			{
				case PermissionType.Mute:
					role = await guild.CreateRoleAsync("Muted", Permissions.None, DiscordColor.Gray, false, false, "Allows people to be muted.");
					Program.Database.Guild.MuteRole(guild.Id, role.Id);
					break;
				case PermissionType.Antimeme:
					role = await guild.CreateRoleAsync("Antimemed", Permissions.None, DiscordColor.Gray, false, false, "Allows people to meme no longer.");
					Program.Database.Guild.AntimemeRole(guild.Id, role.Id);
					break;
				case PermissionType.VoiceBan:
					role = await guild.CreateRoleAsync("Voicebanned", Permissions.None, DiscordColor.Gray, false, false, "Allows people to be banned from voice channels.");
					Program.Database.Guild.VoiceBanRole(guild.Id, role.Id);
					break;
			}
			message = await message.ModifyAsync($"{Formatter.Strike(message.Content)}\nFixing channel permissions...");
			await FixPermissions(guild, role, permissionType);
			_ = await message.ModifyAsync($"{Formatter.Strike(message.Content)}\nDone! {permissionType.Humanize()} role is now {role.Mention}!");
		}

		public static async Task FixPermissions(DiscordChannel channel, DiscordRole role, PermissionType permissionType)
		{
			Permissions textChannelPerms = permissionType switch
			{
				PermissionType.Mute => Permissions.SendMessages | Permissions.AddReactions,
				PermissionType.Antimeme => Permissions.AttachFiles | Permissions.AddReactions | Permissions.EmbedLinks | Permissions.UseExternalEmojis,
				PermissionType.VoiceBan => Permissions.None
			};

			Permissions voiceChannelPerms = permissionType switch
			{
				PermissionType.Mute => Permissions.Speak | Permissions.Stream,
				PermissionType.Antimeme => Permissions.Stream | Permissions.UseVoiceDetection,
				PermissionType.VoiceBan => Permissions.UseVoice
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
		public async Task AntiInvite(CommandContext context, bool isEnabled)
		{
			Program.Database.Guild.AntiInvite(context.Guild.Id, isEnabled);
			_ = await Program.SendMessage(context, "Anti-Invite is now enabled!");
		}
	}
}
