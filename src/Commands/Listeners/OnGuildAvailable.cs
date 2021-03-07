using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Tomoe.Db;

namespace Tomoe.Commands.Listeners
{
	public class GuildAvailable
	{
		private static readonly ILogger _logger = Log.ForContext<GuildAvailable>();

		public static async Task Handler(DiscordClient _client, GuildCreateEventArgs eventArgs)
		{
			using IServiceScope scope = Program.ServiceProvider.CreateScope();
			Database database = scope.ServiceProvider.GetService<Database>();

			Guild guild = await database.Guilds.FirstOrDefaultAsync(guild => guild.Id == eventArgs.Guild.Id);
			if (guild == null)
			{
				guild = new(eventArgs.Guild.Id);
				_ = database.Guilds.Add(guild);
			}

			foreach (DiscordMember member in eventArgs.Guild.Members.Values)
			{
				GuildUser guildUser = new(member.Id);
				guildUser.Roles = member.Roles.Except(new[] { eventArgs.Guild.EveryoneRole }).Select(role => role.Id).ToList();
				guild.Users.Add(guildUser);
			}

			_ = await database.SaveChangesAsync();
			_logger.Information($"\"{eventArgs.Guild.Name}\" ({eventArgs.Guild.Id}) is ready! Handling {eventArgs.Guild.MemberCount} members.");
		}
	}
}
