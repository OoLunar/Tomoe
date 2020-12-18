using Npgsql;
using Npgsql.Logging;
using Tomoe.Database.Interfaces;
using Tomoe.Utils;

namespace Tomoe.Database.Drivers.PostgresSQL {
    public class PostgresSQL : IDatabase {
        private IUser _postgresUser;
        private IGuild _postgresGuild;
        private ITags _postgresTags;
        private ITasks _postgresTasks;

        public IUser User => _postgresUser;
        public IGuild Guild => _postgresGuild;
        public ITags Tags => _postgresTags;
        public ITasks Tasks => _postgresTasks;

        public PostgresSQL(string host, int port, string username, string password, string database_name, SslMode sslMode) {
            NpgsqlLogManager.Provider = new NLogLoggingProvider();
            _postgresUser = new PostgresUser(host, port, username, password, database_name, sslMode);
            _postgresGuild = new PostgresGuild(host, port, username, password, database_name, sslMode);
            _postgresTags = new PostgresTags(host, port, username, password, database_name, sslMode);
            _postgresTasks = new PostgresTasks(host, port, username, password, database_name, sslMode);
        }
    }
}