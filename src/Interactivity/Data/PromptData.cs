using System;
using System.Threading.Tasks;

namespace OoLunar.Tomoe.Interactivity.Data
{
    public record PromptData : IdleData
    {
        public required Func<PromptData, ValueTask> Callback { get; init; }
        public required string Question { get; init; }
        public string? Response { get; set; }
    }
}
