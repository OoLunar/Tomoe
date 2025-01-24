using System;
using DSharpPlus.Entities;
using OoLunar.Tomoe.Interactivity.ComponentCreators;

namespace OoLunar.Tomoe.Interactivity.Moments.Confirm
{
    public interface IConfirmComponentCreator : IComponentCreator
    {
        public DiscordButtonComponent CreateConfirmButton(string question, Ulid id, bool isYesButton);
    }
}
