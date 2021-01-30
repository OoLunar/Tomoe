using System;
using System.Collections.Generic;
using System.IO;

using Microsoft.Data.Sqlite;

using Tomoe.Database.Interfaces;
using Tomoe.Utils;

namespace Tomoe.Database.Drivers.Sqlite
{
	public class Sqlite : IDatabase
	{
		private static readonly Logger _logger = new("Database.SQLite");
		private readonly IUser SqliteUser;
		private readonly IGuild SqliteGuild;
		private readonly ITags SqliteTags;
		private readonly IAssignment SqliteAssignments;
		private readonly IStrikes SqliteStrikes;

		public IUser User { get => SqliteUser; }
		public IGuild Guild { get => SqliteGuild; }
		public ITags Tags { get => SqliteTags; }
		public IAssignment Assignments { get => SqliteAssignments; }
		public IStrikes Strikes { get => SqliteStrikes; }

		public Sqlite(string password, string databaseName, Dictionary<string, string> parameters)
		{
			SqliteOpenMode openMode = Enum.Parse<SqliteOpenMode>(parameters["open_mode"]);
			SqliteCacheMode cacheMode = Enum.Parse<SqliteCacheMode>(parameters["cache_mode"]);

			string databasePath = databaseName;
			if (!Path.IsPathRooted(databaseName)) databasePath = openMode == SqliteOpenMode.Memory ? ":memory:" : databaseName;
			_logger.Debug($"Setting database path to \"{databasePath}\"");

			// Back up the database if it exists.
			if (File.Exists(databaseName)) File.Copy(databaseName, databaseName + ".bak", true);

			SqliteStrikes = new SqliteStrikes(password, databasePath, openMode, cacheMode);
			SqliteAssignments = new SqliteAssignments(password, databasePath, openMode, cacheMode);
			SqliteGuild = new SqliteGuild(password, databasePath, openMode, cacheMode);
			SqliteUser = new SqliteUser(password, databasePath, openMode, cacheMode);
		}

		public void Dispose()
		{
			SqliteGuild.Dispose();
			SqliteAssignments.Dispose();
			SqliteStrikes.Dispose();
			SqliteUser.Dispose();
			GC.SuppressFinalize(this);
		}
	}
}
