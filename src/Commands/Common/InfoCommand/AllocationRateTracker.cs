using System;
using System.Threading;
using System.Threading.Tasks;

namespace OoLunar.Tomoe
{
    /// <summary>
    /// Tracks the memory allocation rate of the application.
    /// </summary>
    public sealed class AllocationRateTracker
    {
        /// <summary>
        /// The current allocation rate of the application's total process memory.
        /// </summary>
        public long AllocationRate { get; private set; } = GC.GetTotalAllocatedBytes();

        /// <summary>
        /// Creates a new instance of the <see cref="AllocationRateTracker"/>, starting the allocation rate tracking in the background.
        /// </summary>
        public AllocationRateTracker() => _ = TrackAllocationRateAsync();

        private async Task TrackAllocationRateAsync()
        {
            // We use a PeriodicTimer to track and update the allocation rate every 200ms
            PeriodicTimer periodicTimer = new(TimeSpan.FromSeconds(1));
            long previousMemoryUsage = GC.GetTotalAllocatedBytes();
            while (await periodicTimer.WaitForNextTickAsync())
            {
                long currentMemoryUsage = GC.GetTotalAllocatedBytes();
                AllocationRate = currentMemoryUsage - previousMemoryUsage;
                previousMemoryUsage = currentMemoryUsage;
            }
        }
    }
}
