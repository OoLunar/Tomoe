using System.Linq;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

using Tomoe.Db;

namespace Tomoe.Commands.Listeners
{
	public class CacheHandler
	{
		public static async Task Handler(DiscordClient _client, MessageCreateEventArgs eventArgs)
		{
			if (eventArgs.Guild == null) return;
			Database Database = (Database)Program.ServiceProvider.GetService(typeof(Database));
			Guild guild = await Database.Guilds.FirstOrDefaultAsync(guild => guild.Id == eventArgs.Guild.Id);
			if (guild == null)
			{
				guild = new(eventArgs.Guild.Id);
				_ = await Database.Guilds.AddAsync(guild);
				_ = await Database.SaveChangesAsync();
			}
			GuildUser guildUser = guild.Users.FirstOrDefault(user => user.Id == eventArgs.Author.Id);
			if (guildUser == null)
			{
				guildUser = new(eventArgs.Author.Id);
				guildUser.Roles = eventArgs.Author.GetMember(eventArgs.Guild).Roles.Select(role => role.Id).ToList();
				guild.Users.Add(guildUser);
			}

			foreach (DiscordUser userMention in eventArgs.Message.MentionedUsers)
			{
				GuildUser mentionUser = guild.Users.FirstOrDefault(user => user.Id == userMention.Id);
				if (mentionUser == null)
				{
					guildUser = new(userMention.Id);
					guildUser.Roles = userMention.GetMember(eventArgs.Guild).Roles.Select(role => role.Id).ToList();
					guild.Users.Add(guildUser);
				}
			}
		}
	}
}
