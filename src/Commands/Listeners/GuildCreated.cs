using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace Tomoe.Commands.Listeners
{
	public class GuildCreated
	{
		public static async Task Handler(DiscordClient _client, GuildCreateEventArgs eventArgs)
		{
			if (Program.Database.Guild.GuildIdExists(eventArgs.Guild.Id))
			{
				DiscordRole muteRole = Program.Database.Guild.MuteRole(eventArgs.Guild.Id).GetRole(eventArgs.Guild);
				DiscordRole antimemeRole = Program.Database.Guild.AntimemeRole(eventArgs.Guild.Id).GetRole(eventArgs.Guild);
				DiscordRole voiceBanRole = Program.Database.Guild.VoiceBanRole(eventArgs.Guild.Id).GetRole(eventArgs.Guild);
				if (muteRole != null) await Moderation.Config.FixMuteRolePermissions(eventArgs.Guild, muteRole);
				if (antimemeRole != null) await Moderation.Config.FixAntimemeRolePermissions(eventArgs.Guild, antimemeRole);
				if (voiceBanRole != null) await Moderation.Config.FixVoiceBanPermissions(eventArgs.Guild, voiceBanRole);
			}
			else Program.Database.Guild.InsertGuildId(eventArgs.Guild.Id);
		}
	}
}
