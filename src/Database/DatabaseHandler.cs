using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Npgsql;

namespace OoLunar.Tomoe.Database
{
    public sealed class DatabaseHandler
    {
        private delegate ValueTask PrepareAsyncDelegate(NpgsqlConnection connection);
        private readonly Dictionary<NpgsqlConnection, (SemaphoreSlim, PrepareAsyncDelegate)> _tableTypes = [];
        private readonly DatabaseConnectionManager _connectionManager;
        private readonly ILogger<DatabaseHandler> _logger;

        public DatabaseHandler(DatabaseConnectionManager connectionManager, ILogger<DatabaseHandler>? logger = null)
        {
            _connectionManager = connectionManager ?? throw new ArgumentNullException(nameof(connectionManager));
            _logger = logger ?? NullLogger<DatabaseHandler>.Instance;
        }

        public async ValueTask InitializeAsync(CancellationToken cancellationToken = default) =>
            await Parallel.ForEachAsync(typeof(Program).Assembly.GetTypes(), cancellationToken, async (Type type, CancellationToken cancellationToken) =>
            {
                if (type.GetCustomAttribute<DatabaseModelAttribute>() is null)
                {
                    return;
                }

                FieldInfo? semaphoreField = type.GetField("_semaphore", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                MethodInfo? prepareAsyncMethod = type.GetMethod("PrepareAsync", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                if (semaphoreField is null || prepareAsyncMethod is null)
                {
                    _logger.LogError("Type {Type} does not have a Semaphore or PrepareAsync method.", type.Name);
                    return;
                }

                if (semaphoreField?.GetValue(null) is not SemaphoreSlim semaphore
                    || prepareAsyncMethod?.CreateDelegate(typeof(PrepareAsyncDelegate)) is not PrepareAsyncDelegate prepareAsyncDelegate)
                {
                    _logger.LogError("Type {Type} does not have a Semaphore or PrepareAsync method.", type.Name);
                    return;
                }

                NpgsqlConnection connection = _connectionManager.CreateConnection();
                lock (_tableTypes)
                {
                    _tableTypes.Add(connection, (semaphore, prepareAsyncDelegate));
                }

                connection.StateChange += StateChangedEventHandlerAsync;
#if DEBUG
                // Sometimes I forget I need to connect to my VPN in order to connect to the database.
                // By logging this during dev, I'll notice it and remember to connect to my VPN before
                // waiting out the 30 second timeout.
                _logger.LogInformation("Connecting to table {TableName}...", type.Name);
#endif
                await connection.OpenAsync(cancellationToken);
            });

        private async void StateChangedEventHandlerAsync(object sender, StateChangeEventArgs eventArgs)
        {
            if (eventArgs.CurrentState is not ConnectionState.Open or ConnectionState.Broken or ConnectionState.Closed || sender is not NpgsqlConnection connection)
            {
                return;
            }

            if (!_tableTypes.TryGetValue(connection, out (SemaphoreSlim, PrepareAsyncDelegate) value))
            {
                _logger.LogError("Connection {Connection} no longer exists within the database handler however is still receiving events.", connection);
                return;
            }

            // Why can't I deconstruct this within the TryGetValue out parameter :(
            (SemaphoreSlim semaphore, PrepareAsyncDelegate prepareAsyncDelegate) = value;

            // Wait for all pending commands to finish executing/throwing.
            await semaphore.WaitAsync();

            // If the connection is broken, we need to close it before we can open it again.
            if (connection.State is ConnectionState.Broken)
            {
                _logger.LogInformation("Database state is broken, closing the connection and then reconnecting.");
                await connection.CloseAsync();
                await connection.OpenAsync();
            }
            else if (connection.State is ConnectionState.Closed)
            {
                _logger.LogInformation("Database state is {CurrentState}, waiting for database to be ready.", eventArgs.CurrentState);
                await connection.OpenAsync();
            }

            // If the connection is still not open, we can't do anything.
            if (connection.State is not ConnectionState.Open)
            {
                _logger.LogError("Database state is {CurrentState}, failed to open the database.", eventArgs.CurrentState);
                return;
            }

            // Prepare our SQL commands
            await prepareAsyncDelegate(connection);

            // Allow commands to be executed again.
            semaphore.Release();
            _logger.LogInformation("Table {Type} is ready!", prepareAsyncDelegate.Method.DeclaringType?.Name);
        }
    }
}
