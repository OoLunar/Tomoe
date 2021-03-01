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
			string commandName = message.Content[commandStart..];
			Command command = commandsNext.FindCommand(commandName, out string args);

			if (command == null) return;

			CommandContext context = commandsNext.CreateContext(message, prefix, command, args);
			Database Database = (Database)Program.ServiceProvider.GetService(typeof(Database));
			Guild guild = await Database.Guilds.FirstOrDefaultAsync(guild => guild.Id == context.Guild.Id);
			if (guild != null && guild.IgnoredChannels.Contains(context.Channel.Id)) return;
			await eventArgs.Channel.TriggerTypingAsync();

			_ = Task.Run(async () => await commandsNext.ExecuteCommandAsync(context));
			return;
		}
	}
}
