using System.Linq;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using DSharpPlus;
using DSharpPlus.EventArgs;

using Tomoe.Db;

namespace Tomoe.Commands.Listeners
{
	public class GuildMemberRemoved
	{
		public static async Task Handler(DiscordClient client, GuildMemberRemoveEventArgs eventArgs)
		{
			Guild guild = await Program.Database.Guilds.FirstOrDefaultAsync(guild => guild.Id == eventArgs.Guild.Id);
			if (guild != null)
			{
				GuildUser user = guild.Users.FirstOrDefault(user => user.Id == eventArgs.Member.Id);
				if (user != null) user.Roles = eventArgs.Member.Roles.Except(new[] { eventArgs.Guild.EveryoneRole }).Select(role => role.Id).ToList();
			}
		}
	}
}
