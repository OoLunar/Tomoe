using System;
using System.Threading;
using DSharpPlus.Entities;

namespace OoLunar.Tomoe.Interactivity.Data
{
    public abstract record IdleData
    {
        public required Ulid Id { get; init; }
        public required ulong AuthorId { get; init; }
        public required CancellationToken CancellationToken { get; init; }
        public DiscordMessage? Message { get; set; }
    }
}
