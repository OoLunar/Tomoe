using System;
using System.Threading;
using System.Threading.Tasks;

namespace OoLunar.Tomoe
{
    public sealed class AllocationRateTracker
    {
        public long AllocationRate { get; private set; } = GC.GetTotalAllocatedBytes();

        public AllocationRateTracker() => _ = TrackAllocationRateAsync();

        private async Task TrackAllocationRateAsync()
        {
            long previousMemoryUsage = GC.GetTotalAllocatedBytes();

            // We use a PeriodicTimer to track and update the allocation rate every 200ms
            PeriodicTimer periodicTimer = new(TimeSpan.FromSeconds(1));
            while (await periodicTimer.WaitForNextTickAsync())
            {
                long currentMemoryUsage = GC.GetTotalAllocatedBytes();
                AllocationRate = currentMemoryUsage - previousMemoryUsage;
                previousMemoryUsage = currentMemoryUsage;
            }
        }
    }
}
