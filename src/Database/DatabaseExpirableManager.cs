using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Npgsql;
using NpgsqlTypes;

namespace OoLunar.Tomoe.Database
{
    public sealed class DatabaseExpirableManager<TSelf, TId>
        where TSelf : IExpirableModel<TSelf, TId>
        where TId : ISpanParsable<TId>
    {
        private readonly NpgsqlCommand _findExpirableCommand;
        private readonly NpgsqlCommand _deleteExpirableCommand;
        private readonly NpgsqlCommand _getExpirableCommand;
        private readonly NpgsqlCommand _updateExpirableCommand;
        private readonly SemaphoreSlim _semaphore = new(1, 1);

        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<DatabaseExpirableManager<TSelf, TId>> _logger;
        private readonly PeriodicTimer _expireTimer;
        private readonly Dictionary<TId, DateTimeOffset> _expirableCache = [];

        public DatabaseExpirableManager(IServiceProvider serviceProvider, IConfiguration configuration, DatabaseConnectionManager connectionManager, ILogger<DatabaseExpirableManager<TSelf, TId>>? logger = null)
        {
            ArgumentNullException.ThrowIfNull(serviceProvider, nameof(serviceProvider));
            ArgumentNullException.ThrowIfNull(configuration, nameof(configuration));
            ArgumentNullException.ThrowIfNull(connectionManager, nameof(connectionManager));
            _serviceProvider = serviceProvider;
            _expireTimer = new(TimeSpan.FromSeconds(configuration.GetValue("database:expire_interval", 30)));
            _logger = logger ?? NullLogger<DatabaseExpirableManager<TSelf, TId>>.Instance;

            NpgsqlConnection connection = connectionManager.GetConnection();
            connection.Open();
            _findExpirableCommand = new($"SELECT id, expires_at FROM {TSelf.TableName} WHERE expires_at > @now", connection);
            _findExpirableCommand.Parameters.Add(new NpgsqlParameter("table", NpgsqlDbType.Text) { Value = TSelf.TableName });
            _findExpirableCommand.Parameters.Add(new NpgsqlParameter("now", NpgsqlDbType.TimestampTz));
            _deleteExpirableCommand = new($"DELETE FROM {TSelf.TableName} WHERE id = @id", connection);
            _deleteExpirableCommand.Parameters.Add(new NpgsqlParameter("table", NpgsqlDbType.Text) { Value = TSelf.TableName });
            _deleteExpirableCommand.Parameters.Add(new NpgsqlParameter("id", NpgsqlDbType.Text));
            _getExpirableCommand = new($"SELECT * FROM {TSelf.TableName} WHERE id = @id", connection);
            _getExpirableCommand.Parameters.Add(new NpgsqlParameter("table", NpgsqlDbType.Text) { Value = TSelf.TableName });
            _getExpirableCommand.Parameters.Add(new NpgsqlParameter("id", NpgsqlDbType.Text));
            _updateExpirableCommand = new($"UPDATE {TSelf.TableName} SET expires_at = @expires_at WHERE id = @id", connection);
            _updateExpirableCommand.Parameters.Add(new NpgsqlParameter("table", NpgsqlDbType.Text) { Value = TSelf.TableName });
            _updateExpirableCommand.Parameters.Add(new NpgsqlParameter("id", NpgsqlDbType.Text));
            _updateExpirableCommand.Parameters.Add(new NpgsqlParameter("expires_at", NpgsqlDbType.TimestampTz));

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

                // Ensure that we'll always get the expirables that have expired and that will expire soon
                _findExpirableCommand.Parameters["now"].Value = DateTimeOffset.UtcNow + _expireTimer.Period;

                // Asynchronously read the expirables from the database
                await using NpgsqlDataReader reader = await _findExpirableCommand.ExecuteReaderAsync();
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
                foreach ((TId id, DateTimeOffset expiresAt) in _expirableCache)
                {
                    // If the expirable is not expired, continue to the next one
                    if (DateTimeOffset.UtcNow >= expiresAt)
                    {
                        await ExpireExpirableAsync(id);
                    }
                }

                _semaphore.Release();
            }
            while (await expireCacheTimer.WaitForNextTickAsync());
        }

        private async ValueTask ExpireExpirableAsync(TId id)
        {
            // Fetch the latest information from the database
            _getExpirableCommand.Parameters["id"].Value = id.ToString();
            NpgsqlDataReader reader = await _getExpirableCommand.ExecuteReaderAsync();
            if (!await reader.ReadAsync() || !TSelf.TryParse(reader, out TSelf? expirable))
            {
                return;
            }
            else if (!IsExpired(expirable.ExpiresAt))
            {
                await UpdateExpirationAsync(id, expirable.ExpiresAt);
                return;
            }

            bool shouldDelete = true;
            try
            {
                _logger.LogTrace("Expiring expirable with ID {Id}", id);
                shouldDelete = await TSelf.ExpireAsync(expirable, _serviceProvider);
            }
            catch (Exception error)
            {
                _logger.LogError(error, "Failed to expire expirable with ID {Id}", id);
            }
            finally
            {
                await reader.DisposeAsync();
                if (!shouldDelete)
                {
                    _logger.LogDebug("Postponing expiration of expirable with ID {Id} for another 15 minutes", id);
                    await UpdateExpirationAsync(id, expirable.ExpiresAt);
                }
                else
                {
                    _logger.LogTrace("Removing expirable with ID {Id} from the cache and database", id);
                    _expirableCache.Remove(id);
                    _deleteExpirableCommand.Parameters["id"].Value = id.ToString();
                    await _deleteExpirableCommand.ExecuteNonQueryAsync();
                }
            }
        }

        private async ValueTask UpdateExpirationAsync(TId id, DateTimeOffset expiresAt)
        {
            _updateExpirableCommand.Parameters["id"].Value = id.ToString();
            _updateExpirableCommand.Parameters["expires_at"].Value = expiresAt + TimeSpan.FromMinutes(15);
            await _updateExpirableCommand.ExecuteNonQueryAsync();
            _expirableCache[id] = expiresAt;
        }

        private static bool IsExpired(DateTimeOffset expiresAt) => DateTimeOffset.UtcNow > expiresAt;
        private static bool WillExpireSoon(DateTimeOffset expiresAt, TimeSpan timeSpan) => expiresAt - DateTimeOffset.UtcNow < timeSpan;
    }
}
