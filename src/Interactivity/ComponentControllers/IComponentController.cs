using System;
using System.Collections.Generic;
using DSharpPlus.Entities;

namespace OoLunar.Tomoe.Interactivity.ComponentCreators
{
    public interface IComponentCreator
    {
        public DiscordButtonComponent CreateTextPromptButton(string question, Ulid id);
        public DiscordTextInputComponent CreateModalPromptButton(string question, Ulid id);
        public DiscordSelectComponent CreatePickDropdown(string question, IReadOnlyList<string> options, Ulid id);
        public DiscordSelectComponent CreateChooseDropdown(string question, IReadOnlyList<string> options, Ulid id);
        public DiscordButtonComponent CreateConfirmButton(string question, Ulid id, bool isYesButton);
    }
}
