using System.Threading.Tasks;

namespace OoLunar.Tomoe.Interactivity.Data
{
    public record PromptData : IdleData
    {
        public required string Question { get; init; }
        public TaskCompletionSource<string?> TaskCompletionSource { get; init; } = new();
    }
}
