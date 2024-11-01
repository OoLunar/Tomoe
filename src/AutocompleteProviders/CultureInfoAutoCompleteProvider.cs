using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.SlashCommands.ArgumentModifiers;
using DSharpPlus.Entities;

namespace OoLunar.Tomoe.AutoCompleteProviders
{
    public sealed class CultureInfoAutoCompleteProvider : IAutoCompleteProvider
    {
        private static readonly CultureInfo[] _cultures;
        private static readonly FrozenSet<DiscordAutoCompleteChoice> _defaultCultureList;
        private static readonly FrozenDictionary<CultureInfo, string> _cultureInfoDisplayNames;

        static CultureInfoAutoCompleteProvider()
        {
            _cultures = CultureInfo.GetCultures(CultureTypes.AllCultures);
            Array.Sort(_cultures, (x, y) => string.Compare(x.DisplayName, y.DisplayName, StringComparison.Ordinal));

            List<DiscordAutoCompleteChoice> choices = [];
            Dictionary<CultureInfo, string> cultureInfoDisplayNames = [];
            for (int i = 0; i < _cultures.Length; i++)
            {
                string displayName = $"{_cultures[i].DisplayName} ({_cultures[i].IetfLanguageTag})";
                cultureInfoDisplayNames[_cultures[i]] = displayName;

                // Only add the first 25 cultures to the default list
                // which is returned when the user input is empty
                if (i < 25)
                {
                    choices.Add(new DiscordAutoCompleteChoice(displayName, _cultures[i].IetfLanguageTag));
                }
            }

            _defaultCultureList = choices.ToFrozenSet();
            _cultureInfoDisplayNames = cultureInfoDisplayNames.ToFrozenDictionary();
        }

        public ValueTask<IEnumerable<DiscordAutoCompleteChoice>> AutoCompleteAsync(AutoCompleteContext context)
        {
            if (string.IsNullOrWhiteSpace(context.UserInput))
            {
                return ValueTask.FromResult<IEnumerable<DiscordAutoCompleteChoice>>(_defaultCultureList);
            }

            List<DiscordAutoCompleteChoice> choices = [];
            foreach (CultureInfo cultureInfo in _cultures)
            {
                if (choices.Count >= 25)
                {
                    break;
                }
                else if (cultureInfo.DisplayName.Contains(context.UserInput, StringComparison.OrdinalIgnoreCase)
                    || cultureInfo.EnglishName.Contains(context.UserInput, StringComparison.OrdinalIgnoreCase)
                    || cultureInfo.IetfLanguageTag.Contains(context.UserInput, StringComparison.OrdinalIgnoreCase))
                {
                    choices.Add(new DiscordAutoCompleteChoice(_cultureInfoDisplayNames[cultureInfo], cultureInfo.Name));
                }
            }

            return ValueTask.FromResult<IEnumerable<DiscordAutoCompleteChoice>>(choices);
        }
    }
}
