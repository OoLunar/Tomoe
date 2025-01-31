using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Npgsql;
using OoLunar.Tomoe.Configuration;

namespace OoLunar.Tomoe.Database
{
    public sealed class DatabaseConnectionManager : IAsyncDisposable, IDisposable
    {
        private readonly List<NpgsqlConnection> _connections = [];
        private readonly string _connectionString;

        public DatabaseConnectionManager(TomoeConfiguration tomoeConfiguration)
        {
            ArgumentNullException.ThrowIfNull(tomoeConfiguration, nameof(tomoeConfiguration));
            _connectionString = new NpgsqlConnectionStringBuilder()
            {
                Host = tomoeConfiguration.Database.Host,
                Port = tomoeConfiguration.Database.Port,
                Username = tomoeConfiguration.Database.Username,
                Password = tomoeConfiguration.Database.Password,
                Database = tomoeConfiguration.Database.Database,
                CommandTimeout = tomoeConfiguration.Database.CommandTimeout,
                IncludeErrorDetail = tomoeConfiguration.Database.IncludeErrorDetail
            }.ConnectionString;
        }

        public NpgsqlConnection CreateConnection()
        {
            NpgsqlConnection connection = new(_connectionString);
            _connections.Add(connection);
            return connection;
        }

        public void RemoveConnection(NpgsqlConnection connection) => _connections.Remove(connection);

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
