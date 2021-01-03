using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Tomoe.Types;
using Tomoe.Utils;

namespace Tomoe.Commands
{
	[Group("config")]
	public class Config : BaseCommandModule
	{
		private static readonly Logger _muteLogger = new("Commands.Config.Mute");
		private static readonly Logger _noMemeLogger = new("Commands.Config.NoMeme");

		[Command("mute"), Description("Sets up or assigns the mute role."), RequireUserPermissions(Permissions.ManageGuild), RequireGuild]
		public async Task Mute(CommandContext context, DiscordRole muteRole)
		{
			ulong? previousMuteRoleId = Program.Database.Guild.MuteRole(context.Guild.Id);
			if (previousMuteRoleId.HasValue && previousMuteRoleId.Value != muteRole.Id)
			{
				DiscordRole previousMuteRole = context.Guild.GetRole(previousMuteRoleId.Value);
				if (previousMuteRole != null)
				{
					DiscordMessage discordMessage = Program.SendMessage(context, $"Previous mute role was <@&{previousMuteRoleId.Value}>. Do you want to overwrite it with {muteRole.Mention}?");
					_ = new Queue(discordMessage, context.User, new(async eventArgs =>
					{
						if (eventArgs.Emoji == Queue.ThumbsUp)
						{
							Program.Database.Guild.MuteRole(context.Guild.Id, muteRole.Id);
							_muteLogger.Trace($"Set {muteRole.Name} ({muteRole.Id}) as mute role for {context.Guild.Name} ({context.Guild.Id})!");
							await FixMuteRolePermissions(context.Guild, muteRole);
							_ = Program.SendMessage(context, $"{muteRole.Mention} is now set as the mute role.");
						}
						else if (eventArgs.Emoji == Queue.ThumbsDown)
						{
							_muteLogger.Trace($"Fixed permissions for the mute role {previousMuteRole.Name} ({previousMuteRole.Id}) on {context.Guild.Name} ({context.Guild.Id})!");
							await FixMuteRolePermissions(context.Guild, previousMuteRole);
							_ = Program.SendMessage(context, $"Roles were left untouched.");
						}
					}));
					return;
				}
			}
			Program.Database.Guild.MuteRole(context.Guild.Id, muteRole.Id);
			_muteLogger.Trace($"Set {muteRole.Name} ({muteRole.Id}) as mute role for {context.Guild.Name} ({context.Guild.Id})!");
			await FixMuteRolePermissions(context.Guild, muteRole);
			_ = Program.SendMessage(context, $"{muteRole.Mention} is now set as the mute role.");
		}

		[Command("mute"), RequireUserPermissions(Permissions.ManageGuild), RequireBotPermissions(Permissions.ManageRoles), RequireGuild]
		public async Task Mute(CommandContext context)
		{
			ulong? previousMuteRoleId = Program.Database.Guild.MuteRole(context.Guild.Id);
			if (previousMuteRoleId.HasValue)
			{
				DiscordRole previousMuteRole = context.Guild.GetRole(previousMuteRoleId.Value);
				if (previousMuteRole != null)
				{
					DiscordMessage discordMessage = Program.SendMessage(context, $"Previous mute role was {previousMuteRole.Mention}. Do you want to overwrite it?");
					_ = new Queue(discordMessage, context.User, new(async eventArgs =>
					{
						if (eventArgs.Emoji == Queue.ThumbsUp) await CreateMuteRole(context);
						else if (eventArgs.Emoji == Queue.ThumbsDown)
						{
							_muteLogger.Trace($"Fixed permissions for the mute role {previousMuteRole.Name} ({previousMuteRole.Id}) on {context.Guild.Name} ({context.Guild.Id})!");
							await FixMuteRolePermissions(context.Guild, previousMuteRole);
							_ = Program.SendMessage(context, $"Roles were left untouched.");
						}
					}));
					return;
				}
			}
			// Should only be executed if there was no previous mute role id, or if the role cannot be found.
			await CreateMuteRole(context);
		}

		public static async Task CreateMuteRole(CommandContext context)
		{
			DiscordMessage message = Program.SendMessage(context, "Creating mute role...");
			DiscordRole muteRole = await context.Guild.CreateRoleAsync("Muted", Permissions.None, DiscordColor.Gray, false, false, "Allows users to be muted.");
			_muteLogger.Trace($"Created mute role \"{muteRole.Name}\" ({muteRole.Id}) for {context.Guild.Name} ({context.Guild.Id})!");
			Program.Database.Guild.MuteRole(context.Guild.Id, muteRole.Id);
			_ = message.ModifyAsync($"{context.User.Mention}: Overriding channel permissions...", null, new List<IMention>() { new UserMention(context.User.Id) });
			await FixMuteRolePermissions(context.Guild, muteRole);
			_ = await message.ModifyAsync($"{context.User.Mention}: Done! Mute role is now {muteRole.Mention}", null, new List<IMention>() { new UserMention(context.User.Id) });
		}



		public static async Task FixMuteRolePermissions(DiscordGuild guild, DiscordRole muteRole)
		{
			foreach (DiscordChannel channel in guild.Channels.Values)
			{
				switch (channel.Type)
				{
					case ChannelType.Text:
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

		[Command("antimeme"), Description("Sets up or assigns the antimeme role."), RequireUserPermissions(Permissions.ManageGuild), RequireGuild, Aliases(new[] { "anti_meme", "nomeme", "no_meme", "memeban", "meme_ban" })]
		public async Task AntiMeme(CommandContext context, DiscordRole antiMemeRole)
		{
			ulong? previousAntiMemeRoleId = Program.Database.Guild.AntiMemeRole(context.Guild.Id);
			if (previousAntiMemeRoleId.HasValue && previousAntiMemeRoleId.Value != antiMemeRole.Id)
			{
				DiscordRole previousAntiMemeRole = context.Guild.GetRole(previousAntiMemeRoleId.Value);
				if (previousAntiMemeRole != null)
				{
					DiscordMessage discordMessage = Program.SendMessage(context, $"Previous antimeme role was {previousAntiMemeRole.Mention}. Do you want to overwrite it with {antiMemeRole.Mention}?");
					_ = new Queue(discordMessage, context.User, new(async eventArgs =>
					{
						if (eventArgs.Emoji == Queue.ThumbsUp)
						{
							Program.Database.Guild.AntiMemeRole(context.Guild.Id, antiMemeRole.Id);
							_noMemeLogger.Trace($"Set {antiMemeRole.Name} ({antiMemeRole.Id}) as the antimeme role for {context.Guild.Name} ({context.Guild.Id})!");
							await FixAntiMemeRolePermissions(context.Guild, antiMemeRole);
							_ = Program.SendMessage(context, $"{antiMemeRole.Mention} is now set as the antimeme role.");
						}
						else if (eventArgs.Emoji == Queue.ThumbsDown)
						{
							_noMemeLogger.Trace($"Fixed permissions for the antimeme role {previousAntiMemeRole.Name} ({previousAntiMemeRole.Id}) on {context.Guild.Name} ({context.Guild.Id})!");
							await FixAntiMemeRolePermissions(context.Guild, previousAntiMemeRole);
							_ = Program.SendMessage(context, $"Roles were left untouched.");
						}
					}));
					return;
				}
			}
			Program.Database.Guild.AntiMemeRole(context.Guild.Id, antiMemeRole.Id);
			_noMemeLogger.Trace($"Set {antiMemeRole.Name} ({antiMemeRole.Id}) as the antimeme role for {context.Guild.Name} ({context.Guild.Id})!");
			await FixAntiMemeRolePermissions(context.Guild, antiMemeRole);
			_ = Program.SendMessage(context, $"{antiMemeRole.Mention} is now set as the antimeme role.");
		}

		[Command("antimeme"), RequireUserPermissions(Permissions.ManageGuild), RequireBotPermissions(Permissions.ManageRoles), RequireGuild]
		public async Task AntiMeme(CommandContext context)
		{
			ulong? previousAntiMemeRoleId = Program.Database.Guild.AntiMemeRole(context.Guild.Id);
			if (previousAntiMemeRoleId.HasValue)
			{
				DiscordRole previousAntiMemeRole = context.Guild.GetRole(previousAntiMemeRoleId.Value);
				if (previousAntiMemeRole != null)
				{
					DiscordMessage discordMessage = Program.SendMessage(context, $"Previous antimeme role was {previousAntiMemeRole.Mention}. Do you want to overwrite it?");
					_ = new Queue(discordMessage, context.User, new(async eventArgs =>
					{
						if (eventArgs.Emoji == Queue.ThumbsUp)
						{
							await CreateAntiMemeRole(context);
						}
						else if (eventArgs.Emoji == Queue.ThumbsDown)
						{
							_noMemeLogger.Trace($"Fixed permissions for the antimeme role {previousAntiMemeRole.Name} ({previousAntiMemeRole.Id}) on {context.Guild.Name} ({context.Guild.Id})!");
							await FixAntiMemeRolePermissions(context.Guild, previousAntiMemeRole);
							_ = Program.SendMessage(context, $"Roles were left untouched.");
						}
					}));
					return;
				}
			}
			// Should only be executed if there was no previous mute role id, or if the role cannot be found.
			await CreateAntiMemeRole(context);
		}

		public static async Task CreateAntiMemeRole(CommandContext context)
		{
			DiscordMessage message = Program.SendMessage(context, "Creating antimeme role...");
			DiscordRole muteRole = await context.Guild.CreateRoleAsync("Antimeme", Permissions.None, DiscordColor.Gray, false, false, "Allows users to be no memed.");
			_noMemeLogger.Trace($"Created antimeme role \"{muteRole.Name}\" ({muteRole.Id}) for {context.Guild.Name} ({context.Guild.Id})!");
			Program.Database.Guild.AntiMemeRole(context.Guild.Id, muteRole.Id);
			_ = message.ModifyAsync($"{context.User.Mention}: Overriding channel permissions...", null, new List<IMention>() { new UserMention(context.User.Id) });
			await FixAntiMemeRolePermissions(context.Guild, muteRole);
			_ = await message.ModifyAsync($"{context.User.Mention}: Done! Antimeme role is now {muteRole.Mention}", null, new List<IMention>() { new UserMention(context.User.Id) });
		}

		public static async Task FixAntiMemeRolePermissions(DiscordGuild guild, DiscordRole muteRole)
		{
			foreach (DiscordChannel channel in guild.Channels.Values)
			{
				switch (channel.Type)
				{
					case ChannelType.Text:
						_noMemeLogger.Trace($"Overwriting permission {Permissions.SendMessages} and {Permissions.AddReactions} for antimeme role {muteRole.Name} ({muteRole.Id}) on {channel.Type} channel {channel.Name} ({channel.Id}) for {guild.Name} ({guild.Id})...");
						await channel.AddOverwriteAsync(muteRole, Permissions.None, Permissions.AttachFiles | Permissions.AddReactions | Permissions.EmbedLinks | Permissions.UseExternalEmojis, "Stops members with the role from spamming chat with reactions, images or links.");
						await Task.Delay(50);
						break;
					case ChannelType.Voice:
						_noMemeLogger.Trace($"Overwriting permission {Permissions.Speak} and {Permissions.Stream} for antimeme role {muteRole.Name} ({muteRole.Id}) on {channel.Type} channel {channel.Name} ({channel.Id}) for {guild.Name} ({guild.Id})...");
						await channel.AddOverwriteAsync(muteRole, Permissions.None, Permissions.Stream | Permissions.UseVoiceDetection, "Disallows users to spam starting and stopping streams. Must push to talk.");
						await Task.Delay(50);
						break;
					case ChannelType.Category:
						_noMemeLogger.Trace($"Overwriting permission {Permissions.SendMessages}, {Permissions.AddReactions}, {Permissions.Speak} and {Permissions.Stream} for antimeme role {muteRole.Name} ({muteRole.Id}) on {channel.Type} channel {channel.Name} ({channel.Id}) for {guild.Name} ({guild.Id})...");
						await channel.AddOverwriteAsync(muteRole, Permissions.None, Permissions.AttachFiles | Permissions.AddReactions | Permissions.EmbedLinks | Permissions.UseExternalEmojis | Permissions.Stream | Permissions.UseVoiceDetection, "Stops members with the role from spamming chat with reactions, images or links/voice channels and through streams. Disallows users to spam starting and stopping streams. Must push to talk.");
						await Task.Delay(50);
						break;
					default: break;
				}
			}
		}
	}
}
