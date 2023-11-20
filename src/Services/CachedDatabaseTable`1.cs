using System;
using System.Threading.Tasks;

namespace OoLunar.Tomoe.Services
{
    public abstract class CachedDatabaseTable<T> : IAsyncDisposable
    {
        public abstract ValueTask DisposeAsync();
    }
}
