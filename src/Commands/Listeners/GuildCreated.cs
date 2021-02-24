using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

using Tomoe.Db;

namespace Tomoe.Commands.Listeners
{
	public class GuildCreated
	{
		public static async Task Handler(DiscordClient _client, GuildCreateEventArgs eventArgs)
		{
			Guild guild = await Program.Database.Guilds.FirstOrDefaultAsync(guild => guild.Id == eventArgs.Guild.Id);
			if (guild != null)
			{
				DiscordRole muteRole = guild.MuteRole.GetRole(eventArgs.Guild);
				DiscordRole antimemeRole = guild.AntimemeRole.GetRole(eventArgs.Guild);
				DiscordRole voiceBanRole = guild.VoiceBanRole.GetRole(eventArgs.Guild);
				if (muteRole != null) await Moderation.Config.FixPermissions(eventArgs.Guild, muteRole, Moderation.Config.PermissionType.Mute);
				if (antimemeRole != null) await Moderation.Config.FixPermissions(eventArgs.Guild, antimemeRole, Moderation.Config.PermissionType.Antimeme);
				if (voiceBanRole != null) await Moderation.Config.FixPermissions(eventArgs.Guild, voiceBanRole, Moderation.Config.PermissionType.VoiceBan);
			}
			else
			{
				guild = new(eventArgs.Guild.Id);
				_ = await Program.Database.SaveChangesAsync();
			}
		}
	}
}
