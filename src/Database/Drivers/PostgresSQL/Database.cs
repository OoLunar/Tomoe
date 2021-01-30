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
		private IUser PostgresUser { get; set; }
		private IGuild PostgresGuild { get; set; }
		private ITags PostgresTags { get; set; }
		private IAssignment PostgresAssignments { get; set; }
		private IStrikes PostgresStrikes { get; set; }

		public IUser User { get => PostgresUser; }
		public IGuild Guild { get => PostgresGuild; }
		public ITags Tags { get => PostgresTags; }
		public IAssignment Assignments { get => PostgresAssignments; }
		public IStrikes Strikes { get => PostgresStrikes; }

		public PostgreSQL(string password, string databaseName, Dictionary<string, string> parameters)
		{
			NpgsqlLogManager.Provider = new NLogLoggingProvider();
			NpgsqlLogManager.IsParameterLoggingEnabled = true;
			int port = int.Parse(parameters["port"]);
			SslMode sslMode = Enum.Parse<SslMode>(parameters["ssl_mode"]);
			PostgresUser = new PostgresUser(parameters["host"], port, parameters["username"], password, databaseName, sslMode);
			PostgresGuild = new PostgresGuild(parameters["host"], port, parameters["username"], password, databaseName, sslMode);
			PostgresTags = new PostgresTags(parameters["host"], port, parameters["username"], password, databaseName, sslMode);
			PostgresAssignments = new PostgresAssignments(parameters["host"], port, parameters["username"], password, databaseName, sslMode);
			PostgresStrikes = new PostgresStrikes(parameters["host"], port, parameters["username"], password, databaseName, sslMode);
		}

		public void Dispose()
		{
			PostgresAssignments.Dispose();
			PostgresGuild.Dispose();
			PostgresTags.Dispose();
			PostgresAssignments.Dispose();
			PostgresStrikes.Dispose();
			GC.SuppressFinalize(this);
		}
	}
}
