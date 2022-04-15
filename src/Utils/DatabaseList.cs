using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.DependencyInjection;
using Tomoe.Interfaces;
using Tomoe.Models;

namespace Tomoe.Utils
{
    /// <summary>
    /// A list that grabs expirable items from the database and triggers an event when they expire.
    /// </summary>
    /// <typeparam name="TObject">A class that implements <see cref="IExpires"/>.</typeparam>
    public class DatabaseList<TObject, TObjectId> : IList<TObject>, IList, IReadOnlyList<TObject>
        where TObjectId : notnull
        where TObject : class, IExpires<TObjectId>
    {
        [SuppressMessage("Roslyn", "CA1711", Justification = "The event args rely on T")]
        public delegate Task ItemExpiredEventArgs(object? sender, TObject expiredPoll);
        public event ItemExpiredEventArgs ItemExpired = null!;

        private Timer UpdateTimer { get; init; } = new();
        private Timer ExpireTimer { get; init; } = new();
        private ServiceProvider ServiceProvider { get; init; } = null!;
        private List<IExpires<TObjectId>> Items { get; init; } = new();

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

        public object? this[int index] { get => throw new NotSupportedException("Databases don't support indexing."); set => throw new NotSupportedException("Databases don't support indexing."); }
        TObject IList<TObject>.this[int index] { get => throw new NotSupportedException("Databases don't support indexing."); set => throw new NotSupportedException("Databases don't support indexing."); }
        TObject IReadOnlyList<TObject>.this[int index] => throw new NotSupportedException("Databases don't support indexing.");

        public bool IsFixedSize => false;
        public bool IsReadOnly => false;
        public int Count => Items.Count;
        public bool IsSynchronized => false;
        public object SyncRoot => this;

        public void Add(TObject item)
        {
            using IServiceScope scope = ServiceProvider.CreateScope() ?? throw new InvalidOperationException("Could not create a ServiceScope.");
            using DatabaseContext database = scope.ServiceProvider.GetRequiredService<DatabaseContext>() ?? throw new InvalidOperationException("DatabaseContext is null.");
            database.Set<TObject>().Add(item);
            database.SaveChangesAsync().GetAwaiter().GetResult();

            if (item.ExpiresAt < DateTime.UtcNow.AddMilliseconds(UpdateTimer.Interval))
            {
                Items.Add(item);
            }
        }

        public int Add(object? value)
        {
            if (value is TObject item)
            {
                Add(item);
            }
            return -1;
        }

        public void Clear()
        {
            Items.Clear();
            PullDatabase();
        }

        public bool Contains(object? value) => value is TObject item && Contains(item);
        public bool Contains(TObject item)
        {
            using IServiceScope scope = ServiceProvider.CreateScope() ?? throw new InvalidOperationException("Could not create a ServiceScope.");
            using DatabaseContext database = scope.ServiceProvider.GetRequiredService<DatabaseContext>() ?? throw new InvalidOperationException("DatabaseContext is null.");
            TObject? databaseItem = database.Set<TObject>().Where(x => x.Id.Equals(item.Id)).FirstOrDefault();
            IExpires<TObjectId>? localItem = Items.FirstOrDefault(x => x.Id.Equals(item.Id));
            if (databaseItem == null && localItem != null)
            {
                Items.Remove(localItem);
            }

            return databaseItem == null;
        }

        public void CopyTo(TObject[] array, int arrayIndex) => Items.CopyTo(array, arrayIndex);
        public void CopyTo(Array array, int index)
        {
            if (array is not TObject[] items)
            {
                throw new ArgumentException("The array must be of type " + typeof(TObject[]).FullName, nameof(array));
            }

            Items.CopyTo(items, index);
        }

        public int IndexOf(TObject item) => throw new NotSupportedException("Databases don't support indexing.");
        public int IndexOf(object? value) => throw new NotSupportedException("Databases don't support indexing.");
        public void Insert(int index, TObject item) => throw new NotSupportedException("Databases don't support indexing.");
        public void Insert(int index, object? value) => throw new NotSupportedException("Databases don't support indexing.");
        public void RemoveAt(int index) => throw new NotSupportedException("Databases don't support indexing.");

        public void Remove(object? value)
        {
            if (value is not TObject item)
            {
                throw new ArgumentException("The value must be of type " + typeof(TObject).FullName, nameof(value));
            }

            Remove(item);
        }

        public bool Remove(TObject item)
        {
            using IServiceScope scope = ServiceProvider.CreateScope() ?? throw new InvalidOperationException("Could not create a ServiceScope.");
            using DatabaseContext database = scope.ServiceProvider.GetRequiredService<DatabaseContext>() ?? throw new InvalidOperationException("DatabaseContext is null.");
            TObject? databaseItem = database.Set<TObject>().Where(x => x.Id.Equals(item.Id)).FirstOrDefault();
            IExpires<TObjectId>? localItem = Items.FirstOrDefault(x => x.Id.Equals(item.Id));

            // If the database doesn't have the requested item, it's been removed. Make sure to update cache.
            if (databaseItem == null && localItem != null)
            {
                Items.Remove(localItem);
            }

            // The database has the item. Remove it.
            if (databaseItem != null)
            {
                database.Set<TObject>().Remove(databaseItem);
                database.SaveChangesAsync().GetAwaiter().GetResult();

                // Check if cache has the item. If it does, remove it.
                if (localItem != null)
                {
                    Items.Remove(localItem);
                }
                return true;
            }

            return false;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        public IEnumerator<TObject> GetEnumerator()
        {
            using IServiceScope scope = ServiceProvider.CreateScope() ?? throw new InvalidOperationException("Could not create a ServiceScope.");
            using DatabaseContext database = scope.ServiceProvider.GetRequiredService<DatabaseContext>() ?? throw new InvalidOperationException("DatabaseContext is null.");
            return database.Set<TObject>().ToList().GetEnumerator();
        }

        public bool Update(TObject item, bool force = false)
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
            EntityEntry<TObject>? entity = database.Set<TObject>().Update(item);
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

        private void PullDatabase(object? sender, ElapsedEventArgs eventArgs) => PullDatabase();
        public void PullDatabase()
        {
            using IServiceScope scope = ServiceProvider.CreateScope() ?? throw new InvalidOperationException("Could not create a ServiceScope.");
            using DatabaseContext database = scope.ServiceProvider.GetRequiredService<DatabaseContext>() ?? throw new InvalidOperationException("DatabaseContext is null.");
            Items.Clear();

            DateTime cacheTime = DateTime.UtcNow.AddMilliseconds(UpdateTimer.Interval); // We do this since EFCore doesn't bother evaluating this client side before executing the query.
            foreach (TObject item in database.Set<TObject>().Where(x => x.ExpiresAt >= cacheTime || x.ExpiresAt < DateTime.UtcNow).AsEnumerable())
            {
                Items.Add(item);
            }
        }

        private void Expire(object? sender, ElapsedEventArgs e) => Expire();
        private async void Expire()
        {
            TObject[] items = new TObject[Items.Count];
            Items.CopyTo(items, 0);
            using IServiceScope scope = ServiceProvider.CreateScope() ?? throw new InvalidOperationException("Could not create a ServiceScope.");

            foreach (TObject item in items)
            {
                if (item.ExpiresAt <= DateTime.UtcNow)
                {
                    if (ItemExpired == null) // No event handlers registered
                    {
                        Remove(item);
                    }
                    else
                    {
                        // Other sources can update the object without the DatabaseList knowing about it, so pull the object to get the latest modification.
                        using DatabaseContext database = scope.ServiceProvider.GetRequiredService<DatabaseContext>() ?? throw new InvalidOperationException("DatabaseContext is null.");
                        TObject? foundItem = database.Set<TObject>().Where(x => x.Id.Equals(item.Id)).FirstOrDefault();
                        if (foundItem == null)
                        {
                            Items.Remove(item);
                        }
                        else
                        {
                            await ItemExpired(this, foundItem);
                        }
                    }
                }
            }
        }
    }
}
