using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using Humanizer;

namespace Tomoe.Commands.Public
{
	public class Help : BaseCommandModule
	{
		[Command("help"), Description("Sends the help menu!")]
		public async Task Overload(CommandContext context)
		{
			Assembly entryAssembly = Assembly.GetEntryAssembly();
			string[] sections = entryAssembly.GetTypes().Where(type => type.FullName.StartsWith("Tomoe.Commands", true, CultureInfo.InvariantCulture)).Select(type => type.FullName.Split('.')[2]).Distinct().ToArray();
			List<Page> embedSections = new();
			foreach (string section in sections)
			{
				if (section.ToLowerInvariant() == "listeners") continue;
				DiscordEmbedBuilder embed = new DiscordEmbedBuilder().GenerateDefaultEmbed(context, $"{section} Commands");
				embed.Description = "Note that all commands are [cAsE iNsEnSiTiVe](https://en.wiktionary.org/wiki/case_insensitive#Adjective). All commands have PascalCase and snake_case varients.";
				foreach (Command command in context.CommandsNext.RegisteredCommands.Values.Distinct())
					if (command.Module.ModuleType.FullName.StartsWith($"Tomoe.Commands.{section}", true, CultureInfo.InvariantCulture) && !command.IsHidden)
						_ = embed.AddField(command.Name.Titleize(), command.Description.Truncate(50, "...") ?? "No description was found. Open up a [Github Issue](https://github.com/OoLunar/Tomoe/issues/new?assignees=OoLunar&labels=bug%2C+documentation%2C+enhancement&template=missing-command-description.md&title=%5BMissing+Command+Description%5D) about this please!", true);
				embedSections.Add(new Page($"{context.User.Mention}:", embed));
			}
			InteractivityExtension interactivity = context.Client.GetInteractivity();
			try { await interactivity.SendPaginatedMessageAsync(context.Channel, context.User, embedSections); }
			catch (NotFoundException) { }
		}

		[Command("help")]
		public async Task Overload(CommandContext context, [Description("Which command to search for."), RemainingText] string commandName)
		{
			commandName = commandName.ToLowerInvariant();
			DiscordEmbedBuilder mainHelpEmbed = new DiscordEmbedBuilder().GenerateDefaultEmbed(context);
			Assembly entryAssembly = Assembly.GetEntryAssembly();
			string[] subcommandName = null;
			if (commandName.Contains(' ')) subcommandName = commandName.Split(' ').Skip(1).ToArray();
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

			CommandGroup commandGroup = command as CommandGroup;

			if (subcommandName == null) // We're looking for just a whole command
			{
				mainHelpEmbed.Title = $"{command.Name.Titleize()} - Help";
				mainHelpEmbed.Description = command.Description ?? "No description was found. Open up a [Github Issue](https://github.com/OoLunar/Tomoe/issues/new?assignees=OoLunar&labels=bug%2C+documentation%2C+enhancement&template=missing-command-description.md&title=%5BMissing+Command+Description%5D) about this please!";
				string aliases = string.Join(", ", command.Aliases.Select(alias => alias));
				_ = mainHelpEmbed.AddField("Aliases", aliases == string.Empty ? "None" : aliases, true);
				_ = mainHelpEmbed.AddField("Section", entryAssembly.GetTypes().First(type => type.FullName == command.Module.ModuleType.FullName).FullName.Split('.')[2], true);
				List<Page> overloads = new();

				if (commandGroup != null) _ = mainHelpEmbed.AddField("Subcommands", string.Join(", ", commandGroup.Children.Select(child => child.Name)));

				StringBuilder commandUsage = new();
				if (command.Overloads.Count == 0) // No overload, put the usage on the main embed.
				{
					_ = commandUsage.Append($"`>>{command.QualifiedName}`");
					_ = mainHelpEmbed.AddField("Usage", commandUsage.ToString());
					overloads.Add(new Page(null, mainHelpEmbed));
				}
				else if (command.Overloads.Count == 1) // One overload, put the usage on the main embed too.
				{
					_ = commandUsage.Append($"`>>{command.QualifiedName}");
					foreach (CommandArgument argument in command.Overloads[0].Arguments)
					{
						_ = commandUsage.Append(argument.IsOptional ? $" [{argument.Name}]" : $" {argument.Name}");
						_ = mainHelpEmbed.AddField(argument.IsOptional ? $"Optional Argument: {argument.Name}" : $"Required Arguement: {argument.Name}", $"{argument.Description ?? "No argument description was found. Open up a [Github Issue](https://github.com/OoLunar/Tomoe/issues/new?assignees=OoLunar&labels=bug%2C+documentation%2C+enhancement&template=missing-command-argument-description.md&title=%5BMissing+Command+Arugment+Description%5D) about it please!"}\nDefault Value: {argument.DefaultValue ?? "None."}\nType: {argument.Type.Name.Humanize()}");
					}
					_ = commandUsage.Append('`');
					_ = mainHelpEmbed.AddField("Usage", commandUsage.ToString());
					overloads.Add(new Page(null, mainHelpEmbed));
				}
				else
				{
					overloads.Add(new(null, mainHelpEmbed));
					overloads.AddRange(GetCommandOverloads(context, command).Select(embed => new Page(null, embed)));
				}

				if (overloads.Count == 1) _ = Program.SendMessage(context, null, overloads[0].Embed);
				else
				{
					InteractivityExtension interactivity = context.Client.GetInteractivity();
					await interactivity.SendPaginatedMessageAsync(context.Channel, context.User, overloads);
				}
			}
			else // We're looking for a sub command
			{
				if (commandGroup == null)
				{
					_ = await Program.SendMessage(context, $"\"{commandName}\" not found.");
					return;
				}
				Command realSubCommand = commandGroup.Children.First(child => child.Name.ToLower() == subcommandName[0]);
				mainHelpEmbed.Title = $"{subcommandName.Last().Titleize()} - {command.Name.Titleize()} - Help";
				mainHelpEmbed.Description = command.Description ?? "No description was found. Open up a [Github Issue](https://github.com/OoLunar/Tomoe/issues/new?assignees=OoLunar&labels=bug%2C+documentation%2C+enhancement&template=missing-command-description.md&title=%5BMissing+Command+Description%5D) about this please!";
				string aliases = string.Join(", ", command.Aliases.Select(alias => alias));
				_ = mainHelpEmbed.AddField("Aliases", aliases == string.Empty ? "None" : aliases, true);
				_ = mainHelpEmbed.AddField("Section", entryAssembly.GetTypes().First(type => type.FullName == command.Module.ModuleType.FullName).FullName.Split('.')[2], true);
				List<Page> overloads = new();
				string commandUsage;
				if (realSubCommand.Overloads.Count == 0) // No overload, put the usage on the main embed.
				{
					commandUsage = $"`>>{command.QualifiedName} {realSubCommand.Name}`";
					_ = mainHelpEmbed.AddField("Usage", commandUsage);
					overloads.Add(new(null, mainHelpEmbed));
				}
				else if (realSubCommand.Overloads.Count == 1) // One overload, put the usage on the main embed too.
				{
					commandUsage = $"`>>{command.QualifiedName} {realSubCommand.Name}";
					foreach (CommandArgument argument in realSubCommand.Overloads[0].Arguments)
					{
						commandUsage += argument.IsOptional ? $" [{argument.Name}]" : $" {argument.Name}";
						_ = mainHelpEmbed.AddField(argument.IsOptional ? $"Optional Argument: {argument.Name}" : $"Required Arguement: {argument.Name}", $"{argument.Description ?? "No argument description was found. Open up a [Github Issue](https://github.com/OoLunar/Tomoe/issues/new?assignees=OoLunar&labels=bug%2C+documentation%2C+enhancement&template=missing-command-argument-description.md&title=%5BMissing+Command+Arugment+Description%5D) about it please!"}\nDefault Value: {argument.DefaultValue ?? "None."}\nType: {argument.Type.Name.Humanize()}");
					}
					commandUsage += '`';
					_ = mainHelpEmbed.AddField("Usage", commandUsage);
					overloads.Add(new(null, mainHelpEmbed));
				}
				else
				{
					overloads.Add(new(null, mainHelpEmbed));
					overloads.AddRange(GetCommandOverloads(context, realSubCommand).Select(embed => new Page(null, embed)));
				}
				InteractivityExtension interactivity = context.Client.GetInteractivity();
				await interactivity.SendPaginatedMessageAsync(context.Channel, context.User, overloads);
			}
		}

		private static DiscordEmbedBuilder[] GetCommandOverloads(CommandContext context, Command command)
		{

			List<DiscordEmbedBuilder> overloads = new();
			List<CommandOverload> commandOverloads = command.Overloads.OrderBy(overload => overload.Priority).ThenBy(overload => overload.Arguments.Count).ToList();
			for (int i = 0; i < commandOverloads.Count; i++) // Create the pages
			{
				DiscordEmbedBuilder embed = new DiscordEmbedBuilder().GenerateDefaultEmbed(context, $"{command.Name} - Help");
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
