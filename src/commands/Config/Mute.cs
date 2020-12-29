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
		private static readonly Logger Logger = new Logger("Config/Mute");

		[Command("mute"), Description("Sets up or assigns the mute role."), RequireUserPermissions(Permissions.ManageGuild), RequireGuild]
		public async Task SetupMute(CommandContext context, DiscordRole muteRole)
		{
			ulong? previousMuteRoleId = Program.Database.Guild.MuteRole(context.Guild.Id);
			if (previousMuteRoleId.HasValue && previousMuteRoleId.Value != muteRole.Id)
			{
				DiscordRole previousMuteRole = context.Guild.GetRole(previousMuteRoleId.Value);
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
						Logger.Trace($"Set {muteRole.Name} ({muteRole.Id}) as mute role for {context.Guild.Name} ({context.Guild.Id})!");
						await FixMuteRolePermissions(context.Guild, muteRole);
						_ = Program.SendMessage(context, $"{muteRole.Mention} is now set as the mute role.");
					}
					else if (emoji == thumbsDown)
					{
						Logger.Trace($"Set previous mute role {previousMuteRole.Name} ({previousMuteRole.Id}) as mute role for {context.Guild.Name} ({context.Guild.Id})!");
						await FixMuteRolePermissions(context.Guild, previousMuteRole);
						_ = Program.SendMessage(context, $"{previousMuteRole.Mention} is now set as the mute role.");
					}
				});
				DiscordMessage discordMessage = Program.SendMessage(context, $"Previous mute role was {previousMuteRole.Mention}. Do you want to overwrite it with {muteRole.Mention}?");
				await discordMessage.CreateReactionAsync(thumbsUp);
				await discordMessage.CreateReactionAsync(thumbsDown);
				createNewRole.MessageId = discordMessage.Id;
				ReactionAdded.QueueList.Add(createNewRole);
			}
			else
			{
				Program.Database.Guild.MuteRole(context.Guild.Id, muteRole.Id);
				DiscordMessage discordMessage = Program.SendMessage(context, $"Setting role permissions for {muteRole.Mention}...");
				await FixMuteRolePermissions(context.Guild, muteRole);
				_ = discordMessage.ModifyAsync($"{context.User.Mention}: {muteRole.Mention} is now set as the mute role.", null, new List<IMention>() { new UserMention(context.User.Id) });
			}
		}

		[Command("mute"), RequireUserPermissions(Permissions.ManageGuild), RequireBotPermissions(Permissions.ManageRoles), RequireGuild]
		public async Task SetupMute(CommandContext context)
		{
			ulong? previousMuteRoleId = Program.Database.Guild.MuteRole(context.Guild.Id);
			if (previousMuteRoleId.HasValue)
			{
				DiscordRole previousMuteRole = context.Guild.GetRole(previousMuteRoleId.Value);
				if (previousMuteRole == null)
				{
					await CreateMuteRole(context);
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
			else
			{
				await CreateMuteRole(context);
			}
		}

		public static async Task CreateMuteRole(CommandContext context)
		{
			DiscordMessage message = Program.SendMessage(context, "Creating mute role...");
			DiscordRole muteRole = await context.Guild.CreateRoleAsync("Muted", Permissions.None, DiscordColor.Gray, false, false, "Allows users to be muted.");
			Logger.Trace($"Created mute role '{muteRole.Name}' ({muteRole.Id}) for {context.Guild.Name} ({context.Guild.Id})!");
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
						Logger.Trace($"Overwriting permission {Permissions.SendMessages} and {Permissions.AddReactions} for mute role {muteRole.Name} ({muteRole.Id}) on {channel.Type} channel {channel.Name} ({channel.Id}) for {guild.Name} ({guild.Id})...");
						_ = channel.AddOverwriteAsync(muteRole, Permissions.None, Permissions.SendMessages | Permissions.AddReactions, "Disallows users to send messages/communicate through reactions.").ConfigureAwait(false).GetAwaiter();
						await Task.Delay(50);
						break;
					case ChannelType.Voice:
						Logger.Trace($"Overwriting permission {Permissions.Speak} and {Permissions.Stream} for mute role {muteRole.Name} ({muteRole.Id}) on {channel.Type} channel {channel.Name} ({channel.Id}) for {guild.Name} ({guild.Id})...");
						_ = channel.AddOverwriteAsync(muteRole, Permissions.None, Permissions.Speak | Permissions.Stream, "Disallows users to communicate in voice channels and through streams.");
						await Task.Delay(50);
						break;
					case ChannelType.Category:
						Logger.Trace($"Overwriting permission {Permissions.SendMessages}, {Permissions.AddReactions}, {Permissions.Speak} and {Permissions.Stream} for mute role {muteRole.Name} ({muteRole.Id}) on {channel.Type} channel {channel.Name} ({channel.Id}) for {guild.Name} ({guild.Id})...");
						_ = channel.AddOverwriteAsync(muteRole, Permissions.None, Permissions.SendMessages | Permissions.AddReactions | Permissions.Speak | Permissions.Stream, "Disallows users to send messages/communicate through reactions/voice channels and through streams.");
						await Task.Delay(50);
						break;
				}
			}
		}
	}
}
