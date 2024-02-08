using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace OoLunar.Tomoe.Database
{
    public sealed class DatabaseHandler
    {
        private delegate ValueTask PrepareAsyncDelegate(NpgsqlConnection connection);

        private static readonly FrozenSet<SemaphoreSlim> Semaphores;
        private static readonly FrozenSet<PrepareAsyncDelegate> PrepareAsyncDelegates;

        static DatabaseHandler()
        {
            List<SemaphoreSlim> semaphores = [];
            List<PrepareAsyncDelegate> prepareAsyncDelegates = [];
            foreach (Type type in typeof(Program).Assembly.GetTypes())
            {
                if (type.GetCustomAttribute<DatabaseModelAttribute>() is null)
                {
                    continue;
                }

                FieldInfo? semaphoreField = type.GetField("Semaphore", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                semaphores.Add((SemaphoreSlim)semaphoreField!.GetValue(null)!);

                MethodInfo? prepareAsyncMethod = type.GetMethod("PrepareAsync", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
                prepareAsyncDelegates.Add((PrepareAsyncDelegate)prepareAsyncMethod!.CreateDelegate(typeof(PrepareAsyncDelegate))!);
            }

            Semaphores = semaphores.ToFrozenSet();
            PrepareAsyncDelegates = prepareAsyncDelegates.ToFrozenSet();
        }

        public static NpgsqlConnection Initialize(IServiceProvider serviceProvider)
        {
            Assembly assembly = typeof(Program).Assembly;
            ILogger<DatabaseHandler> logger = serviceProvider.GetRequiredService<ILogger<DatabaseHandler>>();
            IConfiguration configuration = serviceProvider.GetRequiredService<IConfiguration>();
            NpgsqlConnectionStringBuilder connectionStringBuilder = new()
            {
                Host = configuration.GetValue("database:host", "localhost"),
                Port = configuration.GetValue("database:port", 5432),
                Username = configuration.GetValue("database:username", "postgres"),
                Password = configuration.GetValue("database:password", "postgres"),
                Database = configuration.GetValue("database:database", "tomoe"),
                CommandTimeout = configuration.GetValue("database:timeout", 5),
#if DEBUG
                IncludeErrorDetail = true
#endif
            };

            NpgsqlConnection connection = new(connectionStringBuilder.ToString());
            connection.StateChange += async (_, eventArgs) =>
            {
                if (eventArgs.CurrentState is not ConnectionState.Open or ConnectionState.Broken or ConnectionState.Closed)
                {
                    return;
                }

                logger.LogInformation("Database state changed from {OriginalState} to {CurrentState}", eventArgs.OriginalState, eventArgs.CurrentState);
                foreach (SemaphoreSlim semaphore in Semaphores)
                {
                    // Wait for all pending commands to finish executing/throwing.
                    await semaphore.WaitAsync();
                }

                if (connection.State is ConnectionState.Broken)
                {
                    // If the connection is broken, we need to close it before we can open it again.
                    await connection.CloseAsync();
                }

                while (eventArgs.CurrentState is not ConnectionState.Open)
                {
                    logger.LogInformation("Database state is {CurrentState}, waiting for database to be ready", eventArgs.CurrentState);
                    await connection.OpenAsync();
                }

                logger.LogInformation("Database state is {CurrentState}, preparing database", eventArgs.CurrentState);
                foreach (PrepareAsyncDelegate prepareAsyncDelegate in PrepareAsyncDelegates)
                {
                    // Prepare our SQL commands
                    await prepareAsyncDelegate(connection);
                }

                foreach (SemaphoreSlim semaphore in Semaphores)
                {
                    // Allow commands to be executed again.
                    semaphore.Release();
                }

                logger.LogInformation("Database is ready");
            };

            logger.LogInformation("Connecting to database at {Host}:{Port} as {Username} with database {Database}", connectionStringBuilder.Host, connectionStringBuilder.Port, connectionStringBuilder.Username, connectionStringBuilder.Database);
            connection.Open();
            return connection;
        }
    }
}
