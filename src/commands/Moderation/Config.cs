using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Tomoe.Commands.Listeners;
using Tomoe.Utils;

namespace Tomoe.Commands.Config
{
	[Group("config")]
	public class Mute : BaseCommandModule
	{
		private static readonly Logger MuteLogger = new("Commands.Config.Mute");
		private static readonly Logger NoMemeLogger = new("Commands.Config.NoMeme");

		[Command("mute"), Description("Sets up or assigns the mute role."), RequireUserPermissions(Permissions.ManageGuild), RequireGuild]
		public async Task SetupMute(CommandContext context, DiscordRole muteRole)
		{
			ulong? previousMuteRoleId = Program.Database.Guild.MuteRole(context.Guild.Id);
			DiscordRole previousMuteRole;
			if (previousMuteRoleId.HasValue && previousMuteRoleId.Value != muteRole.Id)
			{
				previousMuteRole = context.Guild.GetRole(previousMuteRoleId.Value);
				if (previousMuteRole != null)
				{
					DiscordEmoji thumbsUp = DiscordEmoji.FromUnicode(context.Client, "👍");
					DiscordEmoji thumbsDown = DiscordEmoji.FromUnicode(context.Client, "👎");
					ReactionAdded.Queue createNewRole = new();
					createNewRole.User = context.User;
					createNewRole.Emojis = new DiscordEmoji[] { thumbsUp, thumbsDown };
					createNewRole.Action = new ReactionAdded.ReactionHandler(async emoji =>
					{
						if (emoji == thumbsUp)
						{
							Program.Database.Guild.MuteRole(context.Guild.Id, muteRole.Id);
							MuteLogger.Trace($"Set {muteRole.Name} ({muteRole.Id}) as mute role for {context.Guild.Name} ({context.Guild.Id})!");
							await FixMuteRolePermissions(context.Guild, muteRole);
							_ = Program.SendMessage(context, $"{muteRole.Mention} is now set as the mute role.");
						}
						else if (emoji == thumbsDown)
						{
							MuteLogger.Trace($"Set previous mute role {previousMuteRole.Name} ({previousMuteRole.Id}) as mute role for {context.Guild.Name} ({context.Guild.Id})!");
							await FixMuteRolePermissions(context.Guild, previousMuteRole);
							_ = Program.SendMessage(context, $"{previousMuteRole.Mention} is now set as the mute role.");
						}
					});
					DiscordMessage discordMessage = Program.SendMessage(context, $"Previous mute role was <{previousMuteRoleId.Value}>. Do you want to overwrite it with {muteRole.Mention}?");
					await discordMessage.CreateReactionAsync(thumbsUp);
					await discordMessage.CreateReactionAsync(thumbsDown);
					createNewRole.MessageId = discordMessage.Id;
					ReactionAdded.QueueList.Add(createNewRole);
					return;
				}
			}
			// Should only be executed if there was no previous mute role id, or if the role cannot be found.
			await CreateMuteRole(context);
		}

		[Command("mute"), RequireUserPermissions(Permissions.ManageGuild), RequireBotPermissions(Permissions.ManageRoles), RequireGuild]
		public async Task SetupMute(CommandContext context)
		{
			ulong? previousMuteRoleId = Program.Database.Guild.MuteRole(context.Guild.Id);
			if (previousMuteRoleId.HasValue)
			{
				DiscordRole previousMuteRole = context.Guild.GetRole(previousMuteRoleId.Value);
				if (previousMuteRole != null)
				{
					DiscordEmoji thumbsUp = DiscordEmoji.FromUnicode(context.Client, "👍");
					DiscordEmoji thumbsDown = DiscordEmoji.FromUnicode(context.Client, "👎");
					ReactionAdded.Queue createNewRole = new();
					createNewRole.User = context.User;
					createNewRole.Emojis = new DiscordEmoji[] { thumbsUp, thumbsDown };
					createNewRole.Action = new ReactionAdded.ReactionHandler(async emoji =>
					{
						if (emoji == thumbsUp)
						{
							await CreateMuteRole(context);
						}
						else if (emoji == thumbsDown)
						{
							_ = Program.SendMessage(context, $"Roles were left untouched.");
						}
					});
					DiscordMessage discordMessage = Program.SendMessage(context, $"Previous mute role was {previousMuteRole.Mention}. Do you want to overwrite it?");
					createNewRole.MessageId = discordMessage.Id;
					await discordMessage.CreateReactionAsync(thumbsUp);
					await discordMessage.CreateReactionAsync(thumbsDown);
					ReactionAdded.QueueList.Add(createNewRole);
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
			MuteLogger.Trace($"Created mute role \"{muteRole.Name}\" ({muteRole.Id}) for {context.Guild.Name} ({context.Guild.Id})!");
			_ = message.ModifyAsync($"{context.User.Mention}: Overriding channel permissions...", null, new List<IMention>() { new UserMention(context.User.Id) });
			await FixMuteRolePermissions(context.Guild, muteRole);
			Program.Database.Guild.MuteRole(context.Guild.Id, muteRole.Id);
			_ = await message.ModifyAsync($"{context.User.Mention}: Done! Mute role is now {muteRole.Mention}", null, new List<IMention>() { new UserMention(context.User.Id) });
		}

		public static async Task FixMuteRolePermissions(DiscordGuild guild, DiscordRole muteRole)
		{
			foreach (DiscordChannel channel in guild.Channels.Values)
			{
				switch (channel.Type)
				{
					case ChannelType.Text:
						MuteLogger.Trace($"Overwriting permission {Permissions.SendMessages} and {Permissions.AddReactions} for mute role {muteRole.Name} ({muteRole.Id}) on {channel.Type} channel {channel.Name} ({channel.Id}) for {guild.Name} ({guild.Id})...");
						_ = channel.AddOverwriteAsync(muteRole, Permissions.None, Permissions.SendMessages | Permissions.AddReactions, "Disallows users to send messages/communicate through reactions.").ConfigureAwait(false).GetAwaiter();
						await Task.Delay(50);
						break;
					case ChannelType.Voice:
						MuteLogger.Trace($"Overwriting permission {Permissions.Speak} and {Permissions.Stream} for mute role {muteRole.Name} ({muteRole.Id}) on {channel.Type} channel {channel.Name} ({channel.Id}) for {guild.Name} ({guild.Id})...");
						_ = channel.AddOverwriteAsync(muteRole, Permissions.None, Permissions.Speak | Permissions.Stream, "Disallows users to communicate in voice channels and through streams.");
						await Task.Delay(50);
						break;
					case ChannelType.Category:
						MuteLogger.Trace($"Overwriting permission {Permissions.SendMessages}, {Permissions.AddReactions}, {Permissions.Speak} and {Permissions.Stream} for mute role {muteRole.Name} ({muteRole.Id}) on {channel.Type} channel {channel.Name} ({channel.Id}) for {guild.Name} ({guild.Id})...");
						_ = channel.AddOverwriteAsync(muteRole, Permissions.None, Permissions.SendMessages | Permissions.AddReactions | Permissions.Speak | Permissions.Stream, "Disallows users to send messages/communicate through reactions/voice channels and through streams.");
						await Task.Delay(50);
						break;
				}
			}
		}

		[Command("nomeme"), Description("Sets up or assigns the no meme role."), RequireUserPermissions(Permissions.ManageGuild), RequireGuild, Aliases(new[] { "anti_meme", "antimeme", "memeban", "meme_ban", "no_meme" })]
		public async Task SetupNoMeme(CommandContext context, DiscordRole noMemeRole)
		{
			ulong? previousNoMemeRoleId = Program.Database.Guild.NoMemeRole(context.Guild.Id);
			DiscordRole previousNoMemeRole;
			if (previousNoMemeRoleId.HasValue && previousNoMemeRoleId.Value != noMemeRole.Id)
			{
				previousNoMemeRole = context.Guild.GetRole(previousNoMemeRoleId.Value);
				if (previousNoMemeRole != null) goto Prompt;
				else goto SetNoMemeRole;
			}
			else goto SetNoMemeRole;

			Prompt:
			{
				DiscordEmoji thumbsUp = DiscordEmoji.FromUnicode(context.Client, "👍");
				DiscordEmoji thumbsDown = DiscordEmoji.FromUnicode(context.Client, "👎");
				ReactionAdded.Queue createNewRole = new();
				createNewRole.User = context.User;
				createNewRole.Emojis = new DiscordEmoji[] { thumbsUp, thumbsDown };
				createNewRole.Action = new ReactionAdded.ReactionHandler(async emoji =>
				{
					if (emoji == thumbsUp)
					{
						Program.Database.Guild.NoMemeRole(context.Guild.Id, noMemeRole.Id);
						NoMemeLogger.Trace($"Set {noMemeRole.Name} ({noMemeRole.Id}) as the no meme role for {context.Guild.Name} ({context.Guild.Id})!");
						await FixNoMemeRolePermissions(context.Guild, noMemeRole);
						_ = Program.SendMessage(context, $"{noMemeRole.Mention} is now set as the no meme role.");
					}
					else if (emoji == thumbsDown)
					{
						NoMemeLogger.Trace($"Set previous no meme role {previousNoMemeRole.Name} ({previousNoMemeRole.Id}) as the no meme role for {context.Guild.Name} ({context.Guild.Id})!");
						await FixNoMemeRolePermissions(context.Guild, previousNoMemeRole);
						_ = Program.SendMessage(context, $"{previousNoMemeRole.Mention} is now set as the no meme role.");
					}
				});
				DiscordMessage discordMessage = Program.SendMessage(context, $"Previous no meme role was <{previousNoMemeRoleId.Value}>. Do you want to overwrite it with {noMemeRole.Mention}?");
				await discordMessage.CreateReactionAsync(thumbsUp);
				await discordMessage.CreateReactionAsync(thumbsDown);
				createNewRole.MessageId = discordMessage.Id;
				ReactionAdded.QueueList.Add(createNewRole);
			};

		SetNoMemeRole:
			{
				Program.Database.Guild.NoMemeRole(context.Guild.Id, noMemeRole.Id);
				DiscordMessage discordMessage = Program.SendMessage(context, $"Setting role permissions for {noMemeRole.Mention}...");
				await FixNoMemeRolePermissions(context.Guild, noMemeRole);
				_ = discordMessage.ModifyAsync($"{context.User.Mention}: {noMemeRole.Mention} is now set as the no meme role.", null, new List<IMention>() { new UserMention(context.User.Id) });
			}
		}

		[Command("nomeme"), RequireUserPermissions(Permissions.ManageGuild), RequireBotPermissions(Permissions.ManageRoles), RequireGuild]
		public async Task SetupNoMeme(CommandContext context)
		{
			ulong? previousNoMemeRoleId = Program.Database.Guild.NoMemeRole(context.Guild.Id);
			if (previousNoMemeRoleId.HasValue)
			{
				DiscordRole previousNoMemeRole = context.Guild.GetRole(previousNoMemeRoleId.Value);
				if (previousNoMemeRole == null)
				{
					await CreateNoMemeRole(context);
					return;
				}
				DiscordEmoji thumbsUp = DiscordEmoji.FromUnicode(context.Client, "👍");
				DiscordEmoji thumbsDown = DiscordEmoji.FromUnicode(context.Client, "👎");
				ReactionAdded.Queue createNewRole = new();
				createNewRole.User = context.User;
				createNewRole.Emojis = new DiscordEmoji[] { thumbsUp, thumbsDown };
				createNewRole.Action = new ReactionAdded.ReactionHandler(async emoji =>
				{
					if (emoji == thumbsUp)
					{
						await CreateNoMemeRole(context);
					}
					else if (emoji == thumbsDown)
					{
						_ = Program.SendMessage(context, $"Roles were left untouched.");
					}
				});
				DiscordMessage discordMessage = Program.SendMessage(context, $"Previous no meme role was {previousNoMemeRole.Mention}. Do you want to overwrite it?");
				createNewRole.MessageId = discordMessage.Id;
				await discordMessage.CreateReactionAsync(thumbsUp);
				await discordMessage.CreateReactionAsync(thumbsDown);
				ReactionAdded.QueueList.Add(createNewRole);
				return;
			}
			else
			{
				await CreateNoMemeRole(context);
			}
		}

		public static async Task CreateNoMemeRole(CommandContext context)
		{
			DiscordMessage message = Program.SendMessage(context, "Creating no meme role...");
			DiscordRole muteRole = await context.Guild.CreateRoleAsync("No Meme", Permissions.None, DiscordColor.Gray, false, false, "Allows users to be no memed.");
			Program.Database.Guild.NoMemeRole(context.Guild.Id, muteRole.Id);
			NoMemeLogger.Trace($"Created no meme role '{muteRole.Name}' ({muteRole.Id}) for {context.Guild.Name} ({context.Guild.Id})!");
			_ = message.ModifyAsync($"{context.User.Mention}: Overriding channel permissions...", null, new List<IMention>() { new UserMention(context.User.Id) });
			await FixNoMemeRolePermissions(context.Guild, muteRole);
			_ = await message.ModifyAsync($"{context.User.Mention}: Done! No meme role is now {muteRole.Mention}", null, new List<IMention>() { new UserMention(context.User.Id) });
		}

		public static async Task FixNoMemeRolePermissions(DiscordGuild guild, DiscordRole muteRole)
		{
			foreach (DiscordChannel channel in guild.Channels.Values)
			{
				switch (channel.Type)
				{
					case ChannelType.Text:
						NoMemeLogger.Trace($"Overwriting permission {Permissions.SendMessages} and {Permissions.AddReactions} for no meme role {muteRole.Name} ({muteRole.Id}) on {channel.Type} channel {channel.Name} ({channel.Id}) for {guild.Name} ({guild.Id})...");
						_ = channel.AddOverwriteAsync(muteRole, Permissions.None, Permissions.AttachFiles | Permissions.AddReactions | Permissions.EmbedLinks | Permissions.UseExternalEmojis, "Stops members with the role from spamming chat with reactions, images or links.").ConfigureAwait(false).GetAwaiter();
						await Task.Delay(50);
						break;
					case ChannelType.Voice:
						NoMemeLogger.Trace($"Overwriting permission {Permissions.Speak} and {Permissions.Stream} for no meme role {muteRole.Name} ({muteRole.Id}) on {channel.Type} channel {channel.Name} ({channel.Id}) for {guild.Name} ({guild.Id})...");
						_ = channel.AddOverwriteAsync(muteRole, Permissions.None, Permissions.Stream | Permissions.UseVoiceDetection, "Disallows users to spam starting and stopping streams. Must push to talk.");
						await Task.Delay(50);
						break;
					case ChannelType.Category:
						NoMemeLogger.Trace($"Overwriting permission {Permissions.SendMessages}, {Permissions.AddReactions}, {Permissions.Speak} and {Permissions.Stream} for no meme role {muteRole.Name} ({muteRole.Id}) on {channel.Type} channel {channel.Name} ({channel.Id}) for {guild.Name} ({guild.Id})...");
						_ = channel.AddOverwriteAsync(muteRole, Permissions.None, Permissions.AttachFiles | Permissions.AddReactions | Permissions.EmbedLinks | Permissions.UseExternalEmojis | Permissions.Stream | Permissions.UseVoiceDetection, "Stops members with the role from spamming chat with reactions, images or links/voice channels and through streams. Disallows users to spam starting and stopping streams. Must push to talk.");
						await Task.Delay(50);
						break;
				}
			}
		}
	}
}
