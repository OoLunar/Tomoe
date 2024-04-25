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
    public static class HelpCommand
    {
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

        public static DiscordMessageBuilder GetHelpMessage(CommandContext context)
        {
            StringBuilder stringBuilder = new();
            foreach (Command command in context.Extension.Commands.Values.OrderBy(x => x.Name))
            {
                stringBuilder.AppendLine($"`{command.Name}`: {(HelpCommandDocumentationMapperEventHandlers.CommandDocumentation.TryGetValue(command, out string? documentation) ? documentation : "No description provided.")}");
            }

            return new DiscordMessageBuilder().WithContent($"A total of {context.Extension.Commands.Values.Select(CountCommands).Sum():N0} commands were found. Use `help <command>` for more information on any of them.").AddEmbed(new DiscordEmbedBuilder().WithTitle("Commands").WithDescription(stringBuilder.ToString()));
        }

        public static DiscordMessageBuilder GetHelpMessage(CommandContext context, Command command)
        {
            DiscordEmbedBuilder embed = new();
            embed.WithTitle($"Help Command: `{command.Name}`");
            embed.WithDescription(HelpCommandDocumentationMapperEventHandlers.CommandDocumentation.TryGetValue(command, out string? documentation) ? documentation : "No description provided.");
            if (command.Subcommands.Count > 0)
            {
                foreach (Command subcommand in command.Subcommands.OrderBy(x => x.Name))
                {
                    embed.AddField(subcommand.Name, HelpCommandDocumentationMapperEventHandlers.CommandDocumentation.TryGetValue(subcommand, out string? subcommandDocumentation) ? subcommandDocumentation : "No description provided.");
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

                embed.AddField("Usage", command.GetUsage());
                foreach (CommandParameter parameter in command.Parameters)
                {
                    embed.AddField($"{parameter.Name.Titleize()} - {context.Extension.GetProcessor<TextCommandProcessor>().Converters[GetConverterFriendlyBaseType(parameter.Type)].ReadableName}", HelpCommandDocumentationMapperEventHandlers.CommandParameterDocumentation.TryGetValue(parameter, out string? parameterDocumentation) ? parameterDocumentation : "No description provided.");
                }

                embed.WithImageUrl("https://files.forsaken-borders.net/transparent.png?cache-bust=1");
                embed.WithFooter("<> = required, [] = optional");
            }

            return new DiscordMessageBuilder().AddEmbed(embed);
        }

        private static Command? GetCommand(IEnumerable<Command> commands, string name)
        {
            int index = name.IndexOf(' ');
            string commandName = index == -1 ? name : name[..index];
            foreach (Command command in commands)
            {
                if (command.Name.Equals(commandName, StringComparison.OrdinalIgnoreCase))
                {
                    return index == -1 ? command : GetCommand(command.Subcommands, name[(index + 1)..]);
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
                        return index == -1 ? command : GetCommand(command.Subcommands, name[(index + 1)..]);
                    }
                }
            }

            return null;
        }

        private static string GetUsage(this Command command)
        {
            StringBuilder builder = new();
            builder.AppendLine("```ansi");
            builder.Append('/');
            builder.Append(Formatter.Colorize(command.FullName, AnsiColor.Cyan));
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
