using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System;
using DSharpPlus.CommandsNext.Exceptions;

// Copied from https://github.com/TheRealHona/DSharpPlusBotTemplate/blob/main/TemplateDiscordBot/Services/CommandService.cs
// Go take a look at their project!
namespace Tomoe.Utils
{
	internal class CommandService
	{
		private static readonly Logger _logger = new("CommandService");
		internal static async Task Launch(DiscordShardedClient discordClient)
		{
			while (true)
			{
				try
				{
					IReadOnlyDictionary<int, CommandsNextExtension> commandsCollection = await discordClient.UseCommandsNextAsync(new CommandsNextConfiguration
					{
						StringPrefixes = new[] { Config.Prefix },
						CaseSensitive = false,
						EnableMentionPrefix = true,
						EnableDms = true
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
						_ = Program.SendMessage(args.Context, Program.NotAGuild, ExtensionMethods.FilteringAction.CodeBlocksZeroWidthSpace);
					}
					else if (error.FailedChecks.OfType<RequireUserPermissionsAttribute>() != null && args.Command.Module.ModuleType != typeof(Commands.Public.Tags))
					{
						_ = Program.SendMessage(args.Context, Program.MissingPermissions, ExtensionMethods.FilteringAction.CodeBlocksZeroWidthSpace);
					}
				}
				else if (args.Exception is NotImplementedException)
				{
					_ = Program.SendMessage(args.Context, $"{args.Command.Name} hasn't been implemented yet!", ExtensionMethods.FilteringAction.CodeBlocksZeroWidthSpace);
				}
				else
				{
					_logger.Error($"'{args.Command?.QualifiedName ?? "<unknown command>"}' errored: {args.Exception.GetType()}, {args.Exception.Message ?? "<no message>"}\n{args.Exception.StackTrace}");
				}
			}
		}
	}
}
