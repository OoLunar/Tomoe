using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OoLunar.DSharpPlus.CommandAll;
using OoLunar.DSharpPlus.CommandAll.Commands.Builders.Commands;
using OoLunar.DSharpPlus.CommandAll.EventArgs;

namespace OoLunar.Tomoe.Events.Handlers
{
    public sealed class ConfigureCommandsHandler
    {
        private readonly ILogger<ConfigureCommandsHandler> Logger;

        public ConfigureCommandsHandler(ILogger<ConfigureCommandsHandler> logger) => Logger = logger ?? throw new ArgumentNullException(nameof(logger));

        [DiscordEvent]
        public Task OnConfigureCommandsAsync(CommandAllExtension extension, ConfigureCommandsEventArgs configureCommandsEventArgs)
        {
            Logger.LogInformation("Configuring commands.");
            DescribeCommands(configureCommandsEventArgs.CommandManager.CommandBuilders.Values);
            Logger.LogInformation("Commands configured.");
            return Task.CompletedTask;
        }

        private void DescribeCommands(IEnumerable<CommandBuilder> builders)
        {
            foreach (CommandBuilder command in builders)
            {
                if (string.IsNullOrWhiteSpace(command.Description))
                {
                    Logger.LogWarning("Command {CommandName} does not have a description.", command.Name);
                    command.Description = "No description provided.";
                }

                foreach (CommandOverloadBuilder overload in command.Overloads)
                {
                    foreach (CommandParameterBuilder parameter in overload.Parameters)
                    {
                        if (parameter.Name == "message")
                        {
                            parameter.SlashMetadata.IsRequired = true;
                        }

                        if (string.IsNullOrWhiteSpace(parameter.Description))
                        {
                            Logger.LogWarning("Parameter {ParameterName} of command {CommandName} does not have a description.", parameter.Name, command.Name);
                            parameter.Description = "No description provided.";
                        }
                    }
                }

                DescribeCommands(command.Subcommands);
            }
        }
    }
}
