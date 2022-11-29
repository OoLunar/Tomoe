using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;

namespace Tomoe.Commands.Moderation
{
	public sealed class AnnounceCommand : BaseCommandModule
	{
		[Command("announce"), RequireOwner]
		public async Task AnnounceAsync(CommandContext context, [RemainingText] string message)
		{
			DiscordUser botOwner = context.Client.CurrentApplication.Owners.First();
			foreach (DiscordGuild guild in context.Client.Guilds.Values)
			{
				try
				{
					await guild.GetMemberAsync(botOwner.Id);
					continue; // Don't leave mutual servers of the bot owner.
				}
				catch (DiscordException) { }

				DiscordChannel channel = guild.PublicUpdatesChannel;
				if (channel is null || !channel.PermissionsFor(context.Guild.CurrentMember).HasPermission(Permissions.SendMessages))
				{
					channel = await guild.Owner.CreateDmChannelAsync();
				}

				try
				{
					await channel.SendMessageAsync(message);
				}
				catch (DiscordException) { }
				await guild.LeaveAsync();
			}
		}
	}
}
