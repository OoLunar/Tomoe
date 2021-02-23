using System.Linq;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using DSharpPlus;
using DSharpPlus.EventArgs;

using Tomoe.Db;

namespace Tomoe.Commands.Listeners
{
	public class GuildMemberUpdated
	{
		public static async Task Handler(DiscordClient client, GuildMemberUpdateEventArgs eventArgs)
		{
			Guild guild = await Program.Database.Guilds.FirstAsync(guild => guild.Id == eventArgs.Guild.Id);
			if (guild != null)
			{
				GuildUser user = guild.Users.First(user => user.Id == eventArgs.Member.Id);
				if (user != null) user.Roles = eventArgs.Member.Roles.Select(role => role.Id).ToList();
				else
				{
					user = new(eventArgs.Guild.Id);
					user.Roles = eventArgs.Member.Roles.Select(role => role.Id).ToList();
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
