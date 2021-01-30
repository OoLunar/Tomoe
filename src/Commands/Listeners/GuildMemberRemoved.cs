using System.Linq;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.EventArgs;

namespace Tomoe.Commands.Listeners
{
	public class GuildMemberRemoved
	{
		public static async Task Handler(DiscordClient client, GuildMemberRemoveEventArgs eventArgs)
		{
			if (Program.Database.Guild.GuildIdExists(eventArgs.Guild.Id))
			{
				if (Program.Database.User.Exists(eventArgs.Guild.Id, eventArgs.Member.Id)) Program.Database.User.SetRoles(eventArgs.Guild.Id, eventArgs.Member.Id, eventArgs.Member.Roles.Select(role => role.Id).ToArray());
				else
				{
					Program.Database.User.Insert(eventArgs.Guild.Id, eventArgs.Member.Id);
					Program.Database.User.SetRoles(eventArgs.Guild.Id, eventArgs.Member.Id, eventArgs.Member.Roles.Select(role => role.Id).ToArray());
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
