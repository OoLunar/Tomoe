using System.Linq;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

using Tomoe.Db;

namespace Tomoe.Commands.Listeners
{
	public class GuildMemberAdded
	{
		public static async Task Handler(DiscordClient client, GuildMemberAddEventArgs eventArgs)
		{
			Guild guild = await Program.Database.Guilds.FirstOrDefaultAsync(guild => guild.Id == eventArgs.Guild.Id);
			if (guild != null)
			{
				GuildUser user = guild.Users.FirstOrDefault(user => user.Id == eventArgs.Member.Id);
				if (user != null)
				{
					foreach (ulong roleId in user.Roles)
					{
						DiscordRole role = eventArgs.Guild.GetRole(roleId);
						if (role == null) continue;
						await eventArgs.Member.GrantRoleAsync(role, "Persistent Roles.");
					}
				}
			}
		}
	}
}
