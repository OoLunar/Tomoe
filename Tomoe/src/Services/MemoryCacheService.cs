using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;

namespace OoLunar.Tomoe.Services
{
    public sealed class MemoryCacheService
    {
        public IReadOnlyDictionary<object, MemoryWrapper> Cache => new ReadOnlyDictionary<object, MemoryWrapper>(_cache);
        private readonly ConcurrentDictionary<object, MemoryWrapper> _cache = new();
        private readonly PeriodicTimer _timer = new(TimeSpan.FromMilliseconds(50));

        public MemoryCacheService() => _ = ExpireItemsAsync();

        public bool TryAdd(object key, object value, DateTimeOffset? expiration = null, Action<object>? callback = null)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            else if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            return _cache.TryAdd(key, new MemoryWrapper(value, expiration, callback));
        }

        public bool TryGetValue(object key, out MemoryWrapper? value) => key == null ? throw new ArgumentNullException(nameof(key)) : _cache.TryGetValue(key, out value);

        public bool TryGetValue<T>(object key, out T? value)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            else if (_cache.TryGetValue(key, out MemoryWrapper? wrapper))
            {
                value = (T)wrapper.Value;
                return true;
            }
            else
            {
                value = default;
                return false;
            }
        }

        public bool TryRemove(object key, out MemoryWrapper? value) => key == null ? throw new ArgumentNullException(nameof(key)) : _cache.TryRemove(key, out value);

        public MemoryWrapper Set(object key, object value, DateTimeOffset? expiration = null, Action<object>? callback = null)
        {
            if (key == null)
            {
                throw new ArgumentNullException(nameof(key));
            }
            else if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            return _cache[key] = new MemoryWrapper(value, expiration, callback);
        }

        private async Task ExpireItemsAsync()
        {
            while (await _timer.WaitForNextTickAsync())
            {
                DateTimeOffset now = DateTimeOffset.UtcNow;
                Parallel.ForEach(Cache, (item) =>
                {
                    if (item.Value.Expiration.HasValue && item.Value.Expiration.Value < now)
                    {
                        if (TryRemove(item.Key, out MemoryWrapper? value) && value is not null && value.Callback is not null)
                        {
                            value.Callback.Invoke(value.Value);
                        }
                    }
                });
            }
        }
    }

    public sealed class MemoryWrapper
    {
        public object Value { get; set; }
        public DateTimeOffset? Expiration { get; set; }
        public Action<object>? Callback { get; set; }

        internal MemoryWrapper(object value, DateTimeOffset? expiration, Action<object>? callback)
        {
            Value = value ?? throw new ArgumentNullException(nameof(value));
            Expiration = expiration;
            Callback = callback;
        }
    }
}
