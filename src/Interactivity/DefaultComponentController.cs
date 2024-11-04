using System;
using System.Collections.Generic;
using System.Linq;
using DSharpPlus.Entities;

namespace OoLunar.Tomoe.Interactivity
{
    public sealed class DefaultComponentController : IComponentController
    {
        public DiscordSelectComponent CreateChooseDropdown(string question, IReadOnlyList<string> options, Ulid id)
            => new(id.ToString(), question, options.Select(option => new DiscordSelectComponentOption(option, option)));

        public DiscordSelectComponent CreateChooseMultipleDropdown(string question, IReadOnlyList<string> options, Ulid id)
            => new(id.ToString(), question, options.Select(option => new DiscordSelectComponentOption(option, option)), false, 1, options.Count);

        public DiscordTextInputComponent CreateModalPromptButton(string question, Ulid id)
            => new(id.ToString(), question);

        public DiscordButtonComponent CreateTextPromptButton(string question, Ulid id)
            => new(DiscordButtonStyle.Primary, id.ToString(), "Click here to answer", false);
    }
}
