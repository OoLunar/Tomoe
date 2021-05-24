// Based off of https://github.com/TheRealHona/DSharpPlusBotTemplate/blob/main/TemplateDiscordBot/Services/CommandService.cs
// Go take a look at their project!
namespace Tomoe.Utils
{
    using DSharpPlus;
    using DSharpPlus.CommandsNext;
    using DSharpPlus.CommandsNext.Attributes;
    using DSharpPlus.CommandsNext.Converters;
    using DSharpPlus.CommandsNext.Exceptions;
    using DSharpPlus.Entities;
    using DSharpPlus.Interactivity;
    using DSharpPlus.Interactivity.Extensions;
    using Humanizer;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using Serilog;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using Tomoe.Db;
    using Tomoe.Utils.Converters;
    using Tomoe.Utils.Exceptions;

    internal class CommandService
    {
        private static readonly ILogger _logger = Log.ForContext<CommandService>();

        internal static async Task Launch(DiscordShardedClient discordClient, IServiceProvider services)
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
            await discordClient.UseInteractivityAsync(new InteractivityConfiguration
            {
                // default timeout for other actions to 2 minutes
                Timeout = TimeSpan.FromMinutes(2)
            });

            foreach (CommandsNextExtension commandsNextExtension in commandsCollection.Values)
            {
                commandsNextExtension.RegisterConverter(new ImageFormatConverter());
                commandsNextExtension.RegisterConverter(new RoleActionConverter());
                commandsNextExtension.RegisterConverter(new TimeSpanConverter());
                commandsNextExtension.RegisterConverter(new ReminderConverter());
                commandsNextExtension.RegisterConverter(new LogTypeConverter());
                commandsNextExtension.RegisterConverter(new StrikeConverter());
                commandsNextExtension.RegisterConverter(new TagConverter());
                commandsNextExtension.RegisterCommands(Assembly.GetEntryAssembly());
                commandsNextExtension.CommandErrored += CommandErrored;
                commandsNextExtension.CommandExecuted += CommandExecuted;
            }
        }

        public static async Task CommandExecuted(CommandsNextExtension client, CommandExecutionEventArgs eventArgs) => await eventArgs.Context.Message.CreateReactionAsync(Constants.Check);

        public static async Task CommandErrored(CommandsNextExtension client, CommandErrorEventArgs eventArgs) => await CommandErrored(eventArgs.Context, eventArgs.Exception);

        public static async Task CommandErrored(CommandContext context, Exception error)
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
                    await helpCommand.ExecuteAsync(helpContext);
                    break;
                case NotImplementedException:
                    await Program.SendMessage(context, $"{context.Command.Name} hasn't been implemented yet!");
                    break;
                case HierarchyException:
                    if (!guildConfig.ShowPermissionErrors)
                    {
                        await context.Message.CreateReactionAsync(Constants.NoPermission);
                    }
                    else
                    {
                        await Program.SendMessage(context, $"**[Denied: Their highest role is higher or equal to your highest role! You don't have enough power over them.]**");
                    }
                    break;
                case ChecksFailedException:
                    ChecksFailedException checksFailedException = error as ChecksFailedException;
                    if (context.Channel.IsPrivate)
                    {
                        await Program.SendMessage(context, Constants.NotAGuild);
                    }
                    else if (checksFailedException.FailedChecks.OfType<RequireUserPermissionsAttribute>() != null)
                    {
                        if (!guildConfig.ShowPermissionErrors)
                        {
                            await context.Message.CreateReactionAsync(Constants.NoPermission);
                        }
                        else
                        {
                            await Program.SendMessage(context, Constants.MissingPermissions);
                        }
                    }
                    break;
                default:
                    _logger.Error($"'{context.Command?.QualifiedName ?? "<unknown command>"}' errored: {error.GetType()}, {error.Message ?? "<no message>"}\n{error.StackTrace}");
                    await Program.SendMessage(context, Formatter.Bold("[Error: An unknown error occured. Try executing the command again?]"));
                    DiscordChannel errorChannel = await context.Client.GetChannelAsync(Program.Config.Update.ChannelId);
                    await errorChannel.SendMessageAsync($"{error.GetType()}: {error.Message ?? "<no message>"}\n{Formatter.BlockCode(error.StackTrace.Truncate(1950))}");
                    break;
            }
        }
    }
}
