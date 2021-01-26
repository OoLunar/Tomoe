using System;
using System.Collections.Generic;

using Npgsql;
using Npgsql.Logging;

using Tomoe.Database.Interfaces;
using Tomoe.Utils;

namespace Tomoe.Database.Drivers.PostgreSQL
{
	public class PostgreSQL : IDatabase
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

		public PostgreSQL(string password, string database_name, Dictionary<string, string> parameters)
		{
			NpgsqlLogManager.Provider = new NLogLoggingProvider();
			NpgsqlLogManager.IsParameterLoggingEnabled = true;
			string host = parameters["host"];
			int port = int.Parse(parameters["port"]);
			string username = parameters["username"];
			SslMode sslMode = Enum.Parse<SslMode>(parameters["ssl_mode"]);
			PostgresUser = new PostgresUser(host, port, username, password, database_name, sslMode);
			PostgresGuild = new PostgresGuild(host, port, username, password, database_name, sslMode);
			PostgresTags = new PostgresTags(host, port, username, password, database_name, sslMode);
			PostgresAssignments = new PostgresAssignments(host, port, username, password, database_name, sslMode);
			PostgresStrikes = new PostgresStrikes(host, port, username, password, database_name, sslMode);
		}
	}
}
