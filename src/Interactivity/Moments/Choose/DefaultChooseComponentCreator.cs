using System;
using System.Collections.Generic;
using System.Linq;
using DSharpPlus.Entities;

namespace OoLunar.Tomoe.Interactivity.Moments.Choose
{
    public class ChooseDefaultComponentCreator : IChooseComponentCreator
    {
        public DiscordSelectComponent CreateChooseDropdown(string question, IReadOnlyList<string> options, Ulid id) => new(
            id.ToString(),
            "Answer here!",
            options.Select(option => new DiscordSelectComponentOption(option, option)),
            false,
            1,
            options.Count
        );
    }
}
