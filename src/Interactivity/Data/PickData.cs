using System.Collections.Generic;
using System.Threading.Tasks;

namespace OoLunar.Tomoe.Interactivity.Data
{
    public record PickData : IdleData
    {
        public required string Question { get; init; }
        public required IReadOnlyList<string> Options { get; init; }
        public TaskCompletionSource<string?> TaskCompletionSource { get; init; } = new();
    }
}
