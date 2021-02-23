using System.Linq;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

using Tomoe.Db;
using Tomoe.Utils;

namespace Tomoe.Commands.Listeners
{
	public class GuildAvailable
	{
		private static readonly Logger _logger = new("Commands.Listeners.GuildAvailable");

		public static async Task Handler(DiscordClient _client, GuildCreateEventArgs eventArgs)
		{
			Guild guild = await Program.Database.Guilds.FirstOrDefaultAsync(guild => guild.Id == eventArgs.Guild.Id);
			if (guild == null)
			{
				guild = new(eventArgs.Guild.Id);
				_ = await Program.Database.Guilds.AddAsync(guild);
				foreach (DiscordMember member in eventArgs.Guild.Members.Values)
				{
					GuildUser guildUser = new(member.Id);
					guildUser.Roles = member.Roles.Select(role => role.Id).ToList();
					guild.Users.Add(guildUser);
				}
				_ = await Program.Database.SaveChangesAsync();
			}
			_logger.Info($"\"{eventArgs.Guild.Name}\" ({eventArgs.Guild.Id}) is ready! Handling {eventArgs.Guild.MemberCount} members.");
		}
	}
}
