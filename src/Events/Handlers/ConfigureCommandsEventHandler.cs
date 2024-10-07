using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.Converters;
using DSharpPlus.Commands.EventArgs;
using DSharpPlus.Commands.Processors.SlashCommands.ArgumentModifiers;
using DSharpPlus.Commands.Processors.TextCommands.ContextChecks;
using DSharpPlus.Commands.Trees;
using DSharpPlus.Entities;
using OoLunar.Tomoe.AutocompleteProviders;

namespace OoLunar.Tomoe.Events.Handlers
{
    public sealed class ConfigureCommandsEventHandler
    {
        public static Task ConfigureCommandsAsync(CommandsExtension _, ConfigureCommandsEventArgs eventArgs)
        {
            foreach (CommandBuilder command in eventArgs.CommandTrees.SelectMany(commandBuilder => commandBuilder.Flatten()))
            {
                foreach (CommandParameterBuilder parameter in command.Parameters)
                {
                    if (parameter.Type is null)
                    {
                        continue;
                    }

                    Type baseParameterType = IArgumentConverter.GetConverterFriendlyBaseType(parameter.Type);
                    if (!parameter.Attributes.Any(attribute => attribute is TextMessageReplyAttribute) &&
                        (baseParameterType == typeof(DiscordUser) || baseParameterType == typeof(DiscordMember) || baseParameterType == typeof(DiscordMessage)))
                    {
                        parameter.Attributes.Add(new TextMessageReplyAttribute());
                    }

                    if (baseParameterType == typeof(DiscordGuild) || baseParameterType == typeof(DiscordRole) || baseParameterType == typeof(DiscordMember))
                    {
                        parameter.Attributes.Add(new RequireGuildAttribute());
                    }
                    else if (baseParameterType == typeof(CultureInfo))
                    {
                        parameter.Attributes.Add(new SlashAutoCompleteProviderAttribute<CultureInfoAutocompleteProvider>());
                    }
                    else if (baseParameterType == typeof(TimeZoneInfo))
                    {
                        parameter.Attributes.Add(new SlashAutoCompleteProviderAttribute<TimeZoneInfoAutocompleteProvider>());
                    }
                }
            }

            return Task.CompletedTask;
        }
    }
}
