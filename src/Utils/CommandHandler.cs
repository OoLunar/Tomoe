using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
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
			int commandStart = -1;
			string prefix = string.Empty;

			if (eventArgs.Channel.IsPrivate)
			{
				// DM channel, try using the default prefix
				commandStart = eventArgs.Message.GetStringPrefixLength(Program.Config.DiscordBotPrefix);
				if (commandStart != -1)
				{
					prefix = eventArgs.Message.Content.Substring(0, commandStart);
				}
			}
			else
			{
				using IServiceScope scope = Program.ServiceProvider.CreateScope();
				Database database = scope.ServiceProvider.GetService<Database>();
				GuildConfig guildConfig = await database.GuildConfigs.FirstOrDefaultAsync(guild => guild.Id == eventArgs.Guild.Id);
				// If the guild doesn't exist, create it.
				if (guildConfig == null)
				{
					guildConfig = new(eventArgs.Guild.Id);
					_ = database.GuildConfigs.Add(guildConfig);
					_ = await database.SaveChangesAsync();
				}
				// Guild exists, check to see it's not executing in an ignored channel.
				else if (guildConfig.IgnoredChannels.Contains(eventArgs.Channel.Id))
				{
					return;
				}

				// Iterate through each prefix and try to see if said prefix was used.
				// Add the default bot prefix and the guild prefixes together into a new list for less code
				foreach (string guildPrefix in new List<string>(guildConfig.Prefixes) { Program.Config.DiscordBotPrefix })
				{
					commandStart = eventArgs.Message.GetStringPrefixLength(guildPrefix);
					if (commandStart != -1)
					{
						prefix = eventArgs.Message.Content.Substring(0, commandStart);
						break;
					}
				}
			}

			// None of the prefixes were used, so try to see if the bot mention was the prefix.
			if (commandStart == -1)
			{
				commandStart = eventArgs.Message.GetMentionPrefixLength(Program.Client.CurrentUser);
				if (commandStart != -1)
				{
					prefix = eventArgs.Message.Content.Substring(0, commandStart);
				}
				else
				{
					return;
				}
			}

			//TODO: Replace the 3 `prefix = eventArgs.Message.Content.Substring(0, commandStart);` into one if statement

			string commandName = eventArgs.Message.Content[commandStart..];
			Command command = commandsNext.FindCommand(commandName, out string args);
			if (command == null) return;

			CommandContext context = commandsNext.CreateContext(eventArgs.Message, prefix, command, args);
			// Let the user know that the bot is executing the command.
			await eventArgs.Message.CreateReactionAsync(Constants.Loading);
			_ = Task.Run(async () => await commandsNext.ExecuteCommandAsync(context));
			await eventArgs.Message.DeleteReactionAsync(Constants.Loading, context.Client.CurrentUser);
			// Log that the command was executed.
			await ModLogs.CommandUsage(context);
			return;
		}
	}
}
