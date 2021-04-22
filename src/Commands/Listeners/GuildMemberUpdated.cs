using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.EventArgs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tomoe.Db;

namespace Tomoe.Commands.Listeners
{
	public class GuildMemberUpdated
	{
		/// <summary>
		/// Updates the users roles for role persistance.
		/// </summary>
		/// <param name="_client">Unused <see cref="DiscordClient"/>.</param>
		/// <param name="eventArgs">Used to update the roles on the database.</param>
		/// <returns></returns>
		public static async Task Handler(DiscordClient _client, GuildMemberUpdateEventArgs eventArgs)
		{
			using IServiceScope scope = Program.ServiceProvider.CreateScope();
			Database database = scope.ServiceProvider.GetService<Database>();
			GuildConfig guildConfig = await database.GuildConfigs.FirstOrDefaultAsync(guild => guild.Id == eventArgs.Guild.Id);
			if (guildConfig != null)
			{
				GuildUser guildUser = database.GuildUsers.FirstOrDefault(user => user.UserId == eventArgs.Member.Id && user.GuildId == eventArgs.Guild.Id);
				if (guildUser != null)
				{
					guildUser.Roles.AddRange(eventArgs.Member.Roles.Except(new[] { eventArgs.Guild.EveryoneRole }).Select(role => role.Id));
				}
			}
		}
	}
}
