using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace Tomoe.Commands.Listeners
{
	public class GuildCreated
	{
		public static async Task Handler(DiscordClient client, GuildCreateEventArgs eventArgs)
		{
			if (Program.Database.Guild.GuildIdExists(eventArgs.Guild.Id))
			{
				DiscordRole muteRole = Program.Database.Guild.MuteRole(eventArgs.Guild.Id).GetRole(eventArgs.Guild);
				DiscordRole antimemeRole = Program.Database.Guild.AntimemeRole(eventArgs.Guild.Id).GetRole(eventArgs.Guild);
				if (muteRole != null) await Config.FixMuteRolePermissions(eventArgs.Guild, muteRole);
				if (antimemeRole != null) await Config.FixAntiMemeRolePermissions(eventArgs.Guild, antimemeRole);
			}
			else Program.Database.Guild.InsertGuildId(eventArgs.Guild.Id);
		}
	}
}
