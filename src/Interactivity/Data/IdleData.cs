using System;
using DSharpPlus.Entities;

namespace OoLunar.Tomoe.Interactivity.Data
{
    public abstract record IdleData
    {
        public Ulid Id { get; init; }
        public DiscordMessage? Message { get; set; }
        public TimeSpan Timeout { get; init; }
        public bool IsTimedOut => Timeout == TimeSpan.Zero || (Id.Time + Timeout) < DateTimeOffset.UtcNow;
    }
}
