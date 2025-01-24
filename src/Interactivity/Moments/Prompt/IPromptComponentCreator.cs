using System;
using DSharpPlus.Entities;
using OoLunar.Tomoe.Interactivity.ComponentCreators;

namespace OoLunar.Tomoe.Interactivity.Moments.Prompt
{
    public interface IPromptComponentCreator : IComponentCreator
    {
        public DiscordButtonComponent CreateTextPromptButton(string question, Ulid id);
        public DiscordTextInputComponent CreateModalPromptButton(string question, Ulid id);
    }
}
