using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Commands;
using DSharpPlus.Commands.ArgumentModifiers;
using DSharpPlus.Commands.ContextChecks;
using DSharpPlus.Commands.Converters;
using DSharpPlus.Commands.Processors.TextCommands;
using DSharpPlus.Commands.Trees;
using DSharpPlus.Commands.Trees.Metadata;
using DSharpPlus.Entities;
using Humanizer;
using OoLunar.Tomoe.Events.Handlers;
using OoLunar.Tomoe.Interactivity.Moments.Pagination;

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
        public static async ValueTask ExecuteAsync(CommandContext context, [RemainingText] string? command = null)
        {
            if (string.IsNullOrWhiteSpace(command))
            {
                await context.PaginateAsync(GenerateGeneralizedHelpPages(context.Extension.Commands));
            }
            else if (!context.Extension.GetProcessor<TextCommandProcessor>().TryGetCommand(command, context.Guild?.Id ?? 0, out int _, out Command? foundCommand))
            {
                await context.RespondAsync($"Command {command} not found.");
            }
            else if (foundCommand.Subcommands.Count > 0)
            {
                await context.PaginateAsync(GenerateGroupHelpMessage(context, await context.GetCultureAsync(), foundCommand));
            }
            else
            {
                await context.RespondAsync(GenerateSingleHelpMessage(context, foundCommand));
            }
        }

        private static IEnumerable<Page> GenerateGeneralizedHelpPages(IReadOnlyDictionary<string, Command> commands)
        {
            DiscordEmbedBuilder embed = new();
            foreach ((Command command, int index) in commands.Values.OrderBy(x => x.Name).Select((x, i) => (x, i)))
            {
                embed.AddField(command.Name.Titleize(), HelpCommandDocumentationMapperEventHandler.CommandDocumentation.TryGetValue(command, out string? documentation)
                    ? documentation.ReplaceLineEndings("").Replace("  ", " ")
                    : "No description provided."
                );

                if (embed.Fields.Count == 6 || index == commands.Count)
                {
                    embed
                        .WithTitle("Commands")
                        .WithColor(0x6b73db)
                        .WithFooter($"{index - 4}-{Math.Min(index + 1, commands.Count)}/{commands.Count} Top Level Commands");

                    DiscordMessageBuilder messageBuilder = new DiscordMessageBuilder()
                        .AddEmbed(embed)
                        .WithContent($"A total of {commands.Count:N0} commands were found. Use `help <command>` for more information on any of them.");

                    yield return new(messageBuilder, description: embed.Fields.Select(x => x.Name).Humanize());
                    embed = new();
                }
            }
        }

        private static IEnumerable<Page> GenerateGroupHelpMessage(CommandContext context, CultureInfo cultureInfo, Command command)
        {
            DiscordEmbedBuilder embed = new();
            embed.WithTitle($"Help Command: `{command.FullName.Titleize()}`");
            embed.WithDescription(HelpCommandDocumentationMapperEventHandler.CommandDocumentation.TryGetValue(command, out string? documentation) ? documentation : "No description provided.");

            Command? groupCommand = command.Subcommands.FirstOrDefault(x => x.Attributes.Any(x => x is DefaultGroupCommandAttribute));
            if (groupCommand is not null)
            {
                embed.AddField("Usage", groupCommand.GetUsage(context.Arguments.Values.First()!.ToString()!.ToLower(cultureInfo)));
            }

            yield return new Page(new DiscordMessageBuilder().AddEmbed(embed));

            DiscordEmbedBuilder subEmbed = new();
            foreach ((Command subcommand, int index) in command.Subcommands.OrderBy(x => x.Name).Select((x, i) => (x, i)))
            {
                subEmbed
                    .WithTitle(subcommand.FullName.Titleize())
                    .WithDescription(HelpCommandDocumentationMapperEventHandler.CommandDocumentation.TryGetValue(subcommand, out documentation) ? documentation : "No description provided.")
                    .WithColor(0x6b73db)
                    .AddField("Usage", subcommand.GetUsage())
                    .WithFooter($"Subcommand {index + 1}/{command.Subcommands.Count}");

                yield return new Page(new DiscordMessageBuilder().AddEmbed(subEmbed), subcommand.Name.Titleize(), documentation);
                subEmbed = new();
            }
        }

        private static DiscordMessageBuilder GenerateSingleHelpMessage(CommandContext context, Command command)
        {
            DiscordEmbedBuilder embed = new();
            embed.WithTitle($"Help Command: `{command.FullName.Titleize()}`");
            embed.WithDescription(HelpCommandDocumentationMapperEventHandler.CommandDocumentation.TryGetValue(command, out string? documentation) ? documentation : "No description provided.");
            if (command.Attributes.FirstOrDefault(x => x is RequirePermissionsAttribute) is RequirePermissionsAttribute permissions)
            {
                DiscordPermissions commonPermissions = permissions.BotPermissions & permissions.UserPermissions;
                DiscordPermissions botUniquePermissions = permissions.BotPermissions ^ commonPermissions;
                DiscordPermissions userUniquePermissions = permissions.UserPermissions ^ commonPermissions;
                StringBuilder builder = new();
                if (commonPermissions != default)
                {
                    builder.AppendLine(commonPermissions.EnumeratePermissions().Select(x => x.ToStringFast()).Humanize());
                }

                if (botUniquePermissions != default)
                {
                    builder.Append("**Bot**: ");
                    builder.AppendLine((permissions.BotPermissions ^ commonPermissions).EnumeratePermissions().Select(x => x.ToStringFast()).Humanize());
                }

                if (userUniquePermissions != default)
                {
                    builder.Append("**User**: ");
                    builder.AppendLine(permissions.UserPermissions.EnumeratePermissions().Select(x => x.ToStringFast()).Humanize());
                }

                embed.AddField("Required Permissions", builder.ToString());
            }

            embed.AddField("Usage", command.GetUsage(context.Arguments.Values.First()!.ToString()!.ToLowerInvariant()));
            foreach (CommandParameter parameter in command.Parameters)
            {
                embed.AddField($"{parameter.Name.Titleize()} - {context.Extension.GetProcessor<TextCommandProcessor>().Converters[IArgumentConverter.GetConverterFriendlyBaseType(parameter.Type)].ReadableName}", HelpCommandDocumentationMapperEventHandler.CommandParameterDocumentation.TryGetValue(parameter, out string? parameterDocumentation) ? parameterDocumentation : "No description provided.");
            }

            embed.WithImageUrl("https://files.forsaken-borders.net/transparent.png");
            embed.WithFooter("<> = required, [] = optional");

            return new DiscordMessageBuilder().AddEmbed(embed);
        }

        private static string GetUsage(this Command command, string? alias = null)
        {
            StringBuilder builder = new();
            builder.AppendLine("```ansi");
            builder.Append('/');
            builder.Append(Formatter.Colorize(alias ?? command.FullName.ToLowerInvariant(), AnsiColor.Cyan));
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
    }
}
