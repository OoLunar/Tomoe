using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
        private readonly string _connectionString;
        private readonly ILogger<ExpirableService<T>> _logger;
        private readonly MemoryCacheService _cache;
        private readonly PeriodicTimer _periodicTimer;
        private readonly Dictionary<PreparedStatementType, DbCommand> _preparedStatements = new();
        private readonly Dictionary<string, Func<object, object?>> _propertyGetDelegateCache = new();
        private readonly Dictionary<string, Action<object, object?>> _propertySetDelegateCache = new();
        private readonly SemaphoreSlim _commandLock = new(1, 1);

        public ExpirableService(IServiceProvider serviceProvider, DatabaseContext databaseContext, ILogger<ExpirableService<T>> logger)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _connectionString = (databaseContext ?? throw new ArgumentNullException(nameof(databaseContext))).Database.GetConnectionString() ?? throw new InvalidOperationException("Database connection string is null.");
            _cache = new MemoryCacheService();
            _periodicTimer = new PeriodicTimer(TimeSpan.FromMinutes(1));
            _ = ExpireTimerAsync();
        }

        public async Task<T> AddAsync(T expirable)
        {
            DbCommand createStatement = _preparedStatements[PreparedStatementType.Create];

            // Iterate through the object's properties, setting the values of the prepared statement's parameters.
            foreach (KeyValuePair<string, Func<object, object?>> kvp in _propertyGetDelegateCache)
            {
                createStatement.Parameters[$"@{kvp.Key}"].Value = kvp.Value(expirable);
            }

            // Lock the command so that only one command can be executed at a time.
            // This is a poor attempt to make this class thread safe.
            await _commandLock.WaitAsync();
            await ExecutePreparedStatementAsync(createStatement, PreparedStatementType.Create);
            _cache.Set(expirable.Id, expirable, CacheExpiration(expirable.ExpiresAt), (obj) => Task.Run(async () => await ExpireItemAsync((T)obj)));
            _commandLock.Release();

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

            await _commandLock.WaitAsync();
            DbDataReader reader = await ExecutePreparedStatementAsync(selectByIdStatement, PreparedStatementType.SelectById) ?? throw new InvalidOperationException("Failed to get reader from select by id statement.");
            if (!reader.HasRows || !reader.Read())
            {
                await reader.DisposeAsync();
                _commandLock.Release();
                return null;
            }

            expirable = Activator.CreateInstance<T>();
            foreach (KeyValuePair<string, Action<object, object?>> kvp in _propertySetDelegateCache)
            {
                kvp.Value(expirable, reader[kvp.Key]!);
            }

            await reader.DisposeAsync();
            if (expirable != null)
            {
                // If the item has already expired, remove it from the database, call the expiry method and return null.
                // This is an edge case that really shouldn't happen but it's better to be safe than sorry.
                if (expirable.ExpiresAt <= DateTime.UtcNow)
                {
                    _commandLock.Release();
                    await RemoveAsync(expirable.Id);
                    await ExpireItemAsync(expirable);
                    return null;
                }

                // Cache the result for the first if statement at the top of the method.
                _cache.Set(expirable.Id, expirable, CacheExpiration(expirable.ExpiresAt), (obj) => Task.Run(async () => await ExpireItemAsync((T)obj)));
            }

            _commandLock.Release();
            return expirable;
        }

        public async Task UpdateAsync(T expirable)
        {
            DbCommand updateByIdStatement = _preparedStatements[PreparedStatementType.UpdateById];
            foreach (KeyValuePair<string, Func<object, object?>> kvp in _propertyGetDelegateCache)
            {
                updateByIdStatement.Parameters[$"@{kvp.Key}"].Value = kvp.Value(expirable);
            }

            await _commandLock.WaitAsync();
            await ExecutePreparedStatementAsync(updateByIdStatement, PreparedStatementType.UpdateById);
            _cache.Set(expirable.Id, expirable, CacheExpiration(expirable.ExpiresAt), (obj) => Task.Run(async () => await ExpireItemAsync((T)obj)));
            _commandLock.Release();

            _logger.LogDebug("Updated expirable {ExpirableId} with expiration {ExpiresAt}.", expirable.Id, expirable.ExpiresAt);
        }

        public async Task RemoveAsync(Guid id)
        {
            DbCommand deleteByIdStatement = _preparedStatements[PreparedStatementType.DeleteById];
            deleteByIdStatement.Parameters["@Id"].Value = id;

            await _commandLock.WaitAsync();
            await ExecutePreparedStatementAsync(deleteByIdStatement, PreparedStatementType.DeleteById);
            _cache.TryRemove(id, out _);
            _commandLock.Release();

            _logger.LogDebug("Removed expirable {ExpirableId}.", id);
        }

        private async Task ExpireItemAsync(T expirable)
        {
            // Might need to be removed from the cache if it's expiry date is long in the future.
            if (expirable.ExpiresAt > DateTimeOffset.UtcNow)
            {
                return;
            }

            DiscordShardedClient client = _serviceProvider.GetRequiredService<DiscordShardedClient>();
            while (client.ShardClients.Any(x => x.Value.Guilds.Count == 0))
            {
                await Task.Delay(TimeSpan.FromMilliseconds(50));
            }

            try
            {
                await RemoveAsync(expirable.Id);
                await expirable.ExpireAsync(_serviceProvider);
            }
            catch (Exception error)
            {
                _logger.LogError(error, "Item {ExpirableId} threw an exception.", expirable.Id);
            }
            _logger.LogInformation("Expirable {ExpirableId} has expired from the database.", expirable.Id);
        }

        private async Task ExpireTimerAsync()
        {
            await PrepareStatementsAsync();
            _logger.LogInformation("Starting the expirable service for {ItemType}.", typeof(T).FullName);

            // Checks if the cancellation token has been cancelled.
            do
            {
                await _commandLock.WaitAsync();
                DbCommand findExpired = _preparedStatements[PreparedStatementType.SelectByExpiresAt];
                DbDataReader reader = await ExecutePreparedStatementAsync(findExpired, PreparedStatementType.SelectByExpiresAt) ?? throw new InvalidOperationException("The database reader is null.");

                if (!reader.HasRows)
                {
                    await reader.DisposeAsync();
                    _commandLock.Release();
                    continue;
                }

                List<T> expirables = new();
                while (reader.Read())
                {
                    T expirable = Activator.CreateInstance<T>();
                    foreach (KeyValuePair<string, Action<object, object?>> kvp in _propertySetDelegateCache)
                    {
                        kvp.Value(expirable, reader[kvp.Key]!);
                    }
                    expirables.Add(expirable);
                }

                await reader.DisposeAsync();
                _commandLock.Release();

                await Parallel.ForEachAsync(expirables, async (expirable, cancellationToken) =>
                {
                    if (expirable.ExpiresAt <= DateTimeOffset.Now)
                    {
                        await ExpireItemAsync(expirable);
                    }
                    else if (!_cache.TryGetValue(expirable.Id, out _))
                    {
                        _cache.Set(expirable.Id, expirable, CacheExpiration(expirable.ExpiresAt), (obj) => Task.Run(async () => await ExpireItemAsync((T)obj)));
                    }
                });
            } while (await _periodicTimer.WaitForNextTickAsync());
        }

        private async Task PrepareStatementsAsync()
        {
            // Check ahead of time to see if the type has a parameterless constructor.
            // This allows us to skip a bunch of null checks and prevents runtime errors in other methods.
            try
            {
                _ = Activator.CreateInstance<T>();
            }
            catch (MissingMethodException)
            {
                throw new InvalidOperationException("The type does not have a parameterless constructor.");
            }

            NpgsqlConnection connection = new(_connectionString);
            await connection.OpenAsync();

            string typeName = typeof(DatabaseContext).GetProperties().First(property => property.PropertyType == typeof(DbSet<T>)).Name;
            DbCommand selectByIdCommand = connection.CreateCommand();
            DbParameter selectIdParameter = selectByIdCommand.CreateParameter();
            selectIdParameter.DbType = DbType.Guid;
            selectIdParameter.ParameterName = "@Id";
            selectIdParameter.SourceColumn = "Id";
            selectByIdCommand.CommandText = $"SELECT * FROM \"{typeName}\" WHERE \"Id\" = @Id";
            selectByIdCommand.Parameters.Add(selectIdParameter);
            await selectByIdCommand.PrepareAsync();
            _preparedStatements.Add(PreparedStatementType.SelectById, selectByIdCommand);

            DbCommand selectByExpiresAtCommand = connection.CreateCommand();
            DbParameter expiresAtParameter = selectByExpiresAtCommand.CreateParameter();
            expiresAtParameter.DbType = DbType.DateTimeOffset;
            expiresAtParameter.ParameterName = "@ExpiresAt";
            expiresAtParameter.SourceColumn = "ExpiresAt";
            selectByExpiresAtCommand.CommandText = $"SELECT * FROM \"{typeName}\" WHERE \"ExpiresAt\" <= now() + '5 days'";
            await selectByExpiresAtCommand.PrepareAsync();
            _preparedStatements.Add(PreparedStatementType.SelectByExpiresAt, selectByExpiresAtCommand);

            DbCommand deleteByIdCommand = connection.CreateCommand();
            DbParameter deleteIdParameter = selectByIdCommand.CreateParameter();
            deleteIdParameter.DbType = DbType.Guid;
            deleteIdParameter.ParameterName = "@Id";
            deleteIdParameter.SourceColumn = "Id";
            deleteByIdCommand.Parameters.Add(deleteIdParameter);
            deleteByIdCommand.CommandText = $"DELETE FROM \"{typeName}\" WHERE \"Id\" = @Id";
            await deleteByIdCommand.PrepareAsync();
            _preparedStatements.Add(PreparedStatementType.DeleteById, deleteByIdCommand);

            DbCommand createCommand = connection.CreateCommand();
            StringBuilder createCommandStringBuilder = new();
            createCommandStringBuilder.Append($"INSERT INTO \"{typeName}\" VALUES (");

            NpgsqlCommand updateByIdCommand = new(null, connection);
            StringBuilder updateCommandColumnStringBuilder = new();
            updateCommandColumnStringBuilder.Append($"UPDATE \"{typeName}\" SET (");
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
                string columnName = columnAttribute?.Name ?? property.Name;

                NpgsqlParameter createParameter = updateByIdCommand.CreateParameter();
                NpgsqlParameter updateParameter = updateByIdCommand.CreateParameter();
                createParameter.ParameterName = updateParameter.ParameterName = columnName;

                NpgsqlDbType dataType = 0;
                if (columnAttribute is not null && columnAttribute.TypeName is not null)
                {
                    switch (columnAttribute.TypeName.ToLowerInvariant())
                    {
                        case "json":
                            dataType = NpgsqlDbType.Json;

                            // Convert the values here to prevent if statements when getting/setting the values
                            _propertyGetDelegateCache.Add(columnName, (obj) => JsonSerializer.Serialize(property.GetValue(obj)!));
                            _propertySetDelegateCache.Add(columnName, (obj, value) => property.SetValue(obj, JsonSerializer.Deserialize(value!.ToString()!, property.PropertyType)));
                            break;
                        case "jsonb":
                            dataType = NpgsqlDbType.Jsonb;

                            // Convert the values here to prevent if statements when getting/setting the values
                            _propertyGetDelegateCache.Add(columnName, (obj) => JsonSerializer.Serialize(property.GetValue(obj)!));
                            _propertySetDelegateCache.Add(columnName, (obj, value) => property.SetValue(obj, JsonSerializer.Deserialize(value!.ToString()!, property.PropertyType)));
                            break;
                        default:
                            throw new NotImplementedException($"The type {columnAttribute.TypeName} is not implemented.");
                    }
                }
                else
                {
                    Type type = property.PropertyType;

                    // Get the base type
                    if (property.PropertyType.IsArray)
                    {
                        dataType |= NpgsqlDbType.Array;
                        type = property.PropertyType.GetElementType()!;
                    }

                    switch (type)
                    {
                        case Type when type == typeof(string):
                            dataType |= NpgsqlDbType.Text;
                            break;
                        case Type when type == typeof(Guid):
                            dataType |= NpgsqlDbType.Uuid;
                            break;
                        case Type when type == typeof(ulong):
                            dataType |= NpgsqlDbType.Numeric;

                            // Convert the values here to prevent if statements when getting/setting the values
                            _propertyGetDelegateCache.Add(columnName, (obj) => Convert.ToInt64(property.GetValue(obj)));
                            _propertySetDelegateCache.Add(columnName, (obj, value) => property.SetValue(obj, Convert.ToUInt64(value)));
                            break;
                        case Type when type == typeof(DateTimeOffset):
                            dataType |= NpgsqlDbType.TimestampTz;
                            break;
                        case Type when type == typeof(DateTime):
                            dataType |= NpgsqlDbType.TimestampTz;
                            break;
                        default:
                            throw new NotImplementedException($"Type {property.PropertyType} does not have a database conversion implemented.");
                    }
                }

                _propertyGetDelegateCache.TryAdd(columnName, property.GetValue);
                _propertySetDelegateCache.TryAdd(columnName, property.SetValue);

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
            await createCommand.PrepareAsync();
            _preparedStatements.Add(PreparedStatementType.Create, createCommand);

            updateCommandColumnStringBuilder.Remove(updateCommandColumnStringBuilder.Length - 2, 2);
            updateCommandValueStringBuilder.Remove(updateCommandValueStringBuilder.Length - 2, 2);
            updateCommandValueStringBuilder.Append(") WHERE \"Id\" = @Id;");
            updateByIdCommand.CommandText = updateCommandColumnStringBuilder.Append(updateCommandValueStringBuilder).ToString();
            await updateByIdCommand.PrepareAsync();
            _preparedStatements.Add(PreparedStatementType.UpdateById, updateByIdCommand);
        }

        private async Task<DbDataReader?> ExecutePreparedStatementAsync(DbCommand command, PreparedStatementType statementType, bool isRetry = false)
        {
            try
            {
                switch (statementType)
                {
                    case PreparedStatementType.Create:
                    case PreparedStatementType.UpdateById:
                    case PreparedStatementType.DeleteById:
                        await command.ExecuteNonQueryAsync();
                        return null;
                    case PreparedStatementType.SelectById:
                    case PreparedStatementType.SelectByExpiresAt:
                        return await command.ExecuteReaderAsync();
                    default:
                        throw new NotImplementedException($"Statement type {statementType} does not have a database execution strategy implemented.");
                }
            }
            catch (DbException)
            {
                if (isRetry)
                {
                    throw;
                }

                // The connection is probably dead, so we'll try to reconnect and retry the statement once with a new connection.
                DbConnection connection = new NpgsqlConnection(_connectionString);
                await connection.OpenAsync();
                await Parallel.ForEachAsync(_preparedStatements, async (statement, cancellationToken) => await statement.Value.PrepareAsync(cancellationToken));
                return await ExecutePreparedStatementAsync(command, statementType, true);
            }
        }

        private DateTimeOffset CacheExpiration(DateTimeOffset expiresAt)
        {
            DateTimeOffset expiresInCache = DateTimeOffset.UtcNow.AddMinutes(5);
            return expiresAt > expiresInCache ? expiresInCache : expiresAt;
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
