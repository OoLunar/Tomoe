using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Threading.Tasks;
using DSharpPlus.Commands.Processors.SlashCommands;
using DSharpPlus.Commands.Processors.SlashCommands.ArgumentModifiers;
using DSharpPlus.Entities;

namespace OoLunar.Tomoe.AutocompleteProviders
{
    public sealed class TimeZoneInfoAutoCompleteProvider : IAutoCompleteProvider
    {
        private static readonly TimeZoneInfo[] _timezones;
        private static readonly FrozenSet<DiscordAutoCompleteChoice> _defaultTimezoneList;
        private static readonly FrozenDictionary<TimeZoneInfo, string> _timezoneDisplayNames;

        static TimeZoneInfoAutoCompleteProvider()
        {
            _timezones = [.. TimeZoneInfo.GetSystemTimeZones()];
            Array.Sort(_timezones, (x, y) => string.Compare(x.DisplayName, y.DisplayName, StringComparison.Ordinal));

            List<DiscordAutoCompleteChoice> choices = [];
            Dictionary<TimeZoneInfo, string> timezoneDisplayNames = [];
            for (int i = 0; i < _timezones.Length; i++)
            {
                string displayName = $"{_timezones[i].DisplayName} ({_timezones[i].Id})";
                timezoneDisplayNames[_timezones[i]] = displayName;

                // Only add the first 25 timezones to the default list
                // which is returned when the user input is empty
                if (i < 25)
                {
                    choices.Add(new DiscordAutoCompleteChoice(displayName, _timezones[i].Id));
                }
            }

            _defaultTimezoneList = choices.ToFrozenSet();
            _timezoneDisplayNames = timezoneDisplayNames.ToFrozenDictionary();
        }

        public ValueTask<IEnumerable<DiscordAutoCompleteChoice>> AutoCompleteAsync(AutoCompleteContext context)
        {
            if (string.IsNullOrWhiteSpace(context.UserInput))
            {
                return ValueTask.FromResult<IEnumerable<DiscordAutoCompleteChoice>>(_defaultTimezoneList);
            }

            List<DiscordAutoCompleteChoice> choices = [];
            foreach (TimeZoneInfo timezone in _timezones)
            {
                if (choices.Count >= 25)
                {
                    break;
                }
                else if (timezone.DisplayName.Contains(context.UserInput, StringComparison.OrdinalIgnoreCase)
                    || timezone.StandardName.Contains(context.UserInput, StringComparison.OrdinalIgnoreCase)
                    || timezone.Id.Contains(context.UserInput, StringComparison.OrdinalIgnoreCase))
                {
                    choices.Add(new DiscordAutoCompleteChoice(_timezoneDisplayNames[timezone], timezone.Id));
                }
            }

            return ValueTask.FromResult<IEnumerable<DiscordAutoCompleteChoice>>(choices);
        }
    }
}
