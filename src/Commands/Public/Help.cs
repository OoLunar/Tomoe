using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using Humanizer;

using Tomoe.Utils;

namespace Tomoe.Commands.Public
{
	public class Help : BaseCommandModule
	{
		private static readonly Logger _logger = new("Commands.Public.Help");

		[Command("Help"), Description("Sends the help menu!")]
		public async Task Overload(CommandContext context)
		{
			_logger.Debug($"Executing in channel {context.Channel.Id} on guild {context.Guild.Id}");
			Assembly entryAssembly = Assembly.GetEntryAssembly();
			string[] sections = entryAssembly.GetTypes().Where(type => type.FullName.StartsWith("Tomoe.Commands", true, CultureInfo.InvariantCulture)).Select(type => type.FullName.Split('.')[2]).Distinct().ToArray();
			List<Page> embedSections = new();
			foreach (string section in sections)
			{
				if (section.ToLowerInvariant() == "listeners") continue;
				_logger.Trace("Creating embed...");
				DiscordEmbedBuilder embed = new DiscordEmbedBuilder().GenerateDefaultEmbed(context, $"{section} Commands");
				_logger.Trace("Filling out description...");
				embed.Description = "Note that all commands are [cAsE iNsEnSiTiVe](https://en.wiktionary.org/wiki/case_insensitive#Adjective). All commands have PascalCase and snake_case varients.";
				foreach (Command command in context.CommandsNext.RegisteredCommands.Values.Distinct())
					if (command.Module.ModuleType.FullName.StartsWith($"Tomoe.Commands.{section}", true, CultureInfo.InvariantCulture) && !command.IsHidden)
						_ = embed.AddField(command.Name.Titleize(), command.Description.Truncate(50, "...") ?? "No description was found. Open up a [Github Issue](https://github.com/OoLunar/Tomoe/issues/new?assignees=OoLunar&labels=bug%2C+documentation%2C+enhancement&template=missing-command-description.md&title=%5BMissing+Command+Description%5D) about this please!", true);
				embedSections.Add(new Page($"{context.User.Mention}:", embed));
			}
			_logger.Trace("Sending embed...");
			InteractivityExtension interactivity = context.Client.GetInteractivity();
			await interactivity.SendPaginatedMessageAsync(context.Channel, context.User, embedSections);
			_logger.Trace("Embed sent!");
		}

		[Command("Help")]
		public async Task Overload(CommandContext context, [Description("Which command to search for."), RemainingText] string commandName)
		{
			_logger.Debug($"Executing in channel {context.Channel.Id} on guild {context.Guild.Id}");
			commandName = commandName.ToLowerInvariant();
			DiscordEmbedBuilder mainHelpEmbed = new DiscordEmbedBuilder().GenerateDefaultEmbed(context, null);
			Assembly entryAssembly = Assembly.GetEntryAssembly();
			string[] subcommand = commandName.Split(' ').Skip(1).ToArray();
			commandName = commandName.Split(' ')[0];
			Command command;
			try
			{
				command = context.CommandsNext.RegisteredCommands.Values.First(command => command.Name.ToLowerInvariant() == commandName || command.Aliases.Contains(commandName));
			}
			catch (InvalidOperationException)
			{
				_ = await Program.SendMessage(context, $"\"{commandName}\" not found.");
				return;
			}

			if (subcommand == Array.Empty<string>()) // We're looking for just a whole command
			{
				_logger.Trace("Command is not a subcommand...");
				mainHelpEmbed.Title = $"{command.Name.Titleize()} - Help";
				mainHelpEmbed.Description = command.Description ?? "No description was found. Open up a [Github Issue](https://github.com/OoLunar/Tomoe/issues/new?assignees=OoLunar&labels=bug%2C+documentation%2C+enhancement&template=missing-command-description.md&title=%5BMissing+Command+Description%5D) about this please!";
				string aliases = string.Join(", ", command.Aliases.Select(alias => alias));
				_ = mainHelpEmbed.AddField("Aliases", aliases == string.Empty ? "None" : aliases, true);
				_ = mainHelpEmbed.AddField("Section", entryAssembly.GetTypes().First(type => type.FullName == command.Module.ModuleType.FullName).FullName.Split('.')[2], true);
				List<Page> overloads = new();
				if ((command as CommandGroup) != null)
				{
					CommandGroup commandGroup = command as CommandGroup;
					_ = mainHelpEmbed.AddField("Subcommands", string.Join(", ", commandGroup.Children.Select(child => child.Name)));
				}
				string commandUsage;
				if (command.Overloads.Count == 0) // No overload, put the usage on the main embed.
				{
					commandUsage = $"`>>{command.QualifiedName}`";
					_ = mainHelpEmbed.AddField("Usage", commandUsage);
				}
				else if (command.Overloads.Count == 1) // One overload, put the usage on the main embed too.
				{
					commandUsage = $"`>>{command.QualifiedName}";
					foreach (CommandArgument argument in command.Overloads[0].Arguments)
					{
						commandUsage += argument.IsOptional ? $" [{argument.Name}]" : $" {argument.Name}";
						_ = mainHelpEmbed.AddField(argument.IsOptional ? $"Optional Argument: {argument.Name}" : $"Required Arguement: {argument.Name}", $"{argument.Description ?? "No argument description was found. Open up a [Github Issue](https://github.com/OoLunar/Tomoe/issues/new?assignees=OoLunar&labels=bug%2C+documentation%2C+enhancement&template=missing-command-argument-description.md&title=%5BMissing+Command+Arugment+Description%5D) about it please!"}\nDefault Value: {argument.DefaultValue ?? "None."}\nType: {argument.Type.Name.Humanize()}");
					}
					commandUsage += '`';
					_ = mainHelpEmbed.AddField("Usage", commandUsage);
				}
				else overloads.AddRange(GetCommandOverloads(context, command).Select(embed => new Page(null, embed)));
				_logger.Trace("Getting interactivity...");
				InteractivityExtension interactivity = context.Client.GetInteractivity();
				_logger.Trace("Sending embed...");
				await interactivity.SendPaginatedMessageAsync(context.Channel, context.User, overloads);
				_logger.Trace("Embed sent!");
			}
			else // We're looking for a sub command
			{
				if ((command as CommandGroup) == null)
				{
					_ = await Program.SendMessage(context, $"\"{commandName}\" not found.");
					return;
				}
				CommandGroup commandGroup = command as CommandGroup;
				Command realSubCommand = commandGroup.Children.First(child => child.Name.ToLower() == subcommand[0]);
				_logger.Trace("Command is a subcommand...");
				_logger.Trace("Setting title...");
				mainHelpEmbed.Title = $"{subcommand.Last().Titleize()} - {command.Name.Titleize()} - Help";
				_logger.Trace("Filling out description...");
				mainHelpEmbed.Description = command.Description ?? "No description was found. Open up a [Github Issue](https://github.com/OoLunar/Tomoe/issues/new?assignees=OoLunar&labels=bug%2C+documentation%2C+enhancement&template=missing-command-description.md&title=%5BMissing+Command+Description%5D) about this please!";
				_logger.Trace("Setting aliases...");
				string aliases = string.Join(", ", command.Aliases.Select(alias => alias));
				mainHelpEmbed.AddField("Aliases", aliases == string.Empty ? "None" : aliases, true);
				_logger.Trace("Getting which section using reflection...");
				_ = mainHelpEmbed.AddField("Section", entryAssembly.GetTypes().First(type => type.FullName == command.Module.ModuleType.FullName).FullName.Split('.')[2], true);
				List<Page> overloads = new();
				string commandUsage;
				if (command.Overloads.Count == 0) // No overload, put the usage on the main embed.
				{
					commandUsage = $"`>>{command.QualifiedName}`";
					mainHelpEmbed.AddField("Usage", commandUsage);
				}
				else if (command.Overloads.Count == 1) // One overload, put the usage on the main embed too.
				{
					commandUsage = $"`>>{command.QualifiedName}";
					foreach (CommandArgument argument in command.Overloads[0].Arguments)
					{
						commandUsage += argument.IsOptional ? $" [{argument.Name}]" : $" {argument.Name}";
						mainHelpEmbed.AddField(argument.IsOptional ? $"Optional Argument: {argument.Name}" : $"Required Arguement: {argument.Name}", $"{argument.Description ?? "No argument description was found. Open up a [Github Issue](https://github.com/OoLunar/Tomoe/issues/new?assignees=OoLunar&labels=bug%2C+documentation%2C+enhancement&template=missing-command-argument-description.md&title=%5BMissing+Command+Arugment+Description%5D) about it please!"}\nDefault Value: {argument.DefaultValue ?? "None."}\nType: {argument.Type.Name.Humanize()}");
					}
					commandUsage += '`';
					mainHelpEmbed.AddField("Usage", commandUsage);
				}
				else overloads.AddRange(GetCommandOverloads(context, command).Select(embed => new Page(null, embed)));
				_logger.Trace("Getting interactivity...");
				InteractivityExtension interactivity = context.Client.GetInteractivity();
				_logger.Trace("Sending embed...");
				await interactivity.SendPaginatedMessageAsync(context.Channel, context.User, overloads);
				_logger.Trace("Embed sent!");
			}
		}

		private static DiscordEmbedBuilder[] GetCommandOverloads(CommandContext context, Command command)
		{
			DiscordEmbedBuilder embed = new DiscordEmbedBuilder().GenerateDefaultEmbed(context, null);
			List<DiscordEmbedBuilder> overloads = new();
			switch (command.Overloads.Count)
			{
				default: // Lord knows how many overloads, put each one on their own embed.
					embed.Footer = new() { Text = "Usage can be found on the next page." };
					List<CommandOverload> commandOverloads = command.Overloads.OrderBy(overload => overload.Priority).ThenBy(overload => overload.Arguments.Count).ToList();
					for (int i = 0; i < commandOverloads.Count; i++) // Create the pages
					{
						embed.Title = $"{command.QualifiedName} - Help";
						embed.Description = $"Usage: `>>{command.QualifiedName}";
						if (commandOverloads[i].Arguments.Count == 0) _ = embed.AddField("Arguments", "None");
						else foreach (CommandArgument argument in commandOverloads[i].Arguments)
							{
								embed.Description += argument.IsOptional ? $" [{argument.Name}]" : $" {argument.Name}";
								_ = embed.AddField(argument.IsOptional ? $"(Optional) {argument.Name}" : $"(Required) {argument.Name}", $"{argument.Description ?? "No argument description was found. Open up a [Github Issue](https://github.com/OoLunar/Tomoe/issues/new?assignees=OoLunar&labels=bug%2C+documentation%2C+enhancement&template=missing-command-argument-description.md&title=%5BMissing+Command+Arugment+Description%5D) about it please!"}\nDefault Value: {argument.DefaultValue ?? "None."}\nType: {argument.Type.Name.Humanize()}");
							}
						embed.Description += '`';
						embed.Footer = new()
						{
							Text = $"Overload {i + 1}/{commandOverloads.Count}. Argument names wrapped in brackets [] are optional."
						};
						overloads.Add(embed);
					}
					return overloads.ToArray();
			}
		}
	}
}
