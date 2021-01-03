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
				ulong? muteRoleId = Program.Database.Guild.MuteRole(eventArgs.Guild.Id);
				ulong? antiMemeRoleId = Program.Database.Guild.AntiMemeRole(eventArgs.Guild.Id);
				if (muteRoleId.HasValue)
				{
					DiscordRole muteRole = eventArgs.Guild.GetRole(muteRoleId.Value);
					if (muteRole != null) await Config.FixMuteRolePermissions(eventArgs.Guild, muteRole);
				}

				if (antiMemeRoleId.HasValue)
				{
					DiscordRole antiMemeRole = eventArgs.Guild.GetRole(antiMemeRoleId.Value);
					if (antiMemeRole != null) await Config.FixAntiMemeRolePermissions(eventArgs.Guild, antiMemeRole);
				}
			}
			else Program.Database.Guild.InsertGuildId(eventArgs.Guild.Id);
		}
	}
}
