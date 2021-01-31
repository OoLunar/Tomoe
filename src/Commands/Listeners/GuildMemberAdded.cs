using System.Linq;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace Tomoe.Commands.Listeners
{
	public class GuildMemberAdded
	{
		public static async Task Handler(DiscordClient client, GuildMemberAddEventArgs eventArgs)
		{
			if (Program.Database.Guild.GuildIdExists(eventArgs.Guild.Id))
			{
				if (Program.Database.User.Exists(eventArgs.Guild.Id, eventArgs.Member.Id))
				{
					ulong[] userRoles = Program.Database.User.GetRoles(eventArgs.Guild.Id, eventArgs.Member.Id);
					foreach (ulong roleId in userRoles)
					{
						DiscordRole role = eventArgs.Guild.GetRole(roleId);
						if (role == null) continue;
						await eventArgs.Member.GrantRoleAsync(role, "Persistent Roles.");
					}
				}
			}
			else
			{
				Program.Database.Guild.InsertGuildId(eventArgs.Guild.Id);
				await Handler(client, eventArgs);
			}
		}
	}
}
