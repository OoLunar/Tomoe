using System.Linq;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

using Tomoe.Db;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Serilog;

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

			bool saved = false;
			while (!saved)
				try
				{
					_ = await database.SaveChangesAsync();
					saved = true;
				}
				catch (DbUpdateConcurrencyException error)
				{
					foreach (EntityEntry entry in error.Entries)
					{
						if (entry.Entity is Guild)
						{
							if (entry.CurrentValues.GetValue<ulong>("Id") == guild.Id)
							{
								entry.OriginalValues.SetValues(entry.CurrentValues);
							}
							else
							{
								entry.OriginalValues.SetValues(entry.GetDatabaseValues());
							}
						}
					}
				}
			_logger.Information($"\"{eventArgs.Guild.Name}\" ({eventArgs.Guild.Id}) is ready! Handling {eventArgs.Guild.MemberCount} members.");
		}
	}
}
