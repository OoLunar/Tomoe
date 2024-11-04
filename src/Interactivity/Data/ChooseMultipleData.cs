using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OoLunar.Tomoe.Interactivity.Data
{
    public record ChooseMultipleData : IdleData
    {
        public required Func<ChooseMultipleData, ValueTask> Callback { get; init; }
        public required string Question { get; init; }
        public required IReadOnlyList<string> Options { get; init; }
        public IReadOnlyList<string> Responses { get; set; } = Array.Empty<string>();
    }
}
