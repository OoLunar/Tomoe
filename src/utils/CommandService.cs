using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext.Exceptions;

// Copied from https://github.com/TheRealHona/DSharpPlusBotTemplate/blob/main/TemplateDiscordBot/Services/CommandService.cs
// Go take a look at their project!
namespace Tomoe.Utils {
    public class CommandService {
        private static Logger _logger = new Logger("CommandService");
        public static async Task Launch(DiscordShardedClient discordClient) {
            while (true) {
                try {
                    IReadOnlyDictionary<int, DSharpPlus.CommandsNext.CommandsNextExtension> commandsCollection = await discordClient.UseCommandsNextAsync(new CommandsNextConfiguration {
                        StringPrefixes = new [] { Config.Prefix },
                            CaseSensitive = false,
                            EnableMentionPrefix = true,
                            EnableDms = true
                    });
                    foreach (CommandsNextExtension commands in commandsCollection.Values) {
                        commands.RegisterConverter(new ImageFormatConverter());
                        commands.RegisterCommands(Assembly.GetEntryAssembly());
                        commands.CommandErrored += commandErrored;
                    }
                    break;
                } catch (System.InvalidOperationException) {
                    _logger.Error("Failed to initalize shards on CommandsNext. Trying again...");
                }
            }
        }

        private static async Task commandErrored(CommandsNextExtension _, CommandErrorEventArgs e) {
            // No need to log when a command isn't found
            if (!(e.Exception is CommandNotFoundException) && !e.Handled) {
                if (e.Exception is ChecksFailedException) {
                    ChecksFailedException error = e.Exception as ChecksFailedException;
                    if (error.Context.Channel.IsPrivate) Program.SendMessage(e.Context, Program.NotAGuild, ExtensionMethods.FilteringAction.CodeBlocksZeroWidthSpace);
                    else if (error.FailedChecks.OfType<RequireUserPermissionsAttribute>() != null && e.Command.Module.ModuleType != typeof(Tomoe.Commands.Public.Tags)) Program.SendMessage(e.Context, Program.MissingPermissions, ExtensionMethods.FilteringAction.CodeBlocksZeroWidthSpace);
                } else if (e.Exception is System.NotImplementedException) Program.SendMessage(e.Context, $"{e.Command.Name} hasn't been implemented yet!", ExtensionMethods.FilteringAction.CodeBlocksZeroWidthSpace);
                else _logger.Error($"'{e.Command?.QualifiedName ?? "<unknown command>"}' errored: {e.Exception.GetType()}, {e.Exception.Message ?? "<no message>"}\n{e.Exception.StackTrace}");
            }
        }
    }
}