using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Tomoe.Commands.Moderation.Attributes;

// Copied from https://github.com/TheRealHona/DSharpPlusBotTemplate/blob/main/TemplateDiscordBot/Services/CommandService.cs
// Go take a look at their project!
namespace Tomoe.Utils
{
	internal class CommandService
	{
		private static readonly Logger _logger = new("CommandService");
		internal static async Task Launch(DiscordShardedClient discordClient, IServiceProvider services)
		{
			while (true)
			{
				try
				{
					IReadOnlyDictionary<int, CommandsNextExtension> commandsCollection = await discordClient.UseCommandsNextAsync(new CommandsNextConfiguration
					{
						StringPrefixes = new[] { Config.Prefix },
						CaseSensitive = false,
						EnableDms = true,
						EnableDefaultHelp = false,
						UseDefaultCommandHandler = false,
						Services = services
					});
					_ = discordClient.UseInteractivityAsync(new InteractivityConfiguration
					{
						// default timeout for other actions to 2 minutes
						Timeout = TimeSpan.FromMinutes(2)
					});

					foreach (CommandsNextExtension commands in commandsCollection.Values)
					{
						commands.RegisterConverter(new ImageFormatConverter());
						commands.RegisterConverter(new ExpandedTimeSpanConverter());
						commands.RegisterCommands(Assembly.GetEntryAssembly());
						commands.CommandErrored += CommandErrored;
					}
					break;
				}
				catch (InvalidOperationException)
				{
					_logger.Error("Failed to initalize shards on CommandsNext. Trying again...");
				}
			}
		}

		private static async Task CommandErrored(CommandsNextExtension _client, CommandErrorEventArgs args)
		{
			// No need to log when a command isn't found
			if (!(args.Exception is CommandNotFoundException) && !args.Handled)
			{
				if (args.Exception is ChecksFailedException)
				{
					ChecksFailedException error = args.Exception as ChecksFailedException;
					if (error.Context.Channel.IsPrivate)
					{
						_ = await Program.SendMessage(args.Context, Constants.NotAGuild);
						args.Handled = true;
					}
					else if (error.FailedChecks.OfType<Punishment>() != null) args.Handled = true;
					else if (error.FailedChecks.OfType<RequireUserPermissionsAttribute>() != null)
					{
						_ = await Program.SendMessage(args.Context, Constants.MissingPermissions);
						args.Handled = true;
					}
				}
				else if (args.Exception is NotImplementedException)
				{
					_ = await Program.SendMessage(args.Context, $"{args.Command.Name} hasn't been implemented yet!");
				}
				else
				{
					_logger.Error($"'{args.Command?.QualifiedName ?? "<unknown command>"}' errored: {args.Exception.GetType()}, {args.Exception.Message ?? "<no message>"}\n{args.Exception.StackTrace}");
				}
			}
		}
	}
}
