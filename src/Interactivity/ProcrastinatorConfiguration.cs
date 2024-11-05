using System;
using OoLunar.Tomoe.Interactivity.ComponentCreators;
using OoLunar.Tomoe.Interactivity.ComponentHandlers;

namespace OoLunar.Tomoe.Interactivity
{
    public sealed record ProcrastinatorConfiguration
    {
        public TimeSpan DefaultTimeout { get; init; } = TimeSpan.FromSeconds(30);
        public IComponentCreator ComponentController { get; init; } = new DefaultComponentCreator();
        public IComponentHandler ComponentHandler { get; init; } = new DefaultComponentHandler();
    }
}
