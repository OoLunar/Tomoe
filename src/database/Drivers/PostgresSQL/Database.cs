using System.Dynamic;
using Npgsql;
using Npgsql.Logging;
using Tomoe.Database.Interfaces;
using Tomoe.Utils;

namespace Tomoe.Database.Drivers.PostgresSQL
{
	public class PostgresSQL : IDatabase
	{
		private readonly IUser PostgresUser;
		private readonly IGuild PostgresGuild;
		private readonly ITags PostgresTags;
		private readonly IAssignment PostgresAssignments;
		private readonly IStrikes PostgresStrikes;

		public IUser User { get => PostgresUser; }
		public IGuild Guild { get => PostgresGuild; }
		public ITags Tags { get => PostgresTags; }
		public IAssignment Assignments { get => PostgresAssignments; }
		public IStrikes Strikes { get => PostgresStrikes; }

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
