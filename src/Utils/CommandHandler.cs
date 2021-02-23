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

			if (commandStart == -1) return;

			string prefix = message.Content.Substring(0, commandStart);
			string commandName = message.Content[0..commandStart];
			Command command = commandsNext.FindCommand(commandName, out string args);

			if (command == null) return;

			CommandContext context = commandsNext.CreateContext(message, prefix, command, args);
			Guild guild = await Program.Database.Guilds.FirstOrDefaultAsync(guild => guild.Id == context.Guild.Id);
			if (guild == null)
			{
				guild = new(context.Guild.Id);
				_ = await Program.Database.Guilds.AddAsync(guild);
			}
			GuildUser guildUser = guild.Users.FirstOrDefault(user => user.Id == context.User.Id);
			if (guildUser == null)
			{
				guildUser = new(context.User.Id);
				guildUser.Roles = context.Member.Roles.Select(role => role.Id).ToList();
				guild.Users.Add(guildUser);
			}

			_ = Task.Run(async () => await commandsNext.ExecuteCommandAsync(context));
			return;
		}
	}
}
