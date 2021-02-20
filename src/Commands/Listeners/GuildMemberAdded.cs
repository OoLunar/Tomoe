using System.Linq;
using System.Threading.Tasks;
using Tomoe.Db;


using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.EntityFrameworkCore;

namespace Tomoe.Commands.Listeners
{
	public class GuildMemberAdded
	{
		public static async Task Handler(DiscordClient client, GuildMemberAddEventArgs eventArgs)
		{
			Guild guild = await Program.Database.Guilds.FirstAsync(guild => guild.Id == eventArgs.Guild.Id);
			if (guild != null)
			{
				GuildUser user = guild.Users.First(user => user.Id == eventArgs.Member.Id);
				if (user != null)
				{
					foreach (ulong roleId in user.Roles)
					{
						DiscordRole role = eventArgs.Guild.GetRole(roleId);
						if (role == null) continue;
						await eventArgs.Member.GrantRoleAsync(role, "Persistent Roles.");
					}
				}
				else
				{
					user = new(eventArgs.Guild.Id);
					guild.Users.Add(user);
				}
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
