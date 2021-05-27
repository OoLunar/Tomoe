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
    using DSharpPlus.SlashCommands;
    using Humanizer;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using Serilog;
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using Tomoe.Db;
    using Tomoe.Utils.Converters;
    using Tomoe.Utils.Exceptions;

    internal class CommandService
    {
        private static readonly ILogger _logger = Log.ForContext<CommandService>();

        internal static void Launch(DiscordClient discordClient, IServiceProvider services)
        {
            CommandsNextExtension commandsNextExtension = discordClient.UseCommandsNext(new CommandsNextConfiguration
            {
                StringPrefixes = new[] { Program.Config.DiscordBotPrefix },
                CaseSensitive = false,
                EnableDms = true,
                EnableDefaultHelp = false,
                UseDefaultCommandHandler = false,
                Services = services,
            });
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
            discordClient.UseInteractivity(new InteractivityConfiguration { Timeout = TimeSpan.FromMinutes(2) });
            SlashCommandsExtension slashCommandsExtension = discordClient.UseSlashCommands();
            slashCommandsExtension.RegisterCommands<Commands.Public.BotInfo>(832354798153236510);
            slashCommandsExtension.RegisterCommands<Commands.Public.Flip>(832354798153236510);
            slashCommandsExtension.RegisterCommands<Commands.Public.GuildIcon>(832354798153236510);
            slashCommandsExtension.RegisterCommands<Commands.Public.Invite>(832354798153236510);
            slashCommandsExtension.RegisterCommands<Commands.Public.MemberCount>(832354798153236510);
            slashCommandsExtension.RegisterCommands<Commands.Public.Ping>(832354798153236510);
            slashCommandsExtension.RegisterCommands<Commands.Public.ProfilePicture>(832354798153236510);
            slashCommandsExtension.RegisterCommands<Commands.Public.Repository>(832354798153236510);
            slashCommandsExtension.RegisterCommands<Commands.Public.GuildInfo>(832354798153236510);
            slashCommandsExtension.RegisterCommands<Commands.Public.Raw>(832354798153236510);
            slashCommandsExtension.RegisterCommands<Commands.Public.RoleInfo>(832354798153236510);
            slashCommandsExtension.RegisterCommands<Commands.Public.Tags>(832354798153236510);
            //slashCommandsExtension.RegisterCommands<Commands.Public.Sort>(832354798153236510);
            slashCommandsExtension.RegisterCommands<Commands.Public.Support>(832354798153236510);
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
