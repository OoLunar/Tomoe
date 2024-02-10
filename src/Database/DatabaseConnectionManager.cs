using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace OoLunar.Tomoe.Database
{
    public sealed class DatabaseConnectionManager : IAsyncDisposable, IDisposable
    {
        private readonly List<NpgsqlConnection> _connections = [];
        private readonly string _connectionString;

        public DatabaseConnectionManager(IConfiguration configuration)
        {
            ArgumentNullException.ThrowIfNull(configuration, nameof(configuration));
            _connectionString = new NpgsqlConnectionStringBuilder()
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
            }.ConnectionString;
        }

        public NpgsqlConnection GetConnection()
        {
            NpgsqlConnection connection = new(_connectionString);
            _connections.Add(connection);
            return connection;
        }

        public void Dispose()
        {
            foreach (NpgsqlConnection connection in _connections)
            {
                connection.Dispose();
            }
        }

        public async ValueTask DisposeAsync()
        {
            foreach (NpgsqlConnection connection in _connections)
            {
                await connection.DisposeAsync();
            }
        }
    }
}
