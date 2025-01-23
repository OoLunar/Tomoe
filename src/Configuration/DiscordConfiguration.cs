using System;
using DSharpPlus.Entities;

namespace OoLunar.Tomoe.Configuration
{
    public sealed record DiscordConfiguration
    {
        public required string? Token { get; init; }
        public string Prefix { get; init; } = ">>";
        public ulong GuildId { get; init; }
        public string SupportInvite { get; init; } = "https://discord.gg/YCSTr2KGq6";
        public string[] Processors { get; init; } = [];
        public TimeSpan CachePrefixSlidingExpiration { get; init; } = TimeSpan.FromMinutes(5);
        public string StatusText { get; init; } = "Looking to help out!";
        public DiscordActivityType StatusType { get; init; } = DiscordActivityType.Custom;
    }
}
