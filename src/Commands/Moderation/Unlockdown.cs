using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using Microsoft.Extensions.DependencyInjection;
using Tomoe.Db;

namespace Tomoe.Commands.Moderation
{
	[Group("unlock"), Description("Unlocks a channel, role or the entire server."), Aliases("unlock_down", "unlockdown"), RequireGuild]
	public class Unlockdown : BaseCommandModule
	{

		public Database Database { private get; set; }

		public static async Task UnlockChannel(CommandContext context, DiscordChannel channel, List<DiscordRole> roles = null)
		{
			using IServiceScope scope = Program.ServiceProvider.CreateScope();
			Database database = scope.ServiceProvider.GetService<Database>();

			if (roles == null || roles.Count == 0)
			{
				roles = new();
				roles.AddRange(context.Guild.Roles.Values);
			}

			List<ulong> roleIds = roles.Select(role => role.Id).ToList();
			List<Lock> locks = database.Locks.Where(dbLock => dbLock.ChannelId == channel.Id && dbLock.GuildId == context.Guild.Id && roleIds.Contains(dbLock.RoleId)).ToList();
			foreach (Lock dbLock in locks)
			{
				if (dbLock.HadPreviousOverwrite)
				{
					await channel.AddOverwriteAsync(dbLock.RoleId.GetRole(context.Guild), dbLock.Allowed, dbLock.Denied, "Unlocking channel to previous overwrites.");
				}
				else
				{
					DiscordOverwrite overwrite = channel.PermissionOverwrites.FirstOrDefault(overwrite => overwrite.Id == dbLock.RoleId);
					if (overwrite != null)
					{
						await overwrite.DeleteAsync("Unlocking channel to previous overwrites");
					}
				}
			}
			database.Locks.RemoveRange(locks);
			_ = await database.SaveChangesAsync();
		}

		[Command("channel")]
		public async Task Channel(CommandContext context, DiscordChannel channel, [RemainingText] string unlockReason = Constants.MissingReason)
		{
			await UnlockChannel(context, channel);
			await ModLogs.Record(context.Guild.Id, "Channel Unlock", $"{context.User.Mention} unlocked channel {channel.Mention}. Reason: {unlockReason}");
			_ = await Program.SendMessage(context, $"Channel successfully unlocked. Permissions were restored to what they were before.");
		}

		[Command("channel")]
		public async Task Channel(CommandContext context, [RemainingText] string unlockReason = Constants.MissingReason) => await Channel(context, context.Channel, unlockReason);

		[Command("server")]
		public async Task Server(CommandContext context, [RemainingText] string unlockReason = Constants.MissingReason)
		{
			foreach (DiscordChannel channel in context.Guild.Channels.Values.OrderBy(channel => channel.Type))
			{
				await UnlockChannel(context, channel);
			}
			await ModLogs.Record(context.Guild.Id, "Server Unlock", $"{context.User.Mention} unlocked the server. Reason: {unlockReason}");
			_ = await Program.SendMessage(context, $"Server successfully unlocked. Permissions were restored to what they were before.");
		}

		[Command("role")]
		public async Task Role(CommandContext context, DiscordRole role, DiscordChannel channel = null, [RemainingText] string unlockReason = Constants.MissingReason)
		{
			if (channel == null)
			{
				foreach (DiscordChannel guildChannel in context.Guild.Channels.Values.OrderBy(channel => channel.Type))
				{
					await UnlockChannel(context, guildChannel, new() { role });
				}
				await ModLogs.Record(context.Guild.Id, "Role Server Unlock", $"{context.User.Mention} unlocked the role {role.Mention}. Reason: {unlockReason}");
				_ = await Program.SendMessage(context, $"{role.Mention} is now unlocked. Permissions were restored to what they were before.");
			}
			else
			{
				await UnlockChannel(context, channel, new() { role });
				await ModLogs.Record(context.Guild.Id, "Role Channel Unlock", $"{context.User.Mention} unlocked the server. Reason: {unlockReason}");
				_ = await Program.SendMessage(context, $"Role successfully unlocked in channel {channel.Mention}. Permissions were restored to what they were before.");
			}
		}

		[Command("bots")]
		public async Task Bots(CommandContext context, DiscordChannel channel = null, [RemainingText] string unlockReason = Constants.MissingReason)
		{
			List<DiscordRole> roles = context.Guild.Roles.Values.Where(role => role.IsManaged).ToList();
			if (channel == null)
			{
				foreach (DiscordRole role in roles)
				{
					foreach (DiscordChannel guildChannel in context.Guild.Channels.Values.OrderBy(channel => channel.Type))
					{
						await UnlockChannel(context, guildChannel, new() { role });
					}
					await ModLogs.Record(context.Guild.Id, "Bots Server Unlock", $"{context.User.Mention} unlocked the bots from the server. Reason: {unlockReason}");
					_ = await Program.SendMessage(context, $"The bots can now send messages or react in the server. Permissions were restored to what they were before.");
				}
			}
			else
			{
				foreach (DiscordRole role in roles)
				{
					await UnlockChannel(context, channel, new() { role });
				}
				await ModLogs.Record(context.Guild.Id, "Bot Channel Unlock", $"{context.User.Mention} unlocked bots from the channel {channel.Mention}. Reason: {unlockReason}");
				_ = await Program.SendMessage(context, $"Bots are successfully unlocked in channel {channel.Mention}. Permissions were restored to what they were before.");
			}
		}
	}
}
