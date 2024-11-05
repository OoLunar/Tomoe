using System.Threading.Tasks;

namespace OoLunar.Tomoe.Interactivity.Data
{
    public record ConfirmData : IdleData
    {
        public required string Question { get; init; }
        public TaskCompletionSource<bool?> TaskCompletionSource { get; init; } = new();
    }
}
