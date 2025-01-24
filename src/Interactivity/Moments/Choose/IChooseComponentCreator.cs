using System;
using System.Collections.Generic;
using DSharpPlus.Entities;
using OoLunar.Tomoe.Interactivity.ComponentCreators;

namespace OoLunar.Tomoe.Interactivity.Moments.Choose
{
    public interface IChooseComponentCreator : IComponentCreator
    {
        public DiscordSelectComponent CreateChooseDropdown(string question, IReadOnlyList<string> options, Ulid id);
    }
}
