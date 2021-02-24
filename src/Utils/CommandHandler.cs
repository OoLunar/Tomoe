using System.Linq;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

using Tomoe.Db;

namespace Tomoe.Utils
{
	public class CommandHandler
	{
		public static async Task Handler(DiscordClient client, MessageCreateEventArgs eventArgs)
		{
			CommandsNextExtension commandsNext = client.GetCommandsNext();
			DiscordMessage message = eventArgs.Message;
			int commandStart = message.GetStringPrefixLength(Config.Prefix);

			if (commandStart == -1)
			{
				commandStart = message.GetMentionPrefixLength(client.CurrentUser);
				if (commandStart == -1) return;
			}

			string prefix = message.Content.Substring(0, commandStart);
			string commandName = message.Content.Substring(commandStart);
			Command command = commandsNext.FindCommand(commandName, out string args);

			if (command == null) return;
			await eventArgs.Channel.TriggerTypingAsync();

			CommandContext context = commandsNext.CreateContext(message, prefix, command, args);
			if (context.Guild != null)
			{
				Database Database = (Database)Program.ServiceProvider.GetService(typeof(Database));
				Guild guild = await Database.Guilds.FirstOrDefaultAsync(guild => guild.Id == eventArgs.Guild.Id);
				if (guild == null)
				{
					guild = new(context.Guild.Id);
					_ = await Database.Guilds.AddAsync(guild);
					_ = await Database.SaveChangesAsync();
				}
				GuildUser guildUser = guild.Users.FirstOrDefault(user => user.Id == context.User.Id);
				if (guildUser == null)
				{
					guildUser = new(context.User.Id);
					guildUser.Roles = context.Member.Roles.Select(role => role.Id).ToList();
					guild.Users.Add(guildUser);
				}

				foreach (DiscordUser userMention in context.Message.MentionedUsers)
				{
					GuildUser mentionUser = guild.Users.FirstOrDefault(user => user.Id == userMention.Id);
					if (guildUser == null)
					{
						guildUser = new(context.User.Id);
						guildUser.Roles = context.Member.Roles.Select(role => role.Id).ToList();
						guild.Users.Add(guildUser);
					}
				}
			}

			_ = Task.Run(async () => await commandsNext.ExecuteCommandAsync(context));
			return;
		}
	}
}
