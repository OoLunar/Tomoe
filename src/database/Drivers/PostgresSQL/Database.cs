using Npgsql;
using Npgsql.Logging;
using Tomoe.Database.Interfaces;
using Tomoe.Utils;

namespace Tomoe.Database.Drivers.PostgresSQL
{
	public class PostgresSQL : IDatabase
	{
		internal IUser _postgresUser { get; private set; }
		internal IGuild _postgresGuild { get; private set; }
		internal ITags _postgresTags { get; private set; }
		internal IAssignment _postgresTasks { get; private set; }
		internal IStrikes _postgresStrikes { get; private set; }

		public IUser User => _postgresUser;
		public IGuild Guild => _postgresGuild;
		public ITags Tags => _postgresTags;
		public IAssignment Tasks => _postgresTasks;
		public IStrikes Strikes => _postgresStrikes;

		public PostgresSQL(string host, int port, string username, string password, string database_name, SslMode sslMode)
		{
			NpgsqlLogManager.Provider = new NLogLoggingProvider();
			NpgsqlLogManager.IsParameterLoggingEnabled = true;
			_postgresUser = new PostgresUser(host, port, username, password, database_name, sslMode);
			_postgresGuild = new PostgresGuild(host, port, username, password, database_name, sslMode);
			_postgresTags = new PostgresTags(host, port, username, password, database_name, sslMode);
			_postgresTasks = new PostgresAssignment(host, port, username, password, database_name, sslMode);
			_postgresStrikes = new PostgresStrikes(host, port, username, password, database_name, sslMode);
		}
	}
}
