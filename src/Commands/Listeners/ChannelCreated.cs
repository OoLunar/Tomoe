using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace Tomoe.Commands.Listeners
{
	public class ChannelCreated
	{
		public static async Task Handler(DiscordClient client, ChannelCreateEventArgs eventArgs)
		{
			if (Program.Database.Guild.GuildIdExists(eventArgs.Guild.Id))
			{
				DiscordRole muteRole = Program.Database.Guild.MuteRole(eventArgs.Guild.Id).GetRole(eventArgs.Guild);
				if (muteRole != null) await Moderation.Config.FixMuteRolePermissions(eventArgs.Channel, muteRole);

				DiscordRole antimemeRole = Program.Database.Guild.AntimemeRole(eventArgs.Guild.Id).GetRole(eventArgs.Guild);
				if (antimemeRole != null) await Moderation.Config.FixAntimemeRolePermissions(eventArgs.Channel, antimemeRole);

				DiscordRole voiceBanRole = Program.Database.Guild.VoiceBanRole(eventArgs.Guild.Id).GetRole(eventArgs.Guild);
				if (voiceBanRole != null) await Moderation.Config.FixVoiceBanPermissions(eventArgs.Channel, voiceBanRole);
			}
			else
			{
				Program.Database.Guild.InsertGuildId(eventArgs.Guild.Id);
				await Handler(client, eventArgs);
			}
		}
	}
}
