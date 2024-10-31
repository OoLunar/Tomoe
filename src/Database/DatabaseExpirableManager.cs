using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Npgsql;
using NpgsqlTypes;
using OoLunar.Tomoe.Configuration;

namespace OoLunar.Tomoe.Database
{
    public sealed class DatabaseExpirableManager<TSelf, TId>
        where TSelf : IExpirableModel<TSelf, TId>
        where TId : ISpanParsable<TId>
    {
        private readonly NpgsqlCommand _findExpirableCommand;
        private readonly NpgsqlCommand _deleteExpirableCommand;
        private readonly NpgsqlCommand _getExpirablesCommand;
        private readonly NpgsqlCommand _updateExpirableCommand;
        private readonly SemaphoreSlim _semaphore = new(1, 1);

        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<DatabaseExpirableManager<TSelf, TId>> _logger;
        private readonly PeriodicTimer _expireTimer;
        private readonly Dictionary<TId, DateTimeOffset> _expirableCache = [];

        public DatabaseExpirableManager(IServiceProvider serviceProvider, TomoeConfiguration tomoeConfiguration, DatabaseConnectionManager connectionManager, ILogger<DatabaseExpirableManager<TSelf, TId>>? logger = null)
        {
            ArgumentNullException.ThrowIfNull(serviceProvider, nameof(serviceProvider));
            ArgumentNullException.ThrowIfNull(tomoeConfiguration, nameof(tomoeConfiguration));
            ArgumentNullException.ThrowIfNull(connectionManager, nameof(connectionManager));
            _serviceProvider = serviceProvider;
            _expireTimer = new(TimeSpan.FromSeconds(tomoeConfiguration.Database.ExpireInterval));
            _logger = logger ?? NullLogger<DatabaseExpirableManager<TSelf, TId>>.Instance;

            NpgsqlConnection connection = connectionManager.CreateConnection();
            connection.Open();

            _findExpirableCommand = new($"SELECT id, expires_at FROM {TSelf.TableName}", connection);
            _findExpirableCommand.Parameters.Add(new NpgsqlParameter("table", NpgsqlDbType.Text) { Value = TSelf.TableName });
            _findExpirableCommand.Prepare();

            _deleteExpirableCommand = new($"DELETE FROM {TSelf.TableName} WHERE id = @id", connection);
            _deleteExpirableCommand.Parameters.Add(new NpgsqlParameter("table", NpgsqlDbType.Text) { Value = TSelf.TableName });
            _deleteExpirableCommand.Parameters.Add(new NpgsqlParameter("id", NpgsqlDbType.Text));
            _deleteExpirableCommand.Prepare();

            _getExpirablesCommand = new($"SELECT * FROM {TSelf.TableName} WHERE expires_at < @expires_at", connection);
            _getExpirablesCommand.Parameters.Add(new NpgsqlParameter("table", NpgsqlDbType.Text) { Value = TSelf.TableName });
            _getExpirablesCommand.Parameters.Add(new NpgsqlParameter("expires_at", NpgsqlDbType.TimestampTz));
            _getExpirablesCommand.Prepare();

            _updateExpirableCommand = new($"UPDATE {TSelf.TableName} SET expires_at = @expires_at WHERE id = @id", connection);
            _updateExpirableCommand.Parameters.Add(new NpgsqlParameter("table", NpgsqlDbType.Text) { Value = TSelf.TableName });
            _updateExpirableCommand.Parameters.Add(new NpgsqlParameter("id", NpgsqlDbType.Text));
            _updateExpirableCommand.Parameters.Add(new NpgsqlParameter("expires_at", NpgsqlDbType.TimestampTz));
            _updateExpirableCommand.Prepare();

            _ = PopulateExpiryCacheAsync();
            _ = ExpireCacheAsync();
        }

        public void AddToCache(TId id, DateTimeOffset expiresAt) => _expirableCache[id] = expiresAt;

        private async Task PopulateExpiryCacheAsync()
        {
            _logger.LogInformation("Starting expirary cache population for {Type}!", TSelf.TableName);
            do
            {
                _logger.LogTrace("Waiting for next expiration check...");

                // Wait until there aren't any other operations happening.
                await _semaphore.WaitAsync();
                _logger.LogTrace("Checking for expired expirables...");

                // Asynchronously read the expirables from the database
                NpgsqlDataReader reader = await _findExpirableCommand.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    if (!TId.TryParse(reader.GetValue(0).ToString(), CultureInfo.InvariantCulture, out TId? id))
                    {
                        continue;
                    }

                    DateTimeOffset expiresAt = (DateTimeOffset)reader.GetDateTime(1);
                    if (!_expirableCache.ContainsKey(id) && (IsExpired(expiresAt) || WillExpireSoon(expiresAt, _expireTimer.Period)))
                    {
                        _logger.LogTrace("Adding expirable with ID {Id} to the cache. Expiring in {TimeSpan}", id, expiresAt - DateTimeOffset.UtcNow);
                        _expirableCache[id] = expiresAt;
                    }
                }

                // Explicitly dispose of the reader since `await using` doesn't seem to be doing it properly.
                await reader.DisposeAsync();

                // Let other operations happen once more.
                _semaphore.Release();
            }
            while (await _expireTimer.WaitForNextTickAsync());
        }

        private async Task ExpireCacheAsync()
        {
            PeriodicTimer expireCacheTimer = new(TimeSpan.FromSeconds(1));
            do
            {
                await _semaphore.WaitAsync();

                // Select in bulk to reduce the number of queries.
                _getExpirablesCommand.Parameters["expires_at"].Value = DateTimeOffset.UtcNow;
                NpgsqlDataReader reader = await _getExpirablesCommand.ExecuteReaderAsync();
                List<TSelf> expiredExpirables = [];
                while (await reader.ReadAsync())
                {
                    if (!TId.TryParse(reader.GetValue(0).ToString(), CultureInfo.InvariantCulture, out TId? id))
                    {
                        _logger.LogWarning("Failed to parse Id '{Id}' as '{Type}' from {TableName}", reader.GetValue(0), typeof(TId).FullName ?? typeof(TId).Name, TSelf.TableName);
                    }
                    else if (!TSelf.TryParse(reader, out TSelf? expirable))
                    {
                        _logger.LogWarning("Failed to parse '{Type}' for Id '{Id}' from {TableName}", typeof(TSelf).FullName ?? typeof(TSelf).Name, id, TSelf.TableName);
                    }
                    else if (!IsExpired(expirable.ExpiresAt))
                    {
                        continue;
                    }
                    else
                    {
                        expiredExpirables.Add(expirable);
                    }
                }

                // Explicitly dispose of the reader since `await using` doesn't seem to be doing it properly.
                await reader.DisposeAsync();

                // Let other operations happen once more.
                _semaphore.Release();

                // Use Parallel because we're going to be making network calls, which can take up a significant amount of time if done sequentially.
                await Parallel.ForEachAsync(expiredExpirables, async (TSelf expirable, CancellationToken cancellationToken) => await ExpireExpirableAsync(expirable));
            }
            while (await expireCacheTimer.WaitForNextTickAsync());
        }

        private async ValueTask ExpireExpirableAsync(TSelf expirable)
        {
            // Fetch the latest information from the database
            bool shouldDelete = true;
            try
            {
                _logger.LogTrace("Expiring expirable with ID {Id}", expirable);
                shouldDelete = await TSelf.ExpireAsync(expirable, _serviceProvider);
            }
            catch (Exception error)
            {
                _logger.LogError(error, "Failed to expire expirable with ID {Id}", expirable);
            }
            finally
            {
                if (!shouldDelete)
                {
                    _logger.LogDebug("Postponing expiration of expirable with ID {Id} for another minute", expirable);
                    await UpdateExpirationAsync(expirable.Id, expirable.ExpiresAt);
                }
                else
                {
                    _logger.LogTrace("Removing expirable with ID {Id} from the cache and database", expirable);
                    await RemoveExpirableAsync(expirable.Id);
                }
            }
        }

        private async ValueTask UpdateExpirationAsync(TId id, DateTimeOffset expiresAt)
        {
            await _semaphore.WaitAsync();
            _updateExpirableCommand.Parameters["id"].Value = id.ToString();
            _updateExpirableCommand.Parameters["expires_at"].Value = expiresAt + TimeSpan.FromMinutes(1);
            await _updateExpirableCommand.ExecuteNonQueryAsync();
            _expirableCache[id] = expiresAt;
            _semaphore.Release();
        }

        private async ValueTask RemoveExpirableAsync(TId id)
        {
            await _semaphore.WaitAsync();
            _expirableCache.Remove(id);
            _deleteExpirableCommand.Parameters["id"].Value = id.ToString();
            await _deleteExpirableCommand.ExecuteNonQueryAsync();
            _semaphore.Release();
        }

        private static bool IsExpired(DateTimeOffset expiresAt) => DateTimeOffset.UtcNow > expiresAt;
        private static bool WillExpireSoon(DateTimeOffset expiresAt, TimeSpan timeSpan) => expiresAt - DateTimeOffset.UtcNow < timeSpan;
    }
}
