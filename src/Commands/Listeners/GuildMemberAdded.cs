using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tomoe.Db;

namespace Tomoe.Commands.Listeners
{
	public class GuildMemberAdded
	{
		public static async Task Handler(DiscordClient _client, GuildMemberAddEventArgs eventArgs)
		{
			using IServiceScope scope = Program.ServiceProvider.CreateScope();
			Database database = scope.ServiceProvider.GetService<Database>();
			Guild guild = await database.Guilds.FirstOrDefaultAsync(guild => guild.Id == eventArgs.Guild.Id);
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
				else
				{
					user = new(eventArgs.Member.Id);
					guild.Users.Add(user);
				}
			}
		}
	}
}
