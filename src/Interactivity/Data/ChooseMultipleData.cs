using System.Collections.Generic;
using System.Threading.Tasks;

namespace OoLunar.Tomoe.Interactivity.Data
{
    public record ChooseMultipleData : IdleData
    {
        public required string Question { get; init; }
        public required IReadOnlyList<string> Options { get; init; }
        public TaskCompletionSource<IReadOnlyList<string>> TaskCompletionSource { get; init; } = new();
    }
}
