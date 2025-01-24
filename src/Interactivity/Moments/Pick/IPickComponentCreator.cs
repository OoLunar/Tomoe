using System;
using System.Collections.Generic;
using DSharpPlus.Entities;
using OoLunar.Tomoe.Interactivity.ComponentCreators;

namespace OoLunar.Tomoe.Interactivity.Moments.Pick
{
    public interface IPickComponentCreator : IComponentCreator
    {
        public DiscordSelectComponent CreatePickDropdown(string question, IReadOnlyList<string> options, Ulid id);
    }
}
