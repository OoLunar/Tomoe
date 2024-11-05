using System;
using System.Collections.Generic;
using System.Linq;
using DSharpPlus.Entities;

namespace OoLunar.Tomoe.Interactivity.ComponentCreators
{
    public sealed class DefaultComponentCreator : IComponentCreator
    {
        public DiscordSelectComponent CreateChooseDropdown(string question, IReadOnlyList<string> options, Ulid id)
            => new(id.ToString(), "Answer here!", options.Select(option => new DiscordSelectComponentOption(option, option)));

        public DiscordSelectComponent CreateChooseMultipleDropdown(string question, IReadOnlyList<string> options, Ulid id)
            => new(id.ToString(), "Answer here!", options.Select(option => new DiscordSelectComponentOption(option, option)), false, 1, options.Count);

        public DiscordTextInputComponent CreateModalPromptButton(string question, Ulid id)
            => new(question, id.ToString());

        public DiscordButtonComponent CreateTextPromptButton(string question, Ulid id)
            => new(DiscordButtonStyle.Primary, id.ToString(), "Click here to answer", false);

        public DiscordButtonComponent CreateConfirmButton(string question, Ulid id, bool isYesButton)
            => new(isYesButton ? DiscordButtonStyle.Success : DiscordButtonStyle.Danger, $"{id}_{isYesButton.ToString().ToLowerInvariant()}", isYesButton ? "Yes" : "No", false);
    }
}
