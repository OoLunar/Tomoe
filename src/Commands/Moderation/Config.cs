using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

using Tomoe.Types;
using Tomoe.Utils;

namespace Tomoe.Commands.Moderation
{
	[Group("config")]
	public class Config : BaseCommandModule
	{
		private static readonly Logger _muteLogger = new("Commands.Config.Mute");
		private static readonly Logger _antimemeLogger = new("Commands.Config.Antimeme");
		private static readonly Logger _voiceBanLogger = new("Commands.Config.VoiceBan");

		#region MuteCommand

		[Command("mute"), Description("Sets up or assigns the mute role."), RequireUserPermissions(Permissions.ManageGuild), RequireGuild]
		public async Task Mute(CommandContext context, DiscordRole muteRole)
		{
			DiscordRole previousMuteRole = Program.Database.Guild.MuteRole(context.Guild.Id).GetRole(context.Guild);
			if (previousMuteRole == null)
			{
				Program.Database.Guild.MuteRole(context.Guild.Id, muteRole.Id);
				_muteLogger.Trace($"Set {muteRole.Name} ({muteRole.Id}) as mute role for {context.Guild.Name} ({context.Guild.Id})!");
				await FixMuteRolePermissions(context.Guild, muteRole);
				_ = await Program.SendMessage(context, $"{muteRole.Mention} is now set as the mute role.");

				return;
			}

			DiscordMessage discordMessage = await Program.SendMessage(context, $"Previous mute role was {previousMuteRole.Mention}. Do you want to overwrite it with {muteRole.Mention}?");
			_ = new Queue(discordMessage, context.User, new(async eventArgs =>
			{
				if (eventArgs.Emoji == Queue.ThumbsUp)
				{
					Program.Database.Guild.MuteRole(context.Guild.Id, muteRole.Id);
					_muteLogger.Trace($"Set {muteRole.Name} ({muteRole.Id}) as mute role for {context.Guild.Name} ({context.Guild.Id})!");
					_ = await discordMessage.ModifyAsync($"{Formatter.Strike(discordMessage.Content)}\n{Formatter.Bold("[Notice: Fixing role permissions...]")}");
					await FixMuteRolePermissions(context.Guild, muteRole);
					_ = await discordMessage.ModifyAsync($"{muteRole.Mention} is now set as the mute role.");
				}
				else if (eventArgs.Emoji == Queue.ThumbsDown)
				{
					_muteLogger.Trace($"Fixed permissions for the mute role {previousMuteRole.Name} ({previousMuteRole.Id}) on {context.Guild.Name} ({context.Guild.Id})!");
					_ = await discordMessage.ModifyAsync($"{Formatter.Strike(discordMessage.Content)}\n{Formatter.Bold("[Notice: Fixing role permissions...]")}");
					await FixMuteRolePermissions(context.Guild, previousMuteRole);
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
				await CreateMuteRole(context, await Program.SendMessage(context, "Creating mute role..."));
				return;
			}

			DiscordMessage discordMessage = await Program.SendMessage(context, $"Previous mute role was {previousMuteRole.Mention}. Do you want to overwrite it?");
			_ = new Queue(discordMessage, context.User, new(async eventArgs =>
			{
				if (eventArgs.Emoji == Queue.ThumbsUp) await CreateMuteRole(context, discordMessage);
				else if (eventArgs.Emoji == Queue.ThumbsDown)
				{
					_muteLogger.Trace($"Fixed permissions for the mute role {previousMuteRole.Name} ({previousMuteRole.Id}) on {context.Guild.Name} ({context.Guild.Id})!");
					await FixMuteRolePermissions(context.Guild, previousMuteRole);
					_ = await Program.SendMessage(context, $"Roles were left untouched.");
				}
			}));
		}

		public static async Task CreateMuteRole(CommandContext context, DiscordMessage message)
		{
			_ = await message.ModifyAsync("Creating mute role...");
			DiscordRole muteRole = await context.Guild.CreateRoleAsync("Muted", Permissions.None, DiscordColor.Gray, false, false, "Allows users to be muted.");
			_muteLogger.Trace($"Created mute role \"{muteRole.Name}\" ({muteRole.Id}) for {context.Guild.Name} ({context.Guild.Id})!");
			Program.Database.Guild.MuteRole(context.Guild.Id, muteRole.Id);
			_ = await message.ModifyAsync($"Overriding channel permissions...");
			await FixMuteRolePermissions(context.Guild, muteRole);
			_ = await message.ModifyAsync($"Done! Mute role is now {muteRole.Mention}");
		}

		public static async Task FixMuteRolePermissions(DiscordGuild guild, DiscordRole muteRole)
		{
			foreach (DiscordChannel channel in guild.Channels.Values)
			{
				switch (channel.Type)
				{
					case ChannelType.Text or ChannelType.News or ChannelType.Unknown:
						_muteLogger.Trace($"Overwriting permission {Permissions.SendMessages} and {Permissions.AddReactions} for mute role {muteRole.Name} ({muteRole.Id}) on {channel.Type} channel {channel.Name} ({channel.Id}) for {guild.Name} ({guild.Id})...");
						await channel.AddOverwriteAsync(muteRole, Permissions.None, Permissions.SendMessages | Permissions.AddReactions, "Disallows users to send messages/communicate through reactions.");
						break;
					case ChannelType.Voice:
						_muteLogger.Trace($"Overwriting permission {Permissions.Speak} and {Permissions.Stream} for mute role {muteRole.Name} ({muteRole.Id}) on {channel.Type} channel {channel.Name} ({channel.Id}) for {guild.Name} ({guild.Id})...");
						await channel.AddOverwriteAsync(muteRole, Permissions.None, Permissions.Speak | Permissions.Stream, "Disallows users to communicate in voice channels and through streams.");
						break;
					case ChannelType.Category:
						_muteLogger.Trace($"Overwriting permission {Permissions.SendMessages}, {Permissions.AddReactions}, {Permissions.Speak} and {Permissions.Stream} for mute role {muteRole.Name} ({muteRole.Id}) on {channel.Type} channel {channel.Name} ({channel.Id}) for {guild.Name} ({guild.Id})...");
						await channel.AddOverwriteAsync(muteRole, Permissions.None, Permissions.SendMessages | Permissions.AddReactions | Permissions.Speak | Permissions.Stream, "Disallows users to send messages/communicate through reactions/voice channels and through streams.");
						break;
					default: break;
				}
				await Task.Delay(50);
			}
		}

		public static async Task FixMuteRolePermissions(DiscordChannel channel, DiscordRole muteRole)
		{
			switch (channel.Type)
			{
				case ChannelType.Text or ChannelType.News or ChannelType.Unknown:
					_muteLogger.Trace($"Overwriting permission {Permissions.SendMessages} and {Permissions.AddReactions} for mute role {muteRole.Name} ({muteRole.Id}) on {channel.Type} channel {channel.Name} ({channel.Id})...");
					await channel.AddOverwriteAsync(muteRole, Permissions.None, Permissions.SendMessages | Permissions.AddReactions, "Disallows users to send messages/communicate through reactions.");
					break;
				case ChannelType.Voice:
					_muteLogger.Trace($"Overwriting permission {Permissions.Speak} and {Permissions.Stream} for mute role {muteRole.Name} ({muteRole.Id}) on {channel.Type} channel {channel.Name} ({channel.Id})...");
					await channel.AddOverwriteAsync(muteRole, Permissions.None, Permissions.Speak | Permissions.Stream, "Disallows users to communicate in voice channels and through streams.");
					break;
				case ChannelType.Category:
					_muteLogger.Trace($"Overwriting permission {Permissions.SendMessages}, {Permissions.AddReactions}, {Permissions.Speak} and {Permissions.Stream} for mute role {muteRole.Name} ({muteRole.Id}) on {channel.Type} channel {channel.Name} ({channel.Id})...");
					await channel.AddOverwriteAsync(muteRole, Permissions.None, Permissions.SendMessages | Permissions.AddReactions | Permissions.Speak | Permissions.Stream, "Disallows users to send messages/communicate through reactions/voice channels and through streams.");
					break;
				default: break;
			}
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
				_antimemeLogger.Trace($"Set {antimemeRole.Name} ({antimemeRole.Id}) as the antimeme role for {context.Guild.Name} ({context.Guild.Id})!");
				await FixAntimemeRolePermissions(context.Guild, antimemeRole);
				_ = await Program.SendMessage(context, $"{antimemeRole.Mention} is now set as the antimeme role.");
				return;
			}

			DiscordMessage discordMessage = await Program.SendMessage(context, $"Previous antimeme role was {previousAntiMemeRole.Mention}. Do you want to overwrite it with {antimemeRole.Mention}?");
			_ = new Queue(discordMessage, context.User, new(async eventArgs =>
			{
				if (eventArgs.Emoji == Queue.ThumbsUp)
				{
					Program.Database.Guild.AntimemeRole(context.Guild.Id, antimemeRole.Id);
					_antimemeLogger.Trace($"Set {antimemeRole.Name} ({antimemeRole.Id}) as the antimeme role for {context.Guild.Name} ({context.Guild.Id})!");
					_ = await discordMessage.ModifyAsync($"{Formatter.Strike(discordMessage.Content)}\n{Formatter.Bold("[Notice: Fixing role permissions...]")}");
					await FixAntimemeRolePermissions(context.Guild, antimemeRole);
					_ = await discordMessage.ModifyAsync($"{antimemeRole.Mention} is now set as the antimeme role.");
				}
				else if (eventArgs.Emoji == Queue.ThumbsDown)
				{
					_antimemeLogger.Trace($"Fixed permissions for the antimeme role {previousAntiMemeRole.Name} ({previousAntiMemeRole.Id}) on {context.Guild.Name} ({context.Guild.Id})!");
					_ = await discordMessage.ModifyAsync($"{Formatter.Strike(discordMessage.Content)}\n{Formatter.Bold("[Notice: Fixing role permissions...]")}");
					await FixAntimemeRolePermissions(context.Guild, antimemeRole);
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
				await CreateAntiMemeRole(context, await Program.SendMessage(context, "Creating antimeme role..."));
				return;
			}

			DiscordMessage discordMessage = await Program.SendMessage(context, $"Previous antimeme role was {previousAntimemeRole.Mention}. Do you want to overwrite it?");
			_ = new Queue(discordMessage, context.User, new(async eventArgs =>
			{
				if (eventArgs.Emoji == Queue.ThumbsUp) await CreateAntiMemeRole(context, discordMessage);
				else if (eventArgs.Emoji == Queue.ThumbsDown)
				{
					_antimemeLogger.Trace($"Fixed permissions for the antimeme role {previousAntimemeRole.Name} ({previousAntimemeRole.Id}) on {context.Guild.Name} ({context.Guild.Id})!");
					await FixAntimemeRolePermissions(context.Guild, previousAntimemeRole);
					_ = await Program.SendMessage(context, $"Roles were left untouched.");
				}
			}));
		}

		public static async Task CreateAntiMemeRole(CommandContext context, DiscordMessage message)
		{
			_ = await message.ModifyAsync("Creating antimeme role...");
			DiscordRole antimemeRole = await context.Guild.CreateRoleAsync("Antimeme", Permissions.None, DiscordColor.Gray, false, false, "Allows users to be antimemed.");
			_antimemeLogger.Trace($"Created antimeme role \"{antimemeRole.Name}\" ({antimemeRole.Id}) for {context.Guild.Name} ({context.Guild.Id})!");
			Program.Database.Guild.AntimemeRole(context.Guild.Id, antimemeRole.Id);
			_ = await message.ModifyAsync($"Overriding channel permissions...");
			await FixAntimemeRolePermissions(context.Guild, antimemeRole);
			_ = await message.ModifyAsync($"Done! Antimeme role is now {antimemeRole.Mention}");
		}

		public static async Task FixAntimemeRolePermissions(DiscordGuild guild, DiscordRole antimemeRole)
		{
			foreach (DiscordChannel channel in guild.Channels.Values)
			{
				switch (channel.Type)
				{
					case ChannelType.Text or ChannelType.News or ChannelType.Unknown:
						_antimemeLogger.Trace($"Overwriting permission {Permissions.SendMessages} and {Permissions.AddReactions} for antimeme role {antimemeRole.Name} ({antimemeRole.Id}) on {channel.Type} channel {channel.Name} ({channel.Id}) for {guild.Name} ({guild.Id})...");
						await channel.AddOverwriteAsync(antimemeRole, Permissions.None, Permissions.AttachFiles | Permissions.AddReactions | Permissions.EmbedLinks | Permissions.UseExternalEmojis, "Stops members with the role from spamming chat with reactions, images or links.");
						break;
					case ChannelType.Voice:
						_antimemeLogger.Trace($"Overwriting permission {Permissions.Speak} and {Permissions.Stream} for antimeme role {antimemeRole.Name} ({antimemeRole.Id}) on {channel.Type} channel {channel.Name} ({channel.Id}) for {guild.Name} ({guild.Id})...");
						await channel.AddOverwriteAsync(antimemeRole, Permissions.None, Permissions.Stream | Permissions.UseVoiceDetection, "Disallows users to spam starting and stopping streams. Must push to talk.");
						break;
					case ChannelType.Category:
						_antimemeLogger.Trace($"Overwriting permission {Permissions.SendMessages}, {Permissions.AddReactions}, {Permissions.Speak} and {Permissions.Stream} for antimeme role {antimemeRole.Name} ({antimemeRole.Id}) on {channel.Type} channel {channel.Name} ({channel.Id}) for {guild.Name} ({guild.Id})...");
						await channel.AddOverwriteAsync(antimemeRole, Permissions.None, Permissions.AttachFiles | Permissions.AddReactions | Permissions.EmbedLinks | Permissions.UseExternalEmojis | Permissions.Stream | Permissions.UseVoiceDetection, "Stops members with the role from spamming chat with reactions, images or links/voice channels and through streams. Disallows users to spam starting and stopping streams. Must push to talk.");
						break;
					default: break;
				}
				await Task.Delay(50);
			}
		}

		public static async Task FixAntimemeRolePermissions(DiscordChannel channel, DiscordRole antimemeRole)
		{
			switch (channel.Type)
			{
				case ChannelType.Text or ChannelType.News or ChannelType.Unknown:
					_antimemeLogger.Trace($"Overwriting permission {Permissions.SendMessages} and {Permissions.AddReactions} for antimeme role {antimemeRole.Name} ({antimemeRole.Id}) on {channel.Type} channel {channel.Name} ({channel.Id})...");
					await channel.AddOverwriteAsync(antimemeRole, Permissions.None, Permissions.AttachFiles | Permissions.AddReactions | Permissions.EmbedLinks | Permissions.UseExternalEmojis, "Stops members with the role from spamming chat with reactions, images or links.");
					break;
				case ChannelType.Voice:
					_antimemeLogger.Trace($"Overwriting permission {Permissions.Speak} and {Permissions.Stream} for antimeme role {antimemeRole.Name} ({antimemeRole.Id}) on {channel.Type} channel {channel.Name} ({channel.Id})...");
					await channel.AddOverwriteAsync(antimemeRole, Permissions.None, Permissions.Stream | Permissions.UseVoiceDetection, "Disallows users to spam starting and stopping streams. Must push to talk.");
					break;
				case ChannelType.Category:
					_antimemeLogger.Trace($"Overwriting permission {Permissions.SendMessages}, {Permissions.AddReactions}, {Permissions.Speak} and {Permissions.Stream} for antimeme role {antimemeRole.Name} ({antimemeRole.Id}) on {channel.Type} channel {channel.Name} ({channel.Id})...");
					await channel.AddOverwriteAsync(antimemeRole, Permissions.None, Permissions.AttachFiles | Permissions.AddReactions | Permissions.EmbedLinks | Permissions.UseExternalEmojis | Permissions.Stream | Permissions.UseVoiceDetection, "Stops members with the role from spamming chat with reactions, images or links/voice channels and through streams. Disallows users to spam starting and stopping streams. Must push to talk.");
					break;
				default: break;
			}
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
				_voiceBanLogger.Trace($"Set {voiceBanRole.Name} ({voiceBanRole.Id}) as the voiceban role for {context.Guild.Name} ({context.Guild.Id})!");
				await FixVoiceBanPermissions(context.Guild, voiceBanRole);
				_ = await Program.SendMessage(context, $"{voiceBanRole.Mention} is now set as the voiceban role.");
				return;
			}

			DiscordMessage discordMessage = await Program.SendMessage(context, $"Previous voiceban role was {previousVoiceBanRole.Mention}. Do you want to overwrite it with {voiceBanRole.Mention}?");
			_ = new Queue(discordMessage, context.User, new(async eventArgs =>
			{
				if (eventArgs.Emoji == Queue.ThumbsUp)
				{
					Program.Database.Guild.VoiceBanRole(context.Guild.Id, voiceBanRole.Id);
					_voiceBanLogger.Trace($"Set {voiceBanRole.Name} ({voiceBanRole.Id}) as the voiceban role for {context.Guild.Name} ({context.Guild.Id})!");
					_ = await discordMessage.ModifyAsync($"{Formatter.Strike(discordMessage.Content)}\n{Formatter.Bold("[Notice: Fixing role permissions...]")}");
					await FixVoiceBanPermissions(context.Guild, voiceBanRole);
					_ = await discordMessage.ModifyAsync($"{voiceBanRole.Mention} is now set as the voiceban role.");
				}
				else if (eventArgs.Emoji == Queue.ThumbsDown)
				{
					_voiceBanLogger.Trace($"Fixed permissions for the voiceban role {previousVoiceBanRole.Name} ({previousVoiceBanRole.Id}) on {context.Guild.Name} ({context.Guild.Id})!");
					_ = await discordMessage.ModifyAsync($"{Formatter.Strike(discordMessage.Content)}\n{Formatter.Bold("[Notice: Fixing role permissions...]")}");
					await FixVoiceBanPermissions(context.Guild, voiceBanRole);
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
				await CreateVoiceBanRole(context, await Program.SendMessage(context, "Creating voiceban role..."));
				return;
			}

			DiscordMessage discordMessage = await Program.SendMessage(context, $"Previous voiceban role was {previousVoiceBanRole.Mention}. Do you want to overwrite it?");
			_ = new Queue(discordMessage, context.User, new(async eventArgs =>
			{
				if (eventArgs.Emoji == Queue.ThumbsUp) await CreateVoiceBanRole(context, discordMessage);
				else if (eventArgs.Emoji == Queue.ThumbsDown)
				{
					_voiceBanLogger.Trace($"Fixed permissions for the voiceban role {previousVoiceBanRole.Name} ({previousVoiceBanRole.Id}) on {context.Guild.Name} ({context.Guild.Id})!");
					await FixVoiceBanPermissions(context.Guild, previousVoiceBanRole);
					_ = await Program.SendMessage(context, $"Roles were left untouched.");
				}
			}));
		}

		public static async Task CreateVoiceBanRole(CommandContext context, DiscordMessage message)
		{
			_ = await message.ModifyAsync("Creating voiceban role...");
			DiscordRole voicebanRole = await context.Guild.CreateRoleAsync("Voiceban", Permissions.None, DiscordColor.Gray, false, false, "Allows users to be voicebanned.");
			_voiceBanLogger.Trace($"Created voiceban role \"{voicebanRole.Name}\" ({voicebanRole.Id}) for {context.Guild.Name} ({context.Guild.Id})!");
			Program.Database.Guild.VoiceBanRole(context.Guild.Id, voicebanRole.Id);
			_ = await message.ModifyAsync($"Overriding channel permissions...");
			await FixVoiceBanPermissions(context.Guild, voicebanRole);
			_ = await message.ModifyAsync($"Done! Voiceban role is now {voicebanRole.Mention}");
		}

		public static async Task FixVoiceBanPermissions(DiscordGuild guild, DiscordRole voicebanRole)
		{
			foreach (DiscordChannel channel in guild.Channels.Values)
			{
				_voiceBanLogger.Trace($"Overwriting permission {Permissions.UseVoice} for voiceban role {voicebanRole.Name} ({voicebanRole.Id}) on {channel.Type} channel {channel.Name} ({channel.Id}) for {guild.Name} ({guild.Id})...");
				switch (channel.Type)
				{
					case ChannelType.Voice:
						await channel.AddOverwriteAsync(voicebanRole, Permissions.None, Permissions.UseVoice, "Disallows users to connect to the voicechat.");
						break;
					case ChannelType.Category:
						await channel.AddOverwriteAsync(voicebanRole, Permissions.None, Permissions.UseVoice, "Disallows users to connect to the voicechannels.");
						break;
					case ChannelType.Text or ChannelType.News or ChannelType.Unknown:
					default: break;
				}
				await Task.Delay(50);
			}
		}

		public static async Task FixVoiceBanPermissions(DiscordChannel channel, DiscordRole voicebanRole)
		{
			switch (channel.Type)
			{
				case ChannelType.Voice:
					_voiceBanLogger.Trace($"Overwriting permission {Permissions.Speak} and {Permissions.Stream} for voiceban role {voicebanRole.Name} ({voicebanRole.Id}) on {channel.Type} channel {channel.Name} ({channel.Id})...");
					await channel.AddOverwriteAsync(voicebanRole, Permissions.None, Permissions.UseVoice, "Disallows users to connect to the voicechat.");
					break;
				case ChannelType.Category:
					_voiceBanLogger.Trace($"Overwriting permission {Permissions.SendMessages}, {Permissions.AddReactions}, {Permissions.Speak} and {Permissions.Stream} for voiceban role {voicebanRole.Name} ({voicebanRole.Id}) on {channel.Type} channel {channel.Name} ({channel.Id})...");
					await channel.AddOverwriteAsync(voicebanRole, Permissions.None, Permissions.UseVoice, "Disallows users to connect to the voicechannels.");
					break;
				case ChannelType.Text or ChannelType.News or ChannelType.Unknown:
				default: break;
			}
		}

		#endregion VoiceBanCommand
	}
}
