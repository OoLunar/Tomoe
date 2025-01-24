using System;
using DSharpPlus.Entities;

namespace OoLunar.Tomoe.Interactivity.Moments.Confirm
{
    public class ConfirmDefaultComponentCreator : IConfirmComponentCreator
    {
        public DiscordButtonComponent CreateConfirmButton(string question, Ulid id, bool isYesButton) => new(
            isYesButton ? DiscordButtonStyle.Success : DiscordButtonStyle.Danger,
            $"{id}_{isYesButton.ToString().ToLowerInvariant()}",
            isYesButton ? "Yes" : "No",
            false
        );
    }
}
