using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Tomoe.Interfaces;
using Tomoe.Models;

namespace Tomoe.Utils
{
    /// <summary>
    /// A list that automatically updates itself from a database connection. The database and it's contents is never exposed, only the locally cached items. Items can be added/removed to the database through this list.
    /// </summary>
    /// <typeparam name="T">A class that implements <see cref="IExpires"/>.</typeparam>
    public class DatabaseList<T> : IList<T>, IList, IReadOnlyList<T> where T : class, IExpires
    {
        [SuppressMessage("Roslyn", "CA1711", Justification = "The event args rely on T")]
        public delegate Task PollExpiredEventArgs(object? sender, T expiredPoll);
        public event PollExpiredEventArgs PollExpired = null!;

        private Timer UpdateTimer { get; init; } = new();
        private Timer ExpireTimer { get; init; } = new();
        private ServiceProvider ServiceProvider { get; init; } = null!;
        private List<T> Items { get; init; } = new();

        /// <summary>
        /// Constructs a new <see cref="DatabaseList{T}"/>. The list pulls from the database, searching for items that are nearing their expiration date.
        /// </summary>
        /// <param name="serviceProvider"></param>
        /// <param name="pullDatabaseInterval">In milliseconds, how often to pull new items from the database.</param>
        /// <param name="expireInterval">In milliseconds, when the item is considered "nearing expiry." When an item is considered nearly expired, it is pulled from the database and kept in a local cache.</param>
        public DatabaseList(ServiceProvider serviceProvider, double pullDatabaseInterval = 60000, double expireInterval = 5000)
        {
            ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            UpdateTimer.Interval = pullDatabaseInterval;
            UpdateTimer.Elapsed += PullDatabase;
            UpdateTimer.Start();

            ExpireTimer.Interval = expireInterval;
            ExpireTimer.Elapsed += Expire;
            ExpireTimer.Start();

            PullDatabase();
            Expire(null, null!);
        }

        /// <inheritdoc/>
        public object? this[int index] { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        /// <inheritdoc/>
        T IList<T>.this[int index] { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        /// <inheritdoc/>
        T IReadOnlyList<T>.this[int index] => throw new NotImplementedException();

        public bool IsFixedSize => false;
        public bool IsReadOnly => false;
        public int Count => Items.Count;
        public bool IsSynchronized => false;
        public object SyncRoot => this;

        /// <summary>
        /// Adds an item to the database. If it's nearing expiration, it will be kept in the local cache.
        /// </summary>
        /// <param name="value">The item to be saved.</param>
        /// <returns>The zero-based index of the item within the entire <see cref="DatabaseList{T}"/>, if found; otherwise, -1 since the item is in the database.</returns>
        public int Add(object? value)
        {
            // object? should be T
            if (value is null or not T)
            {
                throw new ArgumentException($"Value must be of type {typeof(T)}");
            }

            T item = (T)value;
            using IServiceScope scope = ServiceProvider.CreateScope() ?? throw new InvalidOperationException("Could not create a ServiceScope.");
            using DatabaseContext database = scope.ServiceProvider.GetRequiredService<DatabaseContext>() ?? throw new InvalidOperationException("DatabaseContext is null.");
            database.Set<T>().Add(item);
            database.SaveChangesAsync().GetAwaiter().GetResult();

            if (item.ExpiresAt <= DateTime.UtcNow.AddMilliseconds(ExpireTimer.Interval).AddMilliseconds(UpdateTimer.Interval))
            {
                Items.Add(item);
                return Items.Count - 1;
            }
            else
            {
                return -1;
            }
        }

        /// <summary>
        /// Clears the local cache.
        /// </summary>
        public void Clear() => Items.Clear();

        /// <summary>
        /// Tests if the local cache contains an item.
        /// </summary>
        /// <param name="value">The item to seach for.</param>
        /// <returns>If the local cache contains the requested item.</returns>
        public bool Contains(object? value) => value is null or not T ? throw new ArgumentException($"Value must be of type {typeof(T)}") : Contains((T)value);

        /// <summary>
        /// Copies the local cache of items to an array.
        /// </summary>
        /// <param name="array">The array to copy into.</param>
        /// <param name="index">The index to start from.</param>
        public void CopyTo(Array array, int index)
        {
            if (array is null)
            {
                throw new ArgumentNullException(nameof(array));
            }

            if (index < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            if (array.Rank != 1)
            {
                throw new ArgumentException("Array must be one-dimensional.");
            }

            if (array.Length - index < Count)
            {
                throw new ArgumentException("Array is not large enough to copy all the items in the collection.");
            }

            while (index < array.Length)
            {
                array.SetValue(Items[index], index);
                index++;
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates through the local cache.
        /// </summary>
        /// <returns>An enumerator that iterates through the local cache.</returns>
        public IEnumerator GetEnumerator() => Items.GetEnumerator();

        /// <summary>
        /// Searches for the specified object and returns the zero-based index of the first occurrence within the entire <see cref="DatabaseList{T}"/>.
        /// </summary>
        /// <param name="value">The item to get the index of.</param>
        /// <returns>The zero-based index of the first occurrence of item within the entire <see cref="List{T}"/>, if found; otherwise, -1 since the item is in the database or doesn't exist.</returns>
        public int IndexOf(object? value) => value is null or not T ? throw new ArgumentException($"Value must be of type {typeof(T)}") : IndexOf((T)value);

        /// <summary>
        /// Inserts an item into the database. If it's nearing expiration, it will be inserted into the local cache at the requested index.
        /// </summary>
        /// <param name="index">The zero-based index at which item should be inserted.</param>
        /// <param name="value">The item to be inserted.</param>
        public void Insert(int index, object? value)
        {
            if (value is null or not T)
            {
                throw new ArgumentException($"Value must be of type {typeof(T)}");
            }
            Insert(index, (T)value);
        }

        /// <summary>
        /// Removes the requested item from both the local cache and the database.
        /// </summary>
        /// <param name="value">The item to be removed.</param>
        public void Remove(object? value)
        {
            if (value is null or not T)
            {
                throw new ArgumentException($"Value must be of type {typeof(T)}");
            }

            T item = (T)value;
            Items.Remove(item);
            using IServiceScope scope = ServiceProvider.CreateScope() ?? throw new InvalidOperationException("Could not create a ServiceScope.");
            using DatabaseContext database = scope.ServiceProvider.GetRequiredService<DatabaseContext>() ?? throw new InvalidOperationException("DatabaseContext is null.");
            if (database.Set<T>().Contains(item))
            {
                database.Entry(item).State = EntityState.Deleted;
                database.SaveChangesAsync().GetAwaiter().GetResult();
            }
        }

        /// <summary>
        /// Removes an item from the local cache, found by using the provided index.
        /// </summary>
        /// <param name="index">The element's index in the list.</param>
        public void RemoveAt(int index)
        {
            if (index < 0 || index >= Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }
            Items.RemoveAt(index);
        }

        /// <summary>
        /// Checks if the local cache contains the requested item.
        /// </summary>
        /// <param name="item">The item to search for.</param>
        /// <returns>If the local cache contains the requested item.</returns>
        bool ICollection<T>.Contains(T item) => Items.Contains(item);

        /// <summary>
        /// Copies the local cache of items to an array.
        /// </summary>
        /// <param name="array">The array to fill.</param>
        /// <param name="arrayIndex">The index to start at.</param>
        void ICollection<T>.CopyTo(T[] array, int arrayIndex) => CopyTo(array, arrayIndex);

        /// <summary>
        /// Returns the enumerator used to iterate through the local cache.
        /// </summary>
        /// <returns>The enumerator used to iterate through the local cache.</returns>
        IEnumerator<T> IEnumerable<T>.GetEnumerator() => Items.GetEnumerator();

        /// <summary>
        /// Searches for the specified item and returns the zero-based index of the first occurrence within the <see cref="DatabaseList{T}"/>'s local cache.
        /// </summary>
        /// <param name="item">The item to search for.</param>
        /// <returns>The zero-based index of the first occurrence of item within the entire <see cref="List{T}"/>, if found; otherwise, -1 since the item is in the database or doesn't exist.</returns>
        int IList<T>.IndexOf(T item) => Items.IndexOf(item);

        /// <summary>
        /// Inserts an item into the database. If it's nearing expiration, it will be inserted into the local cache at the requested index.
        /// </summary>
        /// <param name="index">The requested index to insert into.</param>
        /// <param name="item">The item to insert.</param>
        void IList<T>.Insert(int index, T item)
        {
            using IServiceScope scope = ServiceProvider.CreateScope() ?? throw new InvalidOperationException("Could not create a ServiceScope.");
            using DatabaseContext database = scope.ServiceProvider.GetRequiredService<DatabaseContext>() ?? throw new InvalidOperationException("DatabaseContext is null.");
            database.Set<T>().Add(item);
            database.SaveChangesAsync().GetAwaiter().GetResult();

            if (item.ExpiresAt <= DateTime.UtcNow.AddMilliseconds(ExpireTimer.Interval).AddMilliseconds(UpdateTimer.Interval))
            {
                Items.Insert(index, item);
            }
        }

        /// <summary>
        /// Adds an item into the database. If it's nearing expiration, it will be inserted into the local cache.
        /// </summary>
        /// <param name="item">The item to add.</param>
        void ICollection<T>.Add(T item) => Add(item);

        /// <summary>
        /// Removes an item from the database and local cache.
        /// </summary>
        /// <param name="item">The item to remove.</param>
        /// <returns>True if the item was found, false otherwise.</returns>
        bool ICollection<T>.Remove(T item)
        {
            Remove(item);
            return true;
        }

        public bool Update(T item, bool force = false)
        {
            if (item is null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            if (item.ExpiresAt >= DateTime.UtcNow.AddMilliseconds(ExpireTimer.Interval).AddMilliseconds(UpdateTimer.Interval))
            {
                Items.Remove(item);
            }

            using IServiceScope scope = ServiceProvider.CreateScope() ?? throw new InvalidOperationException("Could not create a ServiceScope.");
            using DatabaseContext database = scope.ServiceProvider.GetRequiredService<DatabaseContext>() ?? throw new InvalidOperationException("DatabaseContext is null.");
            EntityEntry<T>? entity = database.Set<T>().Update(item);
            if (force)
            {
                database.Entry(entity).State = EntityState.Modified;
                database.SaveChangesAsync().GetAwaiter().GetResult();
                return true;
            }
            else if (database.ChangeTracker.HasChanges())
            {
                database.SaveChangesAsync().GetAwaiter().GetResult();
                return true;
            }
            return false;
        }

        /// <summary>
        /// Updates the local cache to match with the database.
        /// </summary>
        public void PullDatabase()
        {
            using IServiceScope scope = ServiceProvider.CreateScope() ?? throw new InvalidOperationException("Could not create a ServiceScope.");
            using DatabaseContext database = scope.ServiceProvider.GetRequiredService<DatabaseContext>() ?? throw new InvalidOperationException("DatabaseContext is null.");
            foreach (T item in database.Set<T>())
            {
                if (item.ExpiresAt <= DateTime.UtcNow.AddMilliseconds(ExpireTimer.Interval).AddMilliseconds(UpdateTimer.Interval) && !Items.Contains(item))
                {
                    Items.Add(item);
                }
            }
        }

        private void PullDatabase(object? sender, ElapsedEventArgs e) => PullDatabase();
        private async void Expire(object? sender, ElapsedEventArgs e)
        {
            T[] items = new T[Items.Count];
            Items.CopyTo(items, 0);
            using IServiceScope scope = ServiceProvider.CreateScope() ?? throw new InvalidOperationException("Could not create a ServiceScope.");

            foreach (T item in items)
            {
                if (item.ExpiresAt < DateTime.UtcNow)
                {
                    if (PollExpired == null) // No event handlers registered
                    {
                        Remove(item);
                    }
                    else
                    {
                        // Other sources can update the object without the DatabaseList knowing about it, so pull the object to get the latest modification.
                        using DatabaseContext database = scope.ServiceProvider.GetRequiredService<DatabaseContext>() ?? throw new InvalidOperationException("DatabaseContext is null.");
                        T? foundItem = database.Set<T>().Where(x => x == item).FirstOrDefault();
                        if (foundItem == null)
                        {
                            Items.Remove(item);
                        }
                        else
                        {
                            await PollExpired(this, foundItem);
                        }
                    }
                }
            }
        }
    }
}