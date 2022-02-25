using System;
using Tomoe.Enums;

namespace Tomoe.Models
{
    public interface IAutoModel
    {
        Guid Id { get; init; }
        ulong GuildId { get; init; }
        ulong ChannelId { get; init; }
        FilterType FilterType { get; init; }
        string? Filter { get; init; }
    }
}