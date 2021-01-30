using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

using Tomoe.Types;
using Tomoe.Utils;
using System;

namespace Tomoe.Commands.Moderation
{
	[Group("config")]
	public class Config : BaseCommandModule
	{
		private static readonly Logger _muteLogger = new("Commands.Config.Mute");
		private static readonly Logger _antimemeLogger = new("Commands.Config.Antimeme");
		private static readonly Logger _vcBanLogger = new("Commands.Config.VoiceBan");

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
						await Task.Delay(50);
						break;
					case ChannelType.Voice:
						_muteLogger.Trace($"Overwriting permission {Permissions.Speak} and {Permissions.Stream} for mute role {muteRole.Name} ({muteRole.Id}) on {channel.Type} channel {channel.Name} ({channel.Id}) for {guild.Name} ({guild.Id})...");
						await channel.AddOverwriteAsync(muteRole, Permissions.None, Permissions.Speak | Permissions.Stream, "Disallows users to communicate in voice channels and through streams.");
						await Task.Delay(50);
						break;
					case ChannelType.Category:
						_muteLogger.Trace($"Overwriting permission {Permissions.SendMessages}, {Permissions.AddReactions}, {Permissions.Speak} and {Permissions.Stream} for mute role {muteRole.Name} ({muteRole.Id}) on {channel.Type} channel {channel.Name} ({channel.Id}) for {guild.Name} ({guild.Id})...");
						await channel.AddOverwriteAsync(muteRole, Permissions.None, Permissions.SendMessages | Permissions.AddReactions | Permissions.Speak | Permissions.Stream, "Disallows users to send messages/communicate through reactions/voice channels and through streams.");
						await Task.Delay(50);
						break;
					default: break;
				}
			}
		}

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
			DiscordRole previousMuteRole = Program.Database.Guild.AntimemeRole(context.Guild.Id).GetRole(context.Guild);
			if (previousMuteRole == null) // Should only be executed if there was no previous mute role id, or if the role cannot be found.
			{
				await CreateAntiMemeRole(context, await Program.SendMessage(context, "Creating antimeme role..."));
				return;
			}

			DiscordMessage discordMessage = await Program.SendMessage(context, $"Previous antimeme role was {previousMuteRole.Mention}. Do you want to overwrite it?");
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

		public static async Task CreateAntiMemeRole(CommandContext context, DiscordMessage message)
		{
			await message.ModifyAsync("Creating antimeme role...");
			DiscordRole muteRole = await context.Guild.CreateRoleAsync("Antimeme", Permissions.None, DiscordColor.Gray, false, false, "Allows users to be no memed.");
			_antimemeLogger.Trace($"Created antimeme role \"{muteRole.Name}\" ({muteRole.Id}) for {context.Guild.Name} ({context.Guild.Id})!");
			Program.Database.Guild.AntimemeRole(context.Guild.Id, muteRole.Id);
			_ = await message.ModifyAsync($"Overriding channel permissions...");
			await FixAntimemeRolePermissions(context.Guild, muteRole);
			_ = await message.ModifyAsync($"Done! Antimeme role is now {muteRole.Mention}");
		}

		public static async Task FixAntimemeRolePermissions(DiscordGuild guild, DiscordRole muteRole)
		{
			foreach (DiscordChannel channel in guild.Channels.Values)
			{
				switch (channel.Type)
				{
					case ChannelType.Text or ChannelType.News or ChannelType.Unknown:
						_antimemeLogger.Trace($"Overwriting permission {Permissions.SendMessages} and {Permissions.AddReactions} for antimeme role {muteRole.Name} ({muteRole.Id}) on {channel.Type} channel {channel.Name} ({channel.Id}) for {guild.Name} ({guild.Id})...");
						await channel.AddOverwriteAsync(muteRole, Permissions.None, Permissions.AttachFiles | Permissions.AddReactions | Permissions.EmbedLinks | Permissions.UseExternalEmojis, "Stops members with the role from spamming chat with reactions, images or links.");
						await Task.Delay(50);
						break;
					case ChannelType.Voice:
						_antimemeLogger.Trace($"Overwriting permission {Permissions.Speak} and {Permissions.Stream} for antimeme role {muteRole.Name} ({muteRole.Id}) on {channel.Type} channel {channel.Name} ({channel.Id}) for {guild.Name} ({guild.Id})...");
						await channel.AddOverwriteAsync(muteRole, Permissions.None, Permissions.Stream | Permissions.UseVoiceDetection, "Disallows users to spam starting and stopping streams. Must push to talk.");
						await Task.Delay(50);
						break;
					case ChannelType.Category:
						_antimemeLogger.Trace($"Overwriting permission {Permissions.SendMessages}, {Permissions.AddReactions}, {Permissions.Speak} and {Permissions.Stream} for antimeme role {muteRole.Name} ({muteRole.Id}) on {channel.Type} channel {channel.Name} ({channel.Id}) for {guild.Name} ({guild.Id})...");
						await channel.AddOverwriteAsync(muteRole, Permissions.None, Permissions.AttachFiles | Permissions.AddReactions | Permissions.EmbedLinks | Permissions.UseExternalEmojis | Permissions.Stream | Permissions.UseVoiceDetection, "Stops members with the role from spamming chat with reactions, images or links/voice channels and through streams. Disallows users to spam starting and stopping streams. Must push to talk.");
						await Task.Delay(50);
						break;
					default: break;
				}
			}
		}
	}
}
