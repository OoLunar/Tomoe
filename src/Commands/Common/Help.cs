using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using Humanizer;

namespace Tomoe.Commands.Common
{
    public class Help : BaseCommandModule
    {
        [Command("help"), Description("Sends the help menu!")]
        public async Task HelpAsync(CommandContext context)
        {
            List<Page> pages = new();
            DiscordEmbedBuilder embedBuilder = new();
            embedBuilder.Color = new DiscordColor("#7b84d1");
            embedBuilder.Author = new()
            {
                Name = context.Guild == null ? context.User.Username : context.Member.DisplayName,
                IconUrl = context.User.AvatarUrl,
                Url = context.User.AvatarUrl
            };

            foreach (string section in GetSections())
            {
                if (section == "listeners")
                {
                    continue;
                }
                embedBuilder.Title = $"Help - {section.Titleize()} Commands";
                embedBuilder.WithFooter("Note that all commands are cAsE iNsEnSiTiVe.");
                List<Command> sectionCommands = GetSectionCommands(context, section);
                foreach (Command command in sectionCommands)
                {
                    if (embedBuilder.Fields.Count >= 6)
                    {
                        pages.Add(new(null, embedBuilder));
                        embedBuilder.ClearFields();
                    }

                    if (!(await command.RunChecksAsync(context, true)).Any())
                    {
                        embedBuilder.AddField(command.QualifiedName, command.Description.Truncate(75, "...") ?? $"No description was found. Open up a {Formatter.MaskedUrl("Github Issue", new("https://github.com/OoLunar/Tomoe/issues/new?assignees=OoLunar&labels=bug%2C+documentation%2C+enhancement&template=missing-command-description.md&title=%5BMissing+Command+Description%5D"))} about this please!", true);
                    }
                }

                if (embedBuilder.Fields.Any())
                {
                    pages.Add(new(null, embedBuilder));
                    embedBuilder.ClearFields();
                }
            }
            await context.Client.GetInteractivity().SendPaginatedMessageAsync(context.Channel, context.User, pages);
        }

        [Command("help")]
        public async Task HelpAsync(CommandContext context, [Description("Which command to search for."), RemainingText] string commandName)
        {
            commandName = commandName.ToLowerInvariant();
            Command? command = context.CommandsNext.RegisteredCommands.Values.FirstOrDefault(command => command.Name == commandName.Split(' ').First() || command.Aliases.Contains(commandName.Split(' ').First()));
            if (command == null)
            {
                await context.RespondAsync($"Command {Formatter.InlineCode(Formatter.Sanitize(commandName))} not found!");
                return;
            }

            CommandGroup? groupCommand = command as CommandGroup;
            while (commandName.Contains(' '))
            {
                if (groupCommand != null)
                {
                    commandName = commandName.Split(' ')[1];
                    command = groupCommand.Children.FirstOrDefault(child => child.Name == commandName);
                    if (command != null)
                    {
                        continue;
                    }
                }

                if (commandName.Contains(' '))
                {
                    await context.RespondAsync($"Subcommand {Formatter.InlineCode(Formatter.Sanitize(commandName))} not found!");
                    return;
                }
            }

            command ??= groupCommand;

            List<Page> pages = new();
            DiscordEmbedBuilder embedBuilder = new();
            embedBuilder.Color = new DiscordColor("#7b84d1");
            embedBuilder.Author = new()
            {
                Name = context.Guild == null ? context.User.Username : context.Member.DisplayName,
                IconUrl = context.User.AvatarUrl,
                Url = context.User.AvatarUrl
            };

            embedBuilder.Title = context.Prefix + command!.QualifiedName;
            embedBuilder.Description = command.Description;
            if (command.Overloads.Count == 0) // No overload, put the usage on the main embed.
            {
                embedBuilder.AddField("Command Usage", $"`>>{command.QualifiedName}`");
            }
            else if (command.Overloads.Count == 1) // One overload, put the usage on the main embed too.
            {
                StringBuilder commandUsage = new();
                commandUsage.Append(CultureInfo.InvariantCulture, $"`>>{command.QualifiedName}");
                foreach (CommandArgument argument in command.Overloads[0].Arguments)
                {
                    commandUsage.Append(argument.IsOptional ? $" [{argument.Name}]" : $" {argument.Name}");
                    embedBuilder.AddField(argument.IsOptional ? $"Optional Argument: {argument.Name}" : $"Required Argument: {argument.Name}", $"{argument.Description ?? $"No argument description was found. Open up a {Formatter.MaskedUrl("Github Issue", new("https://github.com/OoLunar/Tomoe/issues/new?assignees=OoLunar&labels=bug%2C+documentation%2C+enhancement&template=missing-command-argument-description.md&title=%5BMissing+Command+Arugment+Description%5D"))} about it please!"}\nDefault Value: {argument.DefaultValue ?? "None."}\nType: {context.CommandsNext.GetUserFriendlyTypeName(argument.Type)}", false);
                }
                commandUsage.Append('`');
                embedBuilder.AddField("Command Usage", commandUsage.ToString());
            }
            else
            {
                pages.AddRange(GetCommandOverloads(context, command).Select(embed => new Page(null, embed)));
                embedBuilder.AddField("Usage", "Usage can be found on the next page.");
            }
            if (groupCommand != null)
            {
                embedBuilder.AddField("Subcommands", string.Join(", ", groupCommand.Children.Distinct().Select(child => child.Name)));
            }

            embedBuilder.AddField("Section", GetSection(command), true);
            string aliases = string.Join(", ", command.Aliases);
            if (!string.IsNullOrEmpty(aliases))
            {
                embedBuilder.AddField("Aliases", aliases, true);
            }
            pages = pages.Prepend(new(null, embedBuilder)).ToList();

            if (pages.Count == 1)
            {
                await context.RespondAsync(null, pages[0].Embed);
            }
            else
            {
                InteractivityExtension interactivity = context.Client.GetInteractivity();
                await interactivity.SendPaginatedMessageAsync(context.Channel, context.User, pages);
            }
        }

        private static string[] GetSections() => typeof(Program).Assembly.GetTypes().Where(type => type.FullName!.StartsWith("Tomoe.Commands", true, CultureInfo.InvariantCulture)).Select(type => type.FullName!.Split('.')[2].ToLowerInvariant()).Distinct().ToArray();
        private static List<Command> GetSectionCommands(CommandContext context, string section) => context.CommandsNext.RegisteredCommands.Values.Distinct().Where(command => command.Module.ModuleType.FullName!.StartsWith($"Tomoe.Commands.{section.Titleize()}", true, CultureInfo.InvariantCulture) && !command.IsHidden).OrderBy(command => command.QualifiedName).ToList();
        private static string GetSection(Command command) => typeof(Program).Assembly.GetTypes().First(type => type.FullName! == command.Module.ModuleType.FullName!).FullName!.Split('.')[2];

        private static List<DiscordEmbedBuilder> GetCommandOverloads(CommandContext context, Command command)
        {

            List<DiscordEmbedBuilder> overloads = new();
            List<CommandOverload> commandOverloads = command.Overloads.OrderBy(overload => overload.Priority).ThenBy(overload => overload.Arguments.Count).ToList();
            for (int i = 0; i < commandOverloads.Count; i++) // Create the pages
            {
                DiscordEmbedBuilder embedBuilder = new();
                embedBuilder.Title = context.Prefix + command.QualifiedName;
                embedBuilder.Description = command.Description + $"\nUsage: `>>{command.QualifiedName}";
                embedBuilder.Color = new DiscordColor("#7b84d1");
                embedBuilder.Author = new()
                {
                    Name = context.Guild == null ? context.User.Username : context.Member.DisplayName,
                    IconUrl = context.User.AvatarUrl,
                    Url = context.User.AvatarUrl
                };

                if (commandOverloads[i].Arguments.Count == 0)
                {
                    embedBuilder.AddField("Arguments", "None");
                }
                else
                {
                    foreach (CommandArgument argument in commandOverloads[i].Arguments)
                    {
                        embedBuilder.Description += argument.IsOptional ? $" [{argument.Name}]" : $" {argument.Name}";
                        embedBuilder.AddField(argument.IsOptional ? $"(Optional) {argument.Name}" : $"(Required) {argument.Name}", $"{argument.Description ?? $"No argument description was found. Open up a {Formatter.MaskedUrl("Github Issue", new("https://github.com/OoLunar/Tomoe/issues/new?assignees=OoLunar&labels=bug%2C+documentation%2C+enhancement&template=missing-command-argument-description.md&title=%5BMissing+Command+Arugment+Description%5D"))} about it please!"}\nDefault Value: {argument.DefaultValue ?? "None."}\nType: {context.CommandsNext.GetUserFriendlyTypeName(argument.Type)}", false);
                    }
                }

                embedBuilder.Description += '`';
                embedBuilder.Footer = new()
                {
                    Text = $"Overload {i + 1}/{commandOverloads.Count}. Argument names wrapped in brackets [] are optional."
                };
                overloads.Add(embedBuilder);
            }
            return overloads;
        }
    }
}