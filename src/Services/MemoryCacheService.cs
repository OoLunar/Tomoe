using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;

namespace OoLunar.Tomoe.Services
{
    /// <summary>
    /// A custom memory cache. Allows for optionally removing an item at a specified date, invoking the callback if passed. This is meant to be used as a replacement for <see cref="Microsoft.Extensions.Caching.Memory.IMemoryCache"/>.
    /// </summary>
    public sealed class MemoryCacheService
    {
        /// <summary>
        /// Stores the object in it's boxed state, indexed by the key.
        /// </summary>
        private readonly ConcurrentDictionary<object, MemoryWrapper> _cache = new();

        /// <summary>
        /// A timer that runs every 50 milliseconds to check for expired items.
        /// </summary>
        private readonly PeriodicTimer _timer = new(TimeSpan.FromMilliseconds(50));

        /// <summary>
        /// Initializes a new instance of the <see cref="MemoryCacheService"/> class.
        /// </summary>
        public MemoryCacheService() => _ = ExpireItemsAsync();

        /// <summary>
        /// Attempts to add an item to the cache. If the key already exists, the item will not be added.
        /// </summary>
        /// <param name="key">The key to index the item by.</param>
        /// <param name="value">The item to store.</param>
        /// <param name="expiration">The date and time to remove the item at.</param>
        /// <param name="callback">The callback to invoke when the item is removed.</param>
        /// <returns>True if the item was added, false otherwise.</returns>
        public bool TryAdd(object key, object value, DateTimeOffset? expiration = null, Action<object>? callback = null)
        {
            ArgumentNullException.ThrowIfNull(key);
            ArgumentNullException.ThrowIfNull(value);
            return _cache.TryAdd(key, new MemoryWrapper(value, expiration, callback));
        }

        /// <summary>
        /// Attempts to get an item from the cache.
        /// </summary>
        /// <param name="key">The key to index the item by.</param>
        /// <param name="value">The item to store.</param>
        /// <returns>True if the item was found, false otherwise.</returns>
        public bool TryGetValue(object key, out MemoryWrapper? value) => key == null ? throw new ArgumentNullException(nameof(key)) : _cache.TryGetValue(key, out value);

        /// <summary>
        /// Attempts to get an item from the cache.
        /// </summary>
        /// <typeparam name="T">The type of the item to get.</typeparam>
        /// <param name="key">The key to index the item by.</param>
        /// <param name="value">The item to store.</param>
        /// <returns>True if the item was found, false otherwise.</returns>
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

        /// <summary>
        /// Attempts to remove an item from the cache.
        /// </summary>
        /// <param name="key">The key to index the item by.</param>
        /// <param name="value">The item to store.</param>
        /// <returns>True if the item was removed, false otherwise.</returns>
        public bool TryRemove(object key, out MemoryWrapper? value) => key == null ? throw new ArgumentNullException(nameof(key)) : _cache.TryRemove(key, out value);

        /// <summary>
        /// Attempts to replace an item in the cache.
        /// </summary>
        /// <param name="key">The key to index the item by.</param>
        /// <param name="newValue">The item to store.</param>
        /// <param name="expiration">The date and time to remove the item at.</param>
        /// <param name="callback">The callback to invoke when the item is removed.</param>
        /// <returns>True if the item was replaced, false otherwise.</returns>
        public MemoryWrapper Set(object key, object newValue, DateTimeOffset? expiration = null, Action<object>? callback = null)
        {
            ArgumentNullException.ThrowIfNull(key);
            ArgumentNullException.ThrowIfNull(newValue);
            return _cache[key] = new MemoryWrapper(newValue, expiration, callback);
        }

        /// <summary>
        /// Returns a read-only dictionary of all items in the cache.
        /// </summary>
        /// <returns>A read-only dictionary of all items in the cache.</returns>
        public IReadOnlyDictionary<object, MemoryWrapper> GetItems() => new ReadOnlyDictionary<object, MemoryWrapper>(_cache);

        /// <summary>
        /// Iterates through all items in the cache and removes any that have expired.
        /// </summary>
        private async Task ExpireItemsAsync()
        {
            // We use a periodic timer to wait for the next tick while also preventing multiple iterations from running at the same time.
            while (await _timer.WaitForNextTickAsync())
            {
                // Compare all items to the same time.
                DateTimeOffset now = DateTimeOffset.UtcNow;

                // Parallel loop to remove the expired items as fast as possible.
                Parallel.ForEach(GetItems(), (item) =>
                {
                    // Check if the item has an expiration date set and if it has expired.
                    if (item.Value.Expiration.HasValue && item.Value.Expiration.Value < now)
                    {
                        // Try to remove the item and invoke the callback if it was removed.
                        if (TryRemove(item.Key, out MemoryWrapper? value) && value is not null && value.Callback is not null)
                        {
                            value.Callback.Invoke(value.Value);
                        }
                    }
                });
            }
        }
    }

    /// <summary>
    /// A wrapper for items stored in the cache.
    /// </summary>
    public sealed class MemoryWrapper
    {
        /// <summary>
        /// The value of the item.
        /// </summary>
        public object Value { get; set; }

        /// <summary>
        /// The date and time to remove the item at.
        /// </summary>
        public DateTimeOffset? Expiration { get; set; }

        /// <summary>
        /// The callback to invoke when the item is removed.
        /// </summary>
        public Action<object>? Callback { get; set; }

        /// <summary>
        /// Creates a new instance of the <see cref="MemoryWrapper"/> class.
        /// </summary>
        /// <param name="value">The value of the item.</param>
        /// <param name="expiration">The date and time to remove the item at.</param>
        /// <param name="callback">The callback to invoke when the item is removed.</param>
        internal MemoryWrapper(object value, DateTimeOffset? expiration, Action<object>? callback)
        {
            Value = value ?? throw new ArgumentNullException(nameof(value));
            Expiration = expiration;
            Callback = callback;
        }
    }
}
