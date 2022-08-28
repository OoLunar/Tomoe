using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EdgeDB;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OoLunar.Tomoe.Interfaces;

namespace OoLunar.Tomoe.Services
{
    /// <summary>
    /// Manages a list of objects which is set to expire after a certain amount of time.
    /// </summary>
    /// <typeparam name="T">The type of object keep track of.</typeparam>
    public sealed class ExpirableService<T> : IExpirableService<T> where T : IExpireable<T>, new()
    {
        /// <summary>
        /// A list of objects that are set to expire.
        /// </summary>
        private ConcurrentDictionary<T, DateTimeOffset> ExpirableItems { get; init; } = new();

        /// <summary>
        /// Used to grab the required services.
        /// </summary>
        private IServiceProvider ServiceProvider { get; init; }

        /// <summary>
        /// The program's cancellation token, used if the program is supposed to quit.
        /// </summary>
        private CancellationToken CancellationToken { get; init; }

        /// <summary>
        /// The database provider. Used to add, update or remove the expirable objects.
        /// </summary>
        private EdgeDBClient EdgeDBClient { get; init; }

        /// <summary>
        /// The periodic timer, used to execute and remove the expirable objects.
        /// </summary>
        private PeriodicTimer PeriodicTimer { get; init; }

        /// <summary>
        /// Used to inform the user of any errors that occur.
        /// </summary>
        private ILogger<ExpirableService<T>> Logger { get; init; }

        /// <summary>
        /// Creates a new expirable service.
        /// </summary>
        /// <param name="serviceProvider">The service provider used to grab required services and to pass to the <see cref="IExpireable{T}"/> objects.</param>
        public ExpirableService(IServiceProvider serviceProvider)
        {
            ArgumentNullException.ThrowIfNull(serviceProvider);

            ServiceProvider = serviceProvider;
            CancellationToken = serviceProvider.GetRequiredService<CancellationTokenSource>().Token;
            EdgeDBClient = serviceProvider.GetRequiredService<EdgeDBClient>();
            Logger = serviceProvider.GetRequiredService<ILogger<ExpirableService<T>>>();
            PeriodicTimer = new PeriodicTimer(TimeSpan.FromSeconds(1));
            // Start the forever loop in a non-blocking way.
            ExpireTimerAsync().Start();
        }

        /// <inheritdoc />
        public async Task<T> AddAsync(T item)
        {
            if (item == null)
            {
                Logger.LogWarning("A null item was attempted to be added to the expirable service.");
                throw new ArgumentNullException(nameof(item));
            }
            else if (item.ExpiresAt <= DateTimeOffset.UtcNow)
            {
                Logger.LogWarning("Item {ItemId} with an expired time {ExpiresAt} was attempted to be added to the expirable service.", item.Id, item.ExpiresAt);
                throw new ArgumentException($"Item {item.Id} of type {typeof(T).FullName} has already expired.", nameof(item));
            }

            item = (await QueryBuilder.Insert(item).UnlessConflict().Else(QueryBuilder.Update<T>(old => new T() { ExpiresAt = item.ExpiresAt }).Filter(expirable => expirable.Id == item.Id)).ExecuteAsync(EdgeDBClient, Capabilities.Modifications, CancellationToken)).First();
            ExpirableItems.AddOrUpdate(item, item.ExpiresAt, (key, oldValue) => item.ExpiresAt);
            Logger.LogTrace("Added or updated item {Id} of type {ItemType} to expire at {ExpiresAt}", item.Id, typeof(T).FullName, item.ExpiresAt);
            return item;
        }

        /// <inheritdoc />
        public async Task<bool> RemoveAsync(T item)
        {
            if (item == null)
            {
                Logger.LogWarning("A null item was attempted to be removed from the expirable service.");
                throw new ArgumentNullException(nameof(item));
            }
            else if (ExpirableItems.TryRemove(item, out _))
            {
                item = (await QueryBuilder.Delete<T>().Filter(expirable => expirable.Id == item.Id).ExecuteAsync(EdgeDBClient, Capabilities.Modifications, CancellationToken)).FirstOrDefault() ?? item;
                Logger.LogTrace("Removed item {Id} of type {ItemType}, which expired at {ExpiresAt}.", item.Id, typeof(T).FullName, item.ExpiresAt);
                return true;
            }

            Logger.LogWarning("Item {Id} of type {ItemType} was attempted to be removed from the expirable service, but it was not found.", item.Id, typeof(T).FullName);
            return false;
        }

        /// <inheritdoc />
        public async Task CheckExpiredAsync()
        {
            DateTimeOffset now = DateTimeOffset.UtcNow;

            foreach (KeyValuePair<T, DateTimeOffset> item in ExpirableItems.ToArray())
            {
                try
                {
                    T? latestItem = (await QueryBuilder.Select<T>().Filter(expirable => expirable.Id == item.Key.Id).ExecuteAsync(EdgeDBClient, Capabilities.ReadOnly, CancellationToken)).FirstOrDefault();
                    // The expirable isn't in the database anymore, so we can remove it from the cache.
                    if (latestItem == null)
                    {
                        Logger.LogWarning("Item {Id} of type {ItemType} was not found in the database, but it was in the memory cache. Removing it from the memory cache.", item.Key.Id, typeof(T).FullName);
                        ExpirableItems.TryRemove(item.Key, out _);
                    }
                    // If the time changed on the database and the new time hasn't expired yet, update the cache and move on to the next
                    else if (latestItem.ExpiresAt > now && latestItem.ExpiresAt != item.Value)
                    {
                        Logger.LogDebug("Item {Id} of type {ItemType} has a new expiration time of {ExpiresAt}. Updating the memory cache.", latestItem.Id, typeof(T).FullName, latestItem.ExpiresAt);
                        ExpirableItems.AddOrUpdate(item.Key, latestItem.ExpiresAt, (key, oldValue) => latestItem.ExpiresAt);
                    }
                    // If the expirable has expired and ExpireAsync returned true (meaning it should be removed from the list)
                    else if (latestItem.ExpiresAt <= now && await item.Key.ExpireAsync(ServiceProvider, CancellationToken))
                    {
                        Logger.LogDebug("Item {Id} of type {ItemType} has successfully expired at {ExpiresAt}. Removing it from the memory cache and the database.", latestItem.Id, typeof(T).FullName, latestItem.ExpiresAt);
                        // Calling RemoveAsync will remove it from the database.
                        await RemoveAsync(item.Key);
                    }
                }
                catch (Exception error)
                {
                    Logger.LogError(error, "An exception occurred while checking if item {Id} of type {ItemType} has expired.", item.Key.Id, typeof(T).FullName);
                }
            }
        }

        /// <summary>
        /// An asynchronous task that runs forever, checking if any of the items has expired.
        /// </summary>
        private async Task ExpireTimerAsync()
        {
            Logger.LogInformation("Starting the expirable service for {ItemType}.", typeof(T).FullName);

            // Checks if the cancellation token has been cancelled.
            while (await PeriodicTimer.WaitForNextTickAsync(CancellationToken))
            {
                // Will wait until the check is done before continuing.
                await CheckExpiredAsync();
            }
        }
    }
}
