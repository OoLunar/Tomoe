using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus.CommandAll;
using DSharpPlus.CommandAll.Commands.Builders;
using DSharpPlus.CommandAll.EventArgs;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;

namespace OoLunar.Tomoe.Events.Handlers
{
    public sealed class ConfigureCommandsHandler
    {
        private readonly ILogger<ConfigureCommandsHandler> _logger;

        public ConfigureCommandsHandler(ILogger<ConfigureCommandsHandler> logger) => _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        [DiscordEvent]
        public Task OnConfigureCommandsAsync(CommandAllExtension extension, ConfigureCommandsEventArgs configureCommandsEventArgs)
        {
            _logger.LogInformation("Configuring commands.");
            DescribeCommands(configureCommandsEventArgs.CommandManager.GetCommandBuilders());
            _logger.LogInformation("Commands configured.");
            return Task.CompletedTask;
        }

        private void DescribeCommands(IEnumerable<CommandBuilder> builders)
        {
            foreach (CommandBuilder command in builders)
            {
                if (string.IsNullOrWhiteSpace(command.Description))
                {
                    _logger.LogWarning("Command {CommandName} does not have a description.", command.Name);
                    command.Description = "No description provided.";
                }

                foreach (CommandOverloadBuilder overload in command.Overloads)
                {
                    foreach (CommandParameterBuilder parameter in overload.Parameters)
                    {
                        if (parameter.Name == "imageDimensions" && parameter.ParameterInfo!.ParameterType == typeof(ushort))
                        {
                            parameter.SlashMetadata.Choices ??= new();
                            parameter.SlashMetadata.Choices.Add(new DiscordApplicationCommandOptionChoice("16 x 16px", 16));
                            parameter.SlashMetadata.Choices.Add(new DiscordApplicationCommandOptionChoice("32 x 32px", 32));
                            parameter.SlashMetadata.Choices.Add(new DiscordApplicationCommandOptionChoice("64 x 64px", 64));
                            parameter.SlashMetadata.Choices.Add(new DiscordApplicationCommandOptionChoice("128 x 128px", 128));
                            parameter.SlashMetadata.Choices.Add(new DiscordApplicationCommandOptionChoice("256 x 256px", 256));
                            parameter.SlashMetadata.Choices.Add(new DiscordApplicationCommandOptionChoice("512 x 512px", 512));
                            parameter.SlashMetadata.Choices.Add(new DiscordApplicationCommandOptionChoice("1024 x 1024px", 1024));
                            parameter.SlashMetadata.Choices.Add(new DiscordApplicationCommandOptionChoice("2048 x 2048px", 2048));
                            parameter.SlashMetadata.Choices.Add(new DiscordApplicationCommandOptionChoice("4096 x 4096px", 4096));
                        }

                        if (string.IsNullOrWhiteSpace(parameter.Description))
                        {
                            _logger.LogWarning("Parameter {ParameterName} of command {CommandName} does not have a description.", parameter.Name, command.Name);
                            parameter.Description = "No description provided.";
                        }
                    }
                }

                DescribeCommands(command.Subcommands);
            }
        }
    }
}
