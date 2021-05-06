using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext.Converters;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Tomoe.Db;
using Tomoe.Utils.Converters;
using Tomoe.Utils.Exceptions;

// Copied from https://github.com/TheRealHona/DSharpPlusBotTemplate/blob/main/TemplateDiscordBot/Services/CommandService.cs
// Go take a look at their project!
namespace Tomoe.Utils
{
	internal class CommandService
	{
		private static readonly ILogger _logger = Log.ForContext<CommandService>();

		internal static async Task Launch(DiscordShardedClient discordClient, IServiceProvider services)
		{
			while (true)
			{
				try
				{
					IReadOnlyDictionary<int, CommandsNextExtension> commandsCollection = await discordClient.UseCommandsNextAsync(new CommandsNextConfiguration
					{
						StringPrefixes = new[] { Program.Config.DiscordBotPrefix },
						CaseSensitive = false,
						EnableDms = true,
						EnableDefaultHelp = false,
						UseDefaultCommandHandler = false,
						Services = services,
					});
					_ = discordClient.UseInteractivityAsync(new InteractivityConfiguration
					{
						// default timeout for other actions to 2 minutes
						Timeout = TimeSpan.FromMinutes(2)
					});

					foreach (CommandsNextExtension commands in commandsCollection.Values)
					{
						commands.RegisterConverter(new ImageFormatConverter());
						commands.RegisterConverter(new RoleActionConverter());
						commands.RegisterConverter(new TimeSpanConverter());
						commands.RegisterConverter(new StrikeConverter());
						commands.RegisterConverter(new TagConverter());
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

		public static async Task CommandErrored(CommandsNextExtension client, CommandErrorEventArgs args) => await CommandErrored(client.Client, args.Context, args.Exception);

		public static async Task CommandErrored(DiscordClient client, CommandContext context, Exception error)
		{
			using IServiceScope scope = Program.ServiceProvider.CreateScope();
			Database database = scope.ServiceProvider.GetService<Database>();
			GuildConfig guildConfig = await database.GuildConfigs.FirstOrDefaultAsync(guildConfig => guildConfig.Id == context.Guild.Id);
			switch (error)
			{
				case CommandNotFoundException:
					await context.Message.CreateReactionAsync(Constants.QuestionMark);
					return;
				case ArgumentException:
					Command helpCommand = context.CommandsNext.FindCommand($"help {(context.Command is CommandGroup ? $"{context.Command.QualifiedName} {context.Command.Name}" : context.Command.QualifiedName)}", out string helpCommandArgs);
					CommandContext helpContext = context.CommandsNext.CreateContext(context.Message, context.Prefix, helpCommand, helpCommandArgs);
					_ = await helpCommand.ExecuteAsync(helpContext);
					break;
				case NotImplementedException:
					_ = await Program.SendMessage(context, $"{context.Command.Name} hasn't been implemented yet!");
					break;
				case HierarchyException:
					if (!guildConfig.ShowPermissionErrors)
					{
						await context.Message.CreateReactionAsync(Constants.NoPermission);
					}
					else
					{
						_ = await Program.SendMessage(context, $"**[Denied: Their hierarchy is the same as or higher than yours. You don't have enough power over them.]**");
					}
					break;
				case ChecksFailedException:
					ChecksFailedException checksFailedException = error as ChecksFailedException;
					if (context.Channel.IsPrivate)
					{
						_ = await Program.SendMessage(context, Constants.NotAGuild);
					}
					else if (checksFailedException.FailedChecks.OfType<RequireUserPermissionsAttribute>() != null)
					{
						if (!guildConfig.ShowPermissionErrors)
						{
							await context.Message.CreateReactionAsync(Constants.NoPermission);
						}
						else
						{
							_ = await Program.SendMessage(context, Constants.MissingPermissions);
						}
					}
					break;
				default:
					_logger.Error($"'{context.Command?.QualifiedName ?? "<unknown command>"}' errored: {error.GetType()}, {error.Message ?? "<no message>"}\n{error.StackTrace}");
					_ = await Program.SendMessage(context, Formatter.Bold("[Error: An unknown error occured. Try executing the command again?]"));
					break;
			}
		}
	}
}
