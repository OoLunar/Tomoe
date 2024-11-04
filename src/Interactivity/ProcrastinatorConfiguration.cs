using System;

namespace OoLunar.Tomoe.Interactivity
{
    public sealed record ProcrastinatorConfiguration
    {
        public TimeSpan DefaultTimeout { get; init; } = TimeSpan.FromSeconds(30);
        public IComponentController ComponentController { get; init; } = new DefaultComponentController();
    }
}
