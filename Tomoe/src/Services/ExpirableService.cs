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
        /// <summary>
        /// The service provider passed to <see cref="IExpirable{T}.ExpireAsync(IServiceProvider)"/>.
        /// </summary>
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// The logger used to log errors.
        /// </summary>
        private readonly ILogger<ExpirableService<T>> _logger;

        /// <summary>
        /// The cache used to store the objects. Should only contain objects that will expire within the next 5 minutes.
        /// </summary>
        private readonly MemoryCacheService _cache;

        /// <summary>
        /// A timer that pulls nearly expired objects from the database and adds them to the cache.
        /// </summary>
        private readonly PeriodicTimer _periodicTimer;

        /// <summary>
        /// The connection string to the database. Used to create a new connection as needed.
        /// </summary>
        private readonly string _connectionString;

        /// <summary>
        /// A dictionary of prepared statements, used to execute queries faster.
        /// </summary>
        private readonly Dictionary<PreparedStatementType, DbCommand> _preparedStatements = new();

        /// <summary>
        /// A dictionary of property get delegates, used to get property values faster. These delegates are cached to avoid reflection and also contain conversion logic for converting from the database type to the property type.
        /// </summary>
        private readonly Dictionary<string, Func<object, object?>> _propertyGetDelegateCache = new();

        /// <summary>
        /// A dictionary of property set delegates, used to set property values faster. These delegates are cached to avoid reflection and also contain conversion logic for converting from the property type to the database type.
        /// </summary>
        private readonly Dictionary<string, Action<object, object?>> _propertySetDelegateCache = new();

        /// <summary>
        /// A lock used to prevent multiple commands from being executed at the same time. This is a poor attempt to make this class thread safe.
        /// </summary>
        private readonly SemaphoreSlim _commandLock = new(1, 1);

        /// <summary>
        /// Creates a new <see cref="ExpirableService{T}"/>.
        /// </summary>
        /// <param name="serviceProvider">The service provider passed to <see cref="IExpirable{T}.ExpireAsync(IServiceProvider)"/>.</param>
        /// <param name="databaseContext">The database context used to create the database connection.</param>
        /// <param name="logger">The logger used to log errors.</param>
        public ExpirableService(IServiceProvider serviceProvider, DatabaseContext databaseContext, ILogger<ExpirableService<T>> logger)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _cache = new MemoryCacheService();
            _periodicTimer = new PeriodicTimer(TimeSpan.FromMinutes(1));
            _connectionString = (databaseContext ?? throw new ArgumentNullException(nameof(databaseContext))).Database.GetConnectionString() ?? throw new InvalidOperationException("Database connection string is null.");

            // Start the periodic timer, which prepares the statements and pulls nearly expired items from the database into the cache.
            _ = ExpireTimerAsync();
        }

        /// <summary>
        /// Adds an object to the database and cache.
        /// </summary>
        /// <param name="expirable">The object to add.</param>
        public async Task<T> AddAsync(T expirable)
        {
            DbCommand createStatement = _preparedStatements[PreparedStatementType.Create];

            // Iterate through the object's properties, setting the values of the prepared statement's parameters.
            foreach (KeyValuePair<string, Func<object, object?>> kvp in _propertyGetDelegateCache)
            {
                createStatement.Parameters[$"@{kvp.Key}"].Value = kvp.Value(expirable);
            }

            // Thread safety
            await _commandLock.WaitAsync();
            await ExecutePreparedStatementAsync(createStatement, PreparedStatementType.Create);
            _cache.Set(expirable.Id, expirable, CacheExpiration(expirable.ExpiresAt), (obj) => Task.Run(async () => await ExpireItemAsync((T)obj)));
            _commandLock.Release();

            _logger.LogDebug("Added expirable {ExpirableId} with expiration {ExpiresAt}.", expirable.Id, expirable.ExpiresAt);
            return expirable;
        }

        /// <summary>
        /// Gets an object from the database and cache.
        /// </summary>
        /// <param name="id">The id of the object to get.</param>
        /// <returns>The object, or null if it does not exist.</returns>
        public async Task<T?> GetAsync(Guid id)
        {
            // Check the cache first
            if (_cache.TryGetValue(id, out T? expirable))
            {
                return expirable;
            }

            // If the item is not in the cache, get it from the database
            DbCommand selectByIdStatement = _preparedStatements[PreparedStatementType.SelectById];
            selectByIdStatement.Parameters["@Id"].Value = id;

            // Thread safety
            await _commandLock.WaitAsync();
            DbDataReader reader = await ExecutePreparedStatementAsync(selectByIdStatement, PreparedStatementType.SelectById) ?? throw new InvalidOperationException("Failed to get reader from select by id statement.");

            // If the item does not exist, return null
            if (!reader.HasRows || !reader.Read())
            {
                await reader.DisposeAsync();
                _commandLock.Release();
                return null;
            }

            // Create a new object and set its properties
            expirable = new T();
            foreach (KeyValuePair<string, Action<object, object?>> kvp in _propertySetDelegateCache)
            {
                kvp.Value(expirable, reader[kvp.Key]!);
            }

            // Dispose of the reader for other queries to use
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

        /// <summary>
        /// Updates an object in the database and cache.
        /// </summary>
        /// <param name="expirable">The object to update.</param>
        public async Task UpdateAsync(T expirable)
        {
            // Get the update statement, set the parameters and execute it
            DbCommand updateByIdStatement = _preparedStatements[PreparedStatementType.UpdateById];
            foreach (KeyValuePair<string, Func<object, object?>> kvp in _propertyGetDelegateCache)
            {
                updateByIdStatement.Parameters[$"@{kvp.Key}"].Value = kvp.Value(expirable);
            }

            // Thread safety
            await _commandLock.WaitAsync();
            await ExecutePreparedStatementAsync(updateByIdStatement, PreparedStatementType.UpdateById);
            _cache.Set(expirable.Id, expirable, CacheExpiration(expirable.ExpiresAt), (obj) => Task.Run(async () => await ExpireItemAsync((T)obj)));
            _commandLock.Release();

            _logger.LogDebug("Updated expirable {ExpirableId} with expiration {ExpiresAt}.", expirable.Id, expirable.ExpiresAt);
        }

        /// <summary>
        /// Removes an object from the database and cache.
        /// </summary>
        /// <param name="id">The id of the object to remove.</param>
        /// <remarks>
        /// This method does not call the expiry method.
        /// </remarks>
        public async Task RemoveAsync(Guid id)
        {
            // Get the delete statement, set the parameters and execute it
            DbCommand deleteByIdStatement = _preparedStatements[PreparedStatementType.DeleteById];
            deleteByIdStatement.Parameters["@Id"].Value = id;

            // Thread safety
            await _commandLock.WaitAsync();
            await ExecutePreparedStatementAsync(deleteByIdStatement, PreparedStatementType.DeleteById);
            _cache.TryRemove(id, out _);
            _commandLock.Release();

            _logger.LogDebug("Removed expirable {ExpirableId}.", id);
        }

        /// <summary>
        /// Expires an object from the database and cache.
        /// </summary>
        /// <param name="expirable">The object to expire.</param>
        private async Task ExpireItemAsync(T expirable)
        {
            // Ensure the item has actually expired
            if (expirable.ExpiresAt > DateTimeOffset.UtcNow)
            {
                return;
            }

            DiscordShardedClient client = _serviceProvider.GetRequiredService<DiscordShardedClient>();
            while (client.ShardClients.Any(shard => !shard.Value.Guilds.Any()))
            {
                await Task.Delay(TimeSpan.FromMilliseconds(100));
            }

            try
            {
                // Remove the item from the database and cache before calling the expiry method (which could be blocking for a long time)
                await RemoveAsync(expirable.Id);
                await expirable.ExpireAsync(_serviceProvider);
            }
            catch (Exception error)
            {
                _logger.LogError(error, "Item {ExpirableId} threw an exception.", expirable.Id);
            }

            _logger.LogInformation("Expirable {ExpirableId} has expired from the database.", expirable.Id);
        }

        /// <summary>
        /// Starts the timer that expires items.
        /// </summary>
        private async Task ExpireTimerAsync()
        {
            // Prepare the statements, ensure the database is ready
            await PrepareStatementsAsync();
            _logger.LogInformation("Starting the expirable service for {ItemType}.", typeof(T).FullName);

            do
            {
                // Thread safety
                await _commandLock.WaitAsync();

                // Get all items that have expired
                DbCommand findExpired = _preparedStatements[PreparedStatementType.SelectByExpiresAt];
                DbDataReader reader = await ExecutePreparedStatementAsync(findExpired, PreparedStatementType.SelectByExpiresAt) ?? throw new InvalidOperationException("The database reader is null.");

                // If there are no items, release the lock and wait for the next timer tick
                if (!reader.HasRows)
                {
                    await reader.DisposeAsync();
                    _commandLock.Release();
                    continue;
                }

                // Create a list of items that expire within the next X minutes
                List<T> expirables = new();
                while (reader.Read())
                {
                    T expirable = new();

                    // Set the properties of the object
                    foreach (KeyValuePair<string, Action<object, object?>> kvp in _propertySetDelegateCache)
                    {
                        kvp.Value(expirable, reader[kvp.Key]!);
                    }

                    expirables.Add(expirable);
                }

                // Release the lock and dispose of the reader
                await reader.DisposeAsync();
                _commandLock.Release();

                // Iterate over the items and expire them
                await Parallel.ForEachAsync(expirables, async (expirable, cancellationToken) =>
                {
                    // If the item has already expired, expire it
                    if (expirable.ExpiresAt <= DateTimeOffset.Now)
                    {
                        await ExpireItemAsync(expirable);
                    }
                    // If the item has not expired, add it to the cache
                    else if (!_cache.TryGetValue(expirable.Id, out _))
                    {
                        _cache.Set(expirable.Id, expirable, expirable.ExpiresAt, (obj) => Task.Run(async () => await ExpireItemAsync((T)obj)));
                    }
                });
                // Do/while loop to ensure we get rid of expired items immediately on startup
            } while (await _periodicTimer.WaitForNextTickAsync());
        }

        /// <summary>
        /// Prepares the statements for the database.
        /// </summary>
        private async Task PrepareStatementsAsync()
        {
            NpgsqlConnection connection = new(_connectionString);
            await connection.OpenAsync();

            // Get the table name from EFCore
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

            // Auto generate the create/update SQL statements by iterating over the type's properties
            DbCommand createCommand = connection.CreateCommand();
            StringBuilder createCommandStringBuilder = new();
            createCommandStringBuilder.Append($"INSERT INTO \"{typeName}\" VALUES (");

            NpgsqlCommand updateByIdCommand = new(null, connection);
            StringBuilder updateCommandColumnStringBuilder = new();
            updateCommandColumnStringBuilder.Append($"UPDATE \"{typeName}\" SET (");
            StringBuilder updateCommandValueStringBuilder = new();
            updateCommandValueStringBuilder.Append(") = (");

            // Grab all public properties that are not marked with the NotMapped attribute
            PropertyInfo[] properties = typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.GetProperty);
            foreach (PropertyInfo property in properties)
            {
                if (property.GetCustomAttribute<NotMappedAttribute>() is not null)
                {
                    continue;
                }

                // Get the column name from the Column attribute, or use the property name if it doesn't exist
                ColumnAttribute? columnAttribute = property.GetCustomAttribute<ColumnAttribute>();
                string columnName = columnAttribute?.Name ?? property.Name;

                NpgsqlParameter createParameter = updateByIdCommand.CreateParameter();
                NpgsqlParameter updateParameter = updateByIdCommand.CreateParameter();
                createParameter.ParameterName = updateParameter.ParameterName = columnName;

                // Get the data type from the Column attribute, or use the property type if it doesn't exist
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
                        // If the column attribute is set, but the type is not implemented, throw an exception
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

                    if (type == typeof(ulong))
                    {
                        dataType |= NpgsqlDbType.Numeric;

                        // Set the conversion logic on the properties getters/setters to prevent if statements when getting/setting the values
                        _propertyGetDelegateCache.Add(columnName, (obj) => Convert.ToInt64(property.GetValue(obj)));
                        _propertySetDelegateCache.Add(columnName, (obj, value) => property.SetValue(obj, Convert.ToUInt64(value)));
                    }
                    else
                    {
                        dataType |= type switch
                        {
                            Type when type == typeof(string) => NpgsqlDbType.Text,
                            Type when type == typeof(Guid) => NpgsqlDbType.Uuid,
                            Type when type == typeof(DateTimeOffset) => NpgsqlDbType.TimestampTz,
                            Type when type == typeof(DateTime) => NpgsqlDbType.TimestampTz,
                            _ => throw new NotImplementedException($"Type {property.PropertyType} does not have a database conversion implemented."),
                        };
                    }
                }

                // TryAdd is used here to prevent overwriting the conversion logic for the properties
                _propertyGetDelegateCache.TryAdd(columnName, property.GetValue);
                _propertySetDelegateCache.TryAdd(columnName, property.SetValue);

                createParameter.NpgsqlDbType = updateParameter.NpgsqlDbType = dataType;
                updateParameter.SourceColumn = createParameter.ParameterName;

                createCommand.Parameters.Add(createParameter);
                updateByIdCommand.Parameters.Add(updateParameter);

                // Continue building the SQL statements
                createCommandStringBuilder.Append($"@{createParameter.ParameterName}, ");
                updateCommandColumnStringBuilder.Append($"\"{updateParameter.SourceColumn}\", ");
                updateCommandValueStringBuilder.Append($"@{createParameter.ParameterName}, ");
            }

            // Remove the trailing ", " and add the closing parenthesis
            createCommandStringBuilder.Remove(createCommandStringBuilder.Length - 2, 2);
            createCommandStringBuilder.Append(");");
            createCommand.CommandText = createCommandStringBuilder.ToString();
            await createCommand.PrepareAsync();
            _preparedStatements.Add(PreparedStatementType.Create, createCommand);

            // Remove the trailing ", " and add the closing parenthesis
            updateCommandColumnStringBuilder.Remove(updateCommandColumnStringBuilder.Length - 2, 2);
            updateCommandValueStringBuilder.Remove(updateCommandValueStringBuilder.Length - 2, 2);
            updateCommandValueStringBuilder.Append(") WHERE \"Id\" = @Id;");
            updateByIdCommand.CommandText = updateCommandColumnStringBuilder.Append(updateCommandValueStringBuilder).ToString();
            await updateByIdCommand.PrepareAsync();
            _preparedStatements.Add(PreparedStatementType.UpdateById, updateByIdCommand);
        }

        /// <summary>
        /// Executes a prepared statement, optionally retrying if the connection is dead. This will also prepare the statements on the new connection.
        /// </summary>
        /// <param name="command">The command to execute.</param>
        /// <param name="statementType">The type of statement to execute.</param>
        /// <param name="isRetry">Whether or not this is a retry.</param>
        /// <returns>The <see cref="DbDataReader"/> if the statement is a select statement, otherwise null.</returns>
        private async Task<DbDataReader?> ExecutePreparedStatementAsync(DbCommand command, PreparedStatementType statementType, bool isRetry = false)
        {
            try
            {
                // Execute the statement
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
                // We already tried to reconnect, so we'll just throw the exception since it's probably not a connection issue
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

        /// <summary>
        /// Gets the cache expiration date, or null if the expiration date is more than 5 minutes in the future.
        /// </summary>
        /// <param name="expiresAt">The expiration date.</param>
        /// <returns>The cache expiration date, or null if the expiration date is more than 5 minutes in the future.</returns>
        private static DateTimeOffset? CacheExpiration(DateTimeOffset expiresAt) => expiresAt > DateTimeOffset.UtcNow.AddMinutes(5) ? null : expiresAt;

        /// <summary>
        /// The types of prepared statements.
        /// </summary>
        private enum PreparedStatementType
        {
            Create,
            SelectById,
            SelectByExpiresAt,
            UpdateById,
            DeleteById
        }
    }
}
