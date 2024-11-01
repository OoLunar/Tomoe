using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.SlashCommands.ArgumentModifiers;
using DSharpPlus.Commands.Trees;
using DSharpPlus.Entities;
using Humanizer;

namespace OoLunar.Tomoe.AutoCompleteProviders
{
    public sealed class CommandAutoCompleteProvider : IAutoCompleteProvider
    {
        public ValueTask<IEnumerable<DiscordAutoCompleteChoice>> AutoCompleteAsync(AutoCompleteContext context)
        {
            if (string.IsNullOrWhiteSpace(context.UserInput))
            {
                return ValueTask.FromResult(context.Extension.Commands.Values
                    .OrderBy(command => command.FullName)
                    .Take(25)
                    .Select(command => new DiscordAutoCompleteChoice(command.FullName.Humanize(LetterCasing.Title), command.FullName)));
            }

            List<DiscordAutoCompleteChoice> choices = [];
            foreach (Command command in context.Extension.Commands.Values.SelectMany(command => command.Flatten()))
            {
                if (command.FullName.Contains(context.UserInput, StringComparison.OrdinalIgnoreCase))
                {
                    choices.Add(new DiscordAutoCompleteChoice(command.FullName.Humanize(LetterCasing.Title), command.FullName));
                    if (choices.Count >= 25)
                    {
                        break;
                    }
                }
            }

            // Sort by if the name starts with the user input, then alphabetically.
            choices.Sort((a, b) =>
                a.Name.StartsWith(context.UserInput, StringComparison.OrdinalIgnoreCase) == b.Name.StartsWith(context.UserInput, StringComparison.OrdinalIgnoreCase)
                    ? string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase)
                    : a.Name.StartsWith(context.UserInput, StringComparison.OrdinalIgnoreCase) ? -1 : 1);

            return ValueTask.FromResult<IEnumerable<DiscordAutoCompleteChoice>>(choices);
        }
    }
}
