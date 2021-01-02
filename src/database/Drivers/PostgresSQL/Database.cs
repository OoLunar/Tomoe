using Npgsql;
using Npgsql.Logging;
using Tomoe.Database.Interfaces;
using Tomoe.Utils;

namespace Tomoe.Database.Drivers.PostgresSQL
{
	public class PostgresSQL : IDatabase
	{
		internal IUser PostgresUser { get; private set; }
		internal IGuild PostgresGuild { get; private set; }
		internal ITags PostgresTags { get; private set; }
		internal IAssignment PostgresAssignments { get; private set; }
		internal IStrikes PostgresStrikes { get; private set; }

		public IUser User => PostgresUser;
		public IGuild Guild => PostgresGuild;
		public ITags Tags => PostgresTags;
		public IAssignment Assignments => PostgresAssignments;
		public IStrikes Strikes => PostgresStrikes;

		public PostgresSQL(string host, int port, string username, string password, string database_name, SslMode sslMode)
		{
			NpgsqlLogManager.Provider = new NLogLoggingProvider();
			NpgsqlLogManager.IsParameterLoggingEnabled = true;
			PostgresUser = new PostgresUser(host, port, username, password, database_name, sslMode);
			PostgresGuild = new PostgresGuild(host, port, username, password, database_name, sslMode);
			PostgresTags = new PostgresTags(host, port, username, password, database_name, sslMode);
			PostgresAssignments = new PostgresAssignments(host, port, username, password, database_name, sslMode);
			PostgresStrikes = new PostgresStrikes(host, port, username, password, database_name, sslMode);
		}
	}
}
