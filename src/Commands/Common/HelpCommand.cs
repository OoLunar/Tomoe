using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ArgumentModifiers;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Commands.Trees;
using DSharpPlus.Commands.Trees.Metadata;
using DSharpPlus.Entities;
using Humanizer;
using OoLunar.Tomoe.Events.Handlers;

namespace OoLunar.Tomoe.Commands.Common
{
    /// <summary>
    /// What a headache this command was.
    /// </summary>
    public static class HelpCommand
    {
        /// <summary>
        /// Sends a list of all commands or information about a specific command.
        /// </summary>
        /// <param name="command">Which specific command to get information about. Leave empty to few all commands.</param>
        [Command("help")]
        public static ValueTask ExecuteAsync(CommandContext context, [RemainingText] string? command = null)
        {
            if (string.IsNullOrWhiteSpace(command))
            {
                return context.RespondAsync(GetHelpMessage(context));
            }
            else if (GetCommand(context.Extension.Commands.Values, command) is Command foundCommand)
            {
                return context.RespondAsync(GetHelpMessage(context, foundCommand));
            }

            return context.RespondAsync($"Command {command} not found.");
        }

        private static DiscordMessageBuilder GetHelpMessage(CommandContext context)
        {
            StringBuilder stringBuilder = new();
            foreach (Command command in context.Extension.Commands.Values.OrderBy(x => x.Name))
            {
                stringBuilder.AppendLine($"`{command.Name.Titleize()}`: {(HelpCommandDocumentationMapperEventHandlers.CommandDocumentation.TryGetValue(command, out string? documentation) ? documentation.Replace('\n', ' ').Replace("  ", " ") : "No description provided.")}");
            }

            return new DiscordMessageBuilder().WithContent($"A total of {context.Extension.Commands.Values.Select(CountCommands).Sum():N0} commands were found. Use `help <command>` for more information on any of them.").AddEmbed(new DiscordEmbedBuilder().WithTitle("Commands").WithDescription(stringBuilder.ToString()));
        }

        private static DiscordMessageBuilder GetHelpMessage(CommandContext context, Command command)
        {
            DiscordEmbedBuilder embed = new();
            embed.WithTitle($"Help Command: `{command.FullName.Titleize()}`");
            embed.WithDescription(HelpCommandDocumentationMapperEventHandlers.CommandDocumentation.TryGetValue(command, out string? documentation) ? documentation : "No description provided.");
            if (command.Subcommands.Count > 0)
            {
                Command? groupCommand = command.Subcommands.FirstOrDefault(x => x.Attributes.Any(x => x is DefaultGroupCommandAttribute));
                if (groupCommand is not null)
                {
                    embed.AddField("Usage", groupCommand.GetUsage(context.Arguments.Values.First()!.ToString()!.ToLowerInvariant()));
                }

                foreach (Command subcommand in command.Subcommands.OrderBy(x => x.Name))
                {
                    embed.AddField(subcommand.Name.Titleize(), HelpCommandDocumentationMapperEventHandlers.CommandDocumentation.TryGetValue(subcommand, out string? subcommandDocumentation) ? subcommandDocumentation : "No description provided.");
                }
            }
            else
            {
                if (command.Attributes.FirstOrDefault(x => x is RequirePermissionsAttribute) is RequirePermissionsAttribute permissions)
                {
                    DiscordPermissions commonPermissions = permissions.BotPermissions & permissions.UserPermissions;
                    DiscordPermissions botUniquePermissions = permissions.BotPermissions ^ commonPermissions;
                    DiscordPermissions userUniquePermissions = permissions.UserPermissions ^ commonPermissions;
                    StringBuilder builder = new();
                    if (commonPermissions != default)
                    {
                        builder.AppendLine(commonPermissions.ToPermissionString());
                    }

                    if (botUniquePermissions != default)
                    {
                        builder.Append("**Bot**: ");
                        builder.AppendLine((permissions.BotPermissions ^ commonPermissions).ToPermissionString());
                    }

                    if (userUniquePermissions != default)
                    {
                        builder.Append("**User**: ");
                        builder.AppendLine(permissions.UserPermissions.ToPermissionString());
                    }

                    embed.AddField("Required Permissions", builder.ToString());
                }

                embed.AddField("Usage", command.GetUsage(context.Arguments.Values.First()!.ToString()!.ToLowerInvariant()));
                foreach (CommandParameter parameter in command.Parameters)
                {
                    embed.AddField($"{parameter.Name.Titleize()} - {context.Extension.GetProcessor<TextCommandProcessor>().Converters[GetConverterFriendlyBaseType(parameter.Type)].ReadableName}", HelpCommandDocumentationMapperEventHandlers.CommandParameterDocumentation.TryGetValue(parameter, out string? parameterDocumentation) ? parameterDocumentation : "No description provided.");
                }

                embed.WithImageUrl("https://files.forsaken-borders.net/transparent.png");
                embed.WithFooter("<> = required, [] = optional");
            }

            return new DiscordMessageBuilder().AddEmbed(embed);
        }

        private static Command? GetCommand(IEnumerable<Command> commands, string name)
        {
            string commandName;
            int spaceIndex = -1;
            do
            {
                spaceIndex = name.IndexOf(' ', spaceIndex + 1);
                commandName = spaceIndex == -1 ? name : name[..spaceIndex];
                commandName = commandName.Underscore();
                foreach (Command command in commands)
                {
                    if (command.Name.Equals(commandName, StringComparison.OrdinalIgnoreCase))
                    {
                        return spaceIndex == -1 ? command : GetCommand(command.Subcommands, name[(spaceIndex + 1)..]);
                    }
                }

                // Search aliases
                foreach (Command command in commands)
                {
                    foreach (Attribute attribute in command.Attributes)
                    {
                        if (attribute is not TextAliasAttribute aliasAttribute)
                        {
                            continue;
                        }

                        if (aliasAttribute.Aliases.Any(alias => alias.Equals(commandName, StringComparison.OrdinalIgnoreCase)))
                        {
                            return spaceIndex == -1 ? command : GetCommand(command.Subcommands, name[(spaceIndex + 1)..]);
                        }
                    }
                }
            } while (spaceIndex != -1);

            return null;
        }

        private static string GetUsage(this Command command, string alias)
        {
            StringBuilder builder = new();
            builder.AppendLine("```ansi");
            builder.Append('/');
            builder.Append(Formatter.Colorize(alias, AnsiColor.Cyan));
            foreach (CommandParameter parameter in command.Parameters)
            {
                if (!parameter.DefaultValue.HasValue)
                {
                    builder.Append(Formatter.Colorize(" <", AnsiColor.LightGray));
                    builder.Append(Formatter.Colorize(parameter.Name.Underscore(), AnsiColor.Magenta));
                    builder.Append(Formatter.Colorize(">", AnsiColor.LightGray));
                }
                else if (parameter.DefaultValue.Value != (parameter.Type.IsValueType ? Activator.CreateInstance(parameter.Type) : null))
                {
                    builder.Append(Formatter.Colorize(" [", AnsiColor.Yellow));
                    builder.Append(Formatter.Colorize(parameter.Name.Underscore(), AnsiColor.Magenta));
                    builder.Append(Formatter.Colorize($" = ", AnsiColor.LightGray));
                    builder.Append(Formatter.Colorize($"\"{parameter.DefaultValue.Value}\"", AnsiColor.Cyan));
                    builder.Append(Formatter.Colorize("]", AnsiColor.Yellow));
                }
                else
                {
                    builder.Append(Formatter.Colorize(" [", AnsiColor.Yellow));
                    builder.Append(Formatter.Colorize(parameter.Name.Underscore(), AnsiColor.Magenta));
                    builder.Append(Formatter.Colorize("]", AnsiColor.Yellow));
                }
            }

            builder.Append("```");
            return builder.ToString();
        }

        private static int CountCommands(this Command command)
        {
            int count = 0;
            if (command.Method is not null)
            {
                count++;
            }

            foreach (Command subcommand in command.Subcommands)
            {
                count += CountCommands(subcommand);
            }

            return count;
        }

        private static Type GetConverterFriendlyBaseType(Type type)
        {
            ArgumentNullException.ThrowIfNull(type, nameof(type));

            if (type.IsEnum)
            {
                return typeof(Enum);
            }
            else if (type.IsArray)
            {
                return type.GetElementType()!;
            }

            return Nullable.GetUnderlyingType(type) ?? type;
        }
    }
}
