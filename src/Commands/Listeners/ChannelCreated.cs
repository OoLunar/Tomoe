using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

using Tomoe.Db;

namespace Tomoe.Commands.Listeners
{
	public class ChannelCreated
	{
		public static async Task Handler(DiscordClient _client, ChannelCreateEventArgs eventArgs)
		{
			Guild guild = await Program.Database.Guilds.FirstOrDefaultAsync(guild => guild.Id == eventArgs.Guild.Id);
			if (guild != null)
			{
				DiscordRole muteRole = guild.MuteRole.GetRole(eventArgs.Guild);
				if (muteRole != null) await Moderation.Config.FixPermissions(eventArgs.Channel, muteRole, Moderation.Config.PermissionType.Mute);

				DiscordRole antimemeRole = guild.AntimemeRole.GetRole(eventArgs.Guild);
				if (antimemeRole != null) await Moderation.Config.FixPermissions(eventArgs.Channel, antimemeRole, Moderation.Config.PermissionType.Antimeme);

				DiscordRole voiceBanRole = guild.VoiceBanRole.GetRole(eventArgs.Guild);
				if (voiceBanRole != null) await Moderation.Config.FixPermissions(eventArgs.Channel, voiceBanRole, Moderation.Config.PermissionType.VoiceBan);
			}
		}
	}
}
