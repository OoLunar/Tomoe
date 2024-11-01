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
                return ValueTask.FromResult<IEnumerable<DiscordAutoCompleteChoice>>(context.Extension.Commands.Values
                    .Take(25)
                    .Select(command => new DiscordAutoCompleteChoice(command.FullName.Humanize(LetterCasing.Title), command.FullName))
                    .OrderBy(command => command.Name));
            }

            List<DiscordAutoCompleteChoice> choices = [];
            foreach (Command command in context.Extension.Commands.Values.SelectMany(command => command.Flatten()))
            {
                if (command.FullName.Contains(context.UserInput, StringComparison.OrdinalIgnoreCase))
                {
                    choices.Add(new DiscordAutoCompleteChoice(command.FullName.Humanize(LetterCasing.Title), command.FullName));
                }
            }

            return ValueTask.FromResult<IEnumerable<DiscordAutoCompleteChoice>>(choices
                .OrderBy(x => x.Name.StartsWith(context.UserInput, StringComparison.OrdinalIgnoreCase) ? 0 : 1)
                .ThenBy(x => x.Name));
        }
    }
}
