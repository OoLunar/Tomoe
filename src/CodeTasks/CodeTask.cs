using System.Threading;
using System.Threading.Tasks;
using OoLunar.Tomoe.Database.Models;

namespace OoLunar.Tomoe.CodeTasks
{
    public sealed class CodeTask
    {
        public required CodeTaskModel Model { get; init; }
        public required TaskRunner TaskRunner { get; init; }
        public required CancellationTokenSource CancellationTokenSource { get; init; }
        public Task? Task { get; set; }
    }
}
