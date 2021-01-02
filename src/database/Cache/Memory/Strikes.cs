using System.Collections.Generic;
using System.Data;
using Microsoft.Data.Sqlite;
using Tomoe.Database.Interfaces;

namespace Tomoe.Database.Cache.Memory
{
	public class Strikes
	{
		public enum StatementType
		{
			GetStrike,
			GetVictim,
			GetIssued,
			Add,
			Drop,
			Edit
		}

		private static readonly SqliteConnection connection = new(":memory:"); // Creates an in-memory database.
		private static readonly Dictionary<StatementType, SqliteCommand> preparedStatements = new();

		public Strikes()
		{
			connection.Open();
			SqliteCommand retrieve = new("", connection);
		}

	}
}
