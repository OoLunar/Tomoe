using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OoLunar.Tomoe.Interactivity.Data
{
    public record ChooseData : IdleData
    {
        public required Func<ChooseData, ValueTask> Callback { get; init; }
        public required string Question { get; init; }
        public required IReadOnlyList<string> Options { get; init; }
        public string? Response { get; set; }
    }
}
