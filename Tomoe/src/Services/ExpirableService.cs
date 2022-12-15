using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Npgsql;
using NpgsqlTypes;
using OoLunar.Tomoe.Database;
using OoLunar.Tomoe.Interfaces;

namespace OoLunar.Tomoe.Services
{
    /// <summary>
    /// Manages a list of objects which expire at are removed from the database after a certain amount of time.
    /// </summary>
    /// <typeparam name="T">The type of object keep track of.</typeparam>
    public sealed class ExpirableService<T> where T : class, IExpirable<T>, new()
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly DatabaseContext _databaseContext;
        private readonly ILogger<ExpirableService<T>> _logger;
        private readonly MemoryCache _cache;
        private readonly PeriodicTimer _periodicTimer;
        private readonly Dictionary<PreparedStatementType, DbCommand> _preparedStatements = new();
        private readonly Dictionary<string, Func<object, object?>> _propertyGetDelegateCache = new();
        private readonly Dictionary<string, Action<object, object?>> _propertySetDelegateCache = new();

        public ExpirableService(IServiceProvider serviceProvider, DatabaseContext databaseContext, ILogger<ExpirableService<T>> logger)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _databaseContext = databaseContext ?? throw new ArgumentNullException(nameof(databaseContext));
            _cache = new MemoryCache(new MemoryCacheOptions() { ExpirationScanFrequency = TimeSpan.FromMinutes(2) });

            _databaseContext.Database.OpenConnection();
            DbConnection connection = _databaseContext.Database.GetDbConnection();

            string typeName = $"\"{typeof(DatabaseContext).GetProperties().First(property => property.PropertyType == typeof(DbSet<T>) && property.PropertyType.GenericTypeArguments[0] == typeof(T)).Name}\"";
            DbCommand selectByIdCommand = connection.CreateCommand();
            DbParameter selectIdParameter = selectByIdCommand.CreateParameter();
            selectIdParameter.ParameterName = "@Id";
            selectIdParameter.DbType = DbType.Guid;
            selectIdParameter.SourceColumn = "Id";

            selectByIdCommand.CommandText = $"SELECT * FROM {typeName} WHERE \"Id\" = @Id";
            selectByIdCommand.Parameters.Add(selectIdParameter);
            selectByIdCommand.Prepare();
            _preparedStatements.Add(PreparedStatementType.SelectById, selectByIdCommand);

            DbCommand selectByExpiresAtCommand = connection.CreateCommand();
            DbParameter expiresAtParameter = selectByExpiresAtCommand.CreateParameter();
            expiresAtParameter.ParameterName = "@ExpiresAt";
            expiresAtParameter.DbType = DbType.DateTimeOffset;
            expiresAtParameter.SourceColumn = "ExpiresAt";

            selectByExpiresAtCommand.CommandText = $"SELECT * FROM {typeName} WHERE \"ExpiresAt\" <= @ExpiresAt";
            selectByExpiresAtCommand.Parameters.Add(expiresAtParameter);
            selectByExpiresAtCommand.Prepare();
            _preparedStatements.Add(PreparedStatementType.SelectByExpiresAt, selectByExpiresAtCommand);

            DbCommand deleteByIdCommand = connection.CreateCommand();
            DbParameter deleteIdParameter = selectByIdCommand.CreateParameter();
            deleteIdParameter.ParameterName = "@Id";
            deleteIdParameter.DbType = DbType.Guid;
            deleteIdParameter.SourceColumn = "Id";

            deleteByIdCommand.Parameters.Add(deleteIdParameter);
            deleteByIdCommand.CommandText = $"DELETE FROM {typeName} WHERE \"Id\" = @Id";
            deleteByIdCommand.Prepare();
            _preparedStatements.Add(PreparedStatementType.DeleteById, deleteByIdCommand);

            DbCommand createCommand = connection.CreateCommand();
            NpgsqlCommand updateByIdCommand = new(null, connection as NpgsqlConnection);

            StringBuilder createCommandStringBuilder = new();
            createCommandStringBuilder.Append($"INSERT INTO {typeName} VALUES (");

            StringBuilder updateCommandColumnStringBuilder = new();
            updateCommandColumnStringBuilder.Append($"UPDATE {typeName} SET (");
            StringBuilder updateCommandValueStringBuilder = new();
            updateCommandValueStringBuilder.Append(") = (");

            PropertyInfo[] properties = typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty);
            foreach (PropertyInfo property in properties)
            {
                if (property.GetCustomAttribute<NotMappedAttribute>() is not null)
                {
                    continue;
                }

                ColumnAttribute? columnAttribute = property.GetCustomAttribute<ColumnAttribute>();
                _propertyGetDelegateCache.Add(columnAttribute?.Name ?? property.Name, property.GetValue);
                _propertySetDelegateCache.Add(columnAttribute?.Name ?? property.Name, property.SetValue);

                NpgsqlParameter createParameter = updateByIdCommand.CreateParameter();
                NpgsqlParameter updateParameter = updateByIdCommand.CreateParameter();
                createParameter.ParameterName = updateParameter.ParameterName = columnAttribute?.Name ?? property.Name;

                NpgsqlDbType dataType = columnAttribute?.TypeName?.ToLowerInvariant() switch
                {
                    "json" => NpgsqlDbType.Json,
                    "jsonb" => NpgsqlDbType.Jsonb,
                    _ => 0
                };

                if (dataType is 0)
                {
                    Type type = property.PropertyType;
                    if (property.PropertyType.IsArray)
                    {
                        dataType |= NpgsqlDbType.Array;
                        type = property.PropertyType.GetElementType()!;
                    }

                    dataType |= type switch
                    {
                        Type when type == typeof(string) => NpgsqlDbType.Text,
                        Type when type == typeof(Guid) => NpgsqlDbType.Uuid,
                        Type when type == typeof(ulong) => NpgsqlDbType.Numeric,
                        Type when type == typeof(DateTimeOffset) => NpgsqlDbType.TimestampTz,
                        Type when type == typeof(DateTime) => NpgsqlDbType.TimestampTz,
                        _ => throw new NotImplementedException($"Type {property.PropertyType} does not have a database conversion implemented.")
                    };
                }

                createParameter.NpgsqlDbType = updateParameter.NpgsqlDbType = dataType;
                updateParameter.SourceColumn = createParameter.ParameterName;

                createCommand.Parameters.Add(createParameter);
                updateByIdCommand.Parameters.Add(updateParameter);
                createCommandStringBuilder.Append($"@{createParameter.ParameterName}, ");
                updateCommandColumnStringBuilder.Append($"\"{updateParameter.SourceColumn}\", ");
                updateCommandValueStringBuilder.Append($"@{createParameter.ParameterName}, ");
            }

            createCommandStringBuilder.Remove(createCommandStringBuilder.Length - 2, 2);
            createCommandStringBuilder.Append(");");
            createCommand.CommandText = createCommandStringBuilder.ToString();
            createCommand.Prepare();
            _preparedStatements.Add(PreparedStatementType.Create, createCommand);

            updateCommandColumnStringBuilder.Remove(updateCommandColumnStringBuilder.Length - 2, 2);
            updateCommandValueStringBuilder.Remove(updateCommandValueStringBuilder.Length - 2, 2);
            updateCommandValueStringBuilder.Append(") WHERE \"Id\" = @Id;");

            updateByIdCommand.CommandText = updateCommandColumnStringBuilder.Append(updateCommandValueStringBuilder).ToString();
            updateByIdCommand.Prepare();
            _preparedStatements.Add(PreparedStatementType.UpdateById, updateByIdCommand);

            _periodicTimer = new PeriodicTimer(TimeSpan.FromMinutes(1));
            _ = ExpireTimerAsync();
        }

        public async Task<T> AddAsync(T expirable)
        {
            DbCommand createStatement = _preparedStatements[PreparedStatementType.Create];
            foreach (KeyValuePair<string, Func<object, object?>> kvp in _propertyGetDelegateCache)
            {
                object? value = kvp.Value(expirable);
                if (value is ulong uValue)
                {
                    value = Unsafe.As<ulong, long>(ref uValue);
                }
                createStatement.Parameters[$"@{kvp.Key}"].Value = value;
            }
            await createStatement.ExecuteNonQueryAsync();

            lock (_cache)
            {
                _cache.Remove(expirable.Id);
                _cache.Set(expirable.Id, expirable, CreateCancellationChangeToken(expirable));
            }

            _logger.LogDebug("Added expirable {ExpirableId} with expiration {ExpiresAt}.", expirable.Id, expirable.ExpiresAt);
            return expirable;
        }

        public async Task<T?> GetAsync(Guid id)
        {
            if (_cache.TryGetValue(id, out T? expirable))
            {
                return expirable;
            }

            DbCommand selectByIdStatement = _preparedStatements[PreparedStatementType.SelectById];
            selectByIdStatement.Parameters["@Id"].Value = id;
            using DbDataReader reader = await selectByIdStatement.ExecuteReaderAsync();
            if (reader.Read())
            {
                expirable = (T)Activator.CreateInstance(typeof(T), true)!;
                foreach (KeyValuePair<string, Action<object, object?>> kvp in _propertySetDelegateCache)
                {
                    PropertyInfo property = (PropertyInfo)kvp.Value.Target!;
                    if (property.PropertyType == typeof(ulong) && reader[kvp.Key] is decimal dValue)
                    {
                        kvp.Value(expirable, (ulong)dValue);
                    }
                    else if (property.PropertyType.GetInterfaces().Contains(typeof(IDictionary)))
                    {
                        kvp.Value(expirable, JsonSerializer.Deserialize(reader[kvp.Key].ToString()!, ((PropertyInfo)kvp.Value.Target!).PropertyType)!);
                    }
                    else
                    {
                        kvp.Value(expirable, reader[kvp.Key]!);
                    }
                }
            }

            if (expirable != null)
            {
                lock (_cache)
                {
                    _cache.Remove(expirable.Id);
                    _cache.Set(expirable.Id, expirable, CreateCancellationChangeToken(expirable));
                }
            }

            return expirable;
        }

        public async Task UpdateAsync(T expirable)
        {
            DbCommand updateByIdStatement = _preparedStatements[PreparedStatementType.UpdateById];
            foreach (KeyValuePair<string, Func<object, object?>> kvp in _propertyGetDelegateCache)
            {
                object? value = kvp.Value(expirable);
                if (value is ulong uValue)
                {
                    value = Unsafe.As<ulong, long>(ref uValue);
                }
                updateByIdStatement.Parameters[$"@{kvp.Key}"].Value = value;
            }
            await updateByIdStatement.ExecuteNonQueryAsync();

            lock (_cache)
            {
                _cache.Remove(expirable.Id);
                _cache.Set(expirable.Id, expirable, CreateCancellationChangeToken(expirable));
            }
            _logger.LogDebug("Updated expirable {ExpirableId} with expiration {ExpiresAt}.", expirable.Id, expirable.ExpiresAt);
        }

        public async Task RemoveAsync(Guid id)
        {
            DbCommand deleteByIdStatement = _preparedStatements[PreparedStatementType.DeleteById];
            deleteByIdStatement.Parameters["@Id"].Value = id;
            await deleteByIdStatement.ExecuteNonQueryAsync();
            _cache.Remove(id);

            _logger.LogDebug("Removed expirable {ExpirableId}.", id);
        }

        private CancellationChangeToken CreateCancellationChangeToken(T expirable)
        {
            DateTimeOffset now = DateTimeOffset.Now;
            if (expirable.ExpiresAt <= now)
            {
                throw new ArgumentException("The expirable item has already expired.", nameof(expirable));
            }

            // TODO: `expirable.ExpiresAt - now` will throw on expired items. Find a way to prune these expired items before they're added to the cache.
            CancellationChangeToken cct = new(new CancellationTokenSource(expirable.ExpiresAt - now).Token);
            cct.RegisterChangeCallback(async expirableObject =>
            {
                T expirable = (T)expirableObject!;
                await ExpireItemAsync(expirable);
            }, expirable);
            return cct;
        }

        private async Task ExpireTimerAsync()
        {
            _logger.LogInformation("Starting the expirable service for {ItemType}.", typeof(T).FullName);

            // Checks if the cancellation token has been cancelled.
            while (await _periodicTimer.WaitForNextTickAsync())
            {
                DateTime expireTime = DateTime.UtcNow.AddMinutes(5);
                foreach (T expirable in _databaseContext.Set<T>().AsNoTracking().Where(item => item.ExpiresAt <= expireTime))
                {
                    if (expirable.ExpiresAt <= DateTimeOffset.Now)
                    {
                        await ExpireItemAsync(expirable);
                    }
                    else
                    {
                        _cache.Set(expirable.Id, expirable, CreateCancellationChangeToken(expirable));
                    }
                }
            }
        }

        private async Task ExpireItemAsync(T expirable)
        {
            if (expirable.ExpiresAt > DateTimeOffset.Now)
            {
                // MemoryCache calls this callback when the item has been both updated and removed. We only want to call ExpireAsync when it has been removed.
                return;
            }

            DiscordShardedClient client = _serviceProvider.GetRequiredService<DiscordShardedClient>();
            if (client.ShardClients.Count == 0)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(100));
                return;
            }

            _cache.Remove(expirable.Id);
            await expirable.IsExecuting.WaitAsync();
            if (expirable.HasExecuted)
            {
                return;
            }

            try
            {
                await RemoveAsync(expirable.Id);
                await expirable.ExpireAsync(_serviceProvider);
                expirable.HasExecuted = true;
            }
            catch (Exception error)
            {
                _logger.LogError(error, "Item {ExpirableId} threw an exception.", expirable.Id);
            }
            expirable.IsExecuting.Release();
            _logger.LogInformation("Poll {PollId} has expired from the cache.", expirable.Id);
        }

        private enum PreparedStatementType
        {
            Create,
            SelectById,
            SelectByExpiresAt,
            UpdateById,
            DeleteById,
        }
    }
}
