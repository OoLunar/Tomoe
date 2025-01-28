using System;
using DSharpPlus.Entities;

namespace OoLunar.Tomoe.Interactivity.Moments.Prompt
{
    public class PromptDefaultComponentCreator : IPromptComponentCreator
    {
        public DiscordTextInputComponent CreateModalPromptButton(string question, string placeholder, Ulid id)
            => new(question, id.ToString(), placeholder, required: true, style: DiscordTextInputStyle.Paragraph);

        public DiscordButtonComponent CreateTextPromptButton(string question, Ulid id)
            => new(DiscordButtonStyle.Primary, id.ToString(), "Click here to answer", false);
    }
}
