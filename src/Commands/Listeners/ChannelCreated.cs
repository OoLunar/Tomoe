using System.Threading.Tasks;
using System.Linq;

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Tomoe.Db;
using Microsoft.EntityFrameworkCore;

namespace Tomoe.Commands.Listeners
{
	public class ChannelCreated
	{
		public static async Task Handler(DiscordClient client, ChannelCreateEventArgs eventArgs)
		{
			Guild guild = await Program.Database.Guilds.FirstAsync(guild => guild.Id == eventArgs.Guild.Id);
			if (guild != null)
			{
				DiscordRole muteRole = guild.MuteRole.GetRole(eventArgs.Guild);
				if (muteRole != null) await Moderation.Config.FixPermissions(eventArgs.Channel, muteRole, Moderation.Config.PermissionType.Mute);

				DiscordRole antimemeRole = guild.AntimemeRole.GetRole(eventArgs.Guild);
				if (antimemeRole != null) await Moderation.Config.FixPermissions(eventArgs.Channel, antimemeRole, Moderation.Config.PermissionType.Antimeme);

				DiscordRole voiceBanRole = guild.VoiceBanRole.GetRole(eventArgs.Guild);
				if (voiceBanRole != null) await Moderation.Config.FixPermissions(eventArgs.Channel, voiceBanRole, Moderation.Config.PermissionType.VoiceBan);
			}
			else
			{
				guild = new(eventArgs.Guild.Id);
				_ = await Program.Database.Guilds.AddAsync(guild);
				await Handler(client, eventArgs);
			}
		}
	}
}
