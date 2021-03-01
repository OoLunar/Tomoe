using System.Linq;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

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
			Database Database = Program.ServiceProvider.CreateScope().ServiceProvider.GetService(typeof(Database)) as Database;

			Guild guild = await Database.Guilds.FirstOrDefaultAsync(guild => guild.Id == eventArgs.Guild.Id);
			if (guild == null)
			{
				guild = new(eventArgs.Guild.Id);
				_ = Database.Guilds.Add(guild);
			}

			foreach (DiscordMember member in eventArgs.Guild.Members.Values)
			{
				GuildUser guildUser = new(member.Id);
				guildUser.Roles = member.Roles.Except(new[] { eventArgs.Guild.EveryoneRole }).Select(role => role.Id).ToList();
				guild.Users.Add(guildUser);
			}
			_logger.Info($"\"{eventArgs.Guild.Name}\" ({eventArgs.Guild.Id}) is ready! Handling {eventArgs.Guild.MemberCount} members.");
		}
	}
}
