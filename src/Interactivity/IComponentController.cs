using System;
using System.Collections.Generic;
using DSharpPlus.Entities;

namespace OoLunar.Tomoe.Interactivity
{
    public interface IComponentController
    {
        public DiscordButtonComponent CreateTextPromptButton(string question, Ulid id);
        public DiscordTextInputComponent CreateModalPromptButton(string question, Ulid id);
        public DiscordSelectComponent CreateChooseDropdown(string question, IReadOnlyList<string> options, Ulid id);
        public DiscordSelectComponent CreateChooseMultipleDropdown(string question, IReadOnlyList<string> options, Ulid id);
    }
}
