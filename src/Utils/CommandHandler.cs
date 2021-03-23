using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Tomoe.Commands.Moderation;
using Tomoe.Db;

namespace Tomoe.Utils
{
	public class CommandHandler
	{
		public static async Task Handler(DiscordClient client, MessageCreateEventArgs eventArgs)
		{
			CommandsNextExtension commandsNext = client.GetCommandsNext();
			DiscordMessage message = eventArgs.Message;
			int commandStart = message.GetStringPrefixLength(Program.Config.DiscordBotPrefix);

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
			Database Database = Program.ServiceProvider.GetService<Database>();
			Guild guild = await Database.Guilds.FirstOrDefaultAsync(guild => guild.Id == context.Guild.Id);
			if (guild != null && guild.IgnoredChannels.Contains(context.Channel.Id)) return;
			await eventArgs.Channel.TriggerTypingAsync();
			_ = Task.Run(async () => await commandsNext.ExecuteCommandAsync(context));

			await ModBook.Add(context, $"Executed `{context.Command.Name}` command.", DiscordEvent.None, ModAction.CommandExecuted);
			return;
		}
	}
}
