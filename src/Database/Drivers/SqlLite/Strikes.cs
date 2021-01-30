using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Microsoft.Data.Sqlite;

using Tomoe.Database.Interfaces;
using Tomoe.Utils;

namespace Tomoe.Database.Drivers.Sqlite
{
	public class SqliteStrikes : IStrikes
	{
		private readonly SqliteConnection _connection;
		private readonly Logger _logger = new("Database.SQLite.Strikes");
		private readonly Dictionary<StatementType, SqliteCommand> _preparedStatements = new();

		private enum StatementType
		{
			Add,
			Drop,
			Edit,
			GetIssued,
			GetStrike,
			GetVictim
		}

		private Dictionary<int, List<dynamic>> ExecuteQuery(StatementType command, List<SqliteParameter> parameters, bool needsResult = false)
		{
			SqliteCommand statement = _preparedStatements[command];
			if (statement.Parameters.Count != parameters.Count) throw new SqliteException("Prepared parameters count do not line up with given parameters count.", 1);
			Dictionary<string, SqliteParameter> sortedParameters = new();
			foreach (SqliteParameter parameter in parameters) sortedParameters.Add(parameter.ParameterName, parameter);
			foreach (SqliteParameter parameter in statement.Parameters) parameter.Value = sortedParameters[parameter.ParameterName].Value;
			string parameterValues = string.Join(", ", sortedParameters.Select(param => param.Value.Value));
			_logger.Trace($"Executing prepared statement \"{command}\" with parameters: {(parameterValues == string.Empty ? "None" : parameterValues)}");
			try
			{
				if (needsResult)
				{
					SqliteDataReader reader = statement.ExecuteReader();
					Dictionary<int, List<dynamic>> values = new();
					int indexCount = 0;
					while (reader.Read())
					{
						List<dynamic> list = new();
						for (int i = 0; i < reader.FieldCount; i++)
						{
							if (reader[i] == DBNull.Value) list.Add(null);
							else list.Add(reader[i]);
							_logger.Trace($"Recieved values: {reader[i] ?? "null"} on iteration {i}");
						}

						if (list.Count == 1 && list[0] == null) values.Add(indexCount, null);
						else values.Add(indexCount, list);
						indexCount++;
					}
					reader.DisposeAsync().GetAwaiter().GetResult();
					if (values.Count == 0 || (values.Count == 1 && values[0] == null)) values = null;
					return values;
				}
				else
				{
					_ = statement.ExecuteNonQuery();
					return null;
				}
			}
			catch (SqliteException error) when (error.ErrorCode == 11)
			{
				_logger.Critical("Database image is malformed!");
				return null;
			}
		}

		private Dictionary<int, List<dynamic>> ExecuteQuery(StatementType command, SqliteParameter parameter, bool needsResult = false) => ExecuteQuery(command, new List<SqliteParameter>() { parameter }, needsResult);

		public SqliteStrikes(string password, string databaseName, SqliteOpenMode openMode, SqliteCacheMode cacheMode)
		{
			SqliteConnectionStringBuilder connectionString = new();
			connectionString.Mode = openMode;
			connectionString.Cache = cacheMode;
			connectionString.DataSource = databaseName;
			connectionString.Password = password;
			_connection = new(connectionString.ToString());
			_logger.Info("Opening connection to database...");
			_connection.Open();
			_logger.Debug("Creating strikes table if it doesn't exist...");
			SqliteCommand createStrikeTable = new(File.ReadAllText(Path.Join(FileSystem.ProjectRoot, "res/sql/drivers/sqlite/strike_table.sql")), _connection);
			_ = createStrikeTable.ExecuteNonQuery();
			createStrikeTable.Dispose();

			_logger.Info("Preparing SQL commands...");
			_logger.Debug($"Preparing {StatementType.Add}...");
			SqliteCommand add = new("INSERT INTO strikes (guild_Id, victim_Id, issuer_Id, reason, jumplink, victim_messaged, strike_count) SELECT @guildId, @victimId, @issuerId, @reason, @jumplink, @victimMessaged, MAX(strike_count) + 1 FROM strikes WHERE victim_id = @victimId AND guild_Id = @guildId", _connection);
			_ = add.Parameters.Add(new("@guildId", SqliteType.Integer, sizeof(ulong), "guild_id"));
			_ = add.Parameters.Add(new("@victimId", SqliteType.Integer, sizeof(ulong), "victim_id"));
			_ = add.Parameters.Add(new("@issuerId", SqliteType.Integer, sizeof(ulong), "issuer_id"));
			_ = add.Parameters.Add(new("@reason", SqliteType.Text));
			_ = add.Parameters.Add(new("@jumpLink", SqliteType.Text));
			_ = add.Parameters.Add(new("@victimMessaged", SqliteType.Integer, sizeof(bool), "victim_messaged"));
			add.Prepare();
			_preparedStatements.Add(StatementType.Add, add);

			_logger.Debug($"Preparing {StatementType.Drop}...");
			SqliteCommand drop = new("UPDATE strikes SET dropped=true, reason=reason || ',' || @reason WHERE id=@strikeId", _connection);
			_ = drop.Parameters.Add(new("@reason", SqliteType.Text));
			_ = drop.Parameters.Add(new("@strikeId", SqliteType.Integer, sizeof(short), "strike_id"));
			drop.Prepare();
			_preparedStatements.Add(StatementType.Drop, drop);

			_logger.Debug($"Preparing {StatementType.Edit}...");
			SqliteCommand edit = new("UPDATE strikes SET reason=reason || ',' || @reason WHERE id=@strikeId", _connection);
			_ = edit.Parameters.Add(new("@reason", SqliteType.Text));
			_ = edit.Parameters.Add(new("@strikeId", SqliteType.Integer, sizeof(short), "strike_id"));
			edit.Prepare();
			_preparedStatements.Add(StatementType.Edit, edit);

			_logger.Debug($"Preparing {StatementType.GetIssued}...");
			SqliteCommand getIssued = new("SELECT guild_id, victim_id, issuer_id, reason, jumplink, victim_messaged, dropped, created_at, id, strike_count FROM strikes WHERE guild_id=@guildId AND issuer_id=@issuerId", _connection);
			_ = getIssued.Parameters.Add(new("@guildId", SqliteType.Integer, sizeof(ulong), "guild_id"));
			_ = getIssued.Parameters.Add(new("@victimId", SqliteType.Integer, sizeof(ulong), "victim_id"));
			getIssued.Prepare();
			_preparedStatements.Add(StatementType.GetIssued, getIssued);

			_logger.Debug($"Preparing {StatementType.GetStrike}...");
			SqliteCommand getStrike = new("SELECT * FROM strikes WHERE id=@strikeId", _connection);
			SqliteParameter getStrikeId = new("@strikeId", SqliteType.Integer, sizeof(short), "strike_id");
			_ = getStrike.Parameters.Add(getStrikeId);
			getStrike.Prepare();
			_preparedStatements.Add(StatementType.GetStrike, getStrike);

			_logger.Debug($"Preparing {StatementType.GetVictim}...");
			SqliteCommand getVictim = new("SELECT * FROM strikes WHERE guild_id=@guildId AND victim_id=@victimId", _connection);
			_ = getVictim.Parameters.Add(new("@victimId", SqliteType.Integer, sizeof(ulong), "victim_id"));
			getVictim.Prepare();
			_preparedStatements.Add(StatementType.GetVictim, getVictim);
		}

		public void Dispose()
		{
			_preparedStatements.Clear();
			_connection.Dispose();
			GC.SuppressFinalize(this);
		}

		public Strike? Add(ulong guildId, ulong victimId, ulong issuerId, string reason, string jumpLink, bool victimMessaged)
		{
			List<dynamic> queryResults = ExecuteQuery(StatementType.Add, new List<SqliteParameter>() { new SqliteParameter("guildId", guildId), new SqliteParameter("victimId", victimId), new SqliteParameter("issuerId", issuerId), new SqliteParameter("reason", reason), new SqliteParameter("jumpLink", jumpLink), new SqliteParameter("victimMessaged", victimMessaged) }, true)?[0];
			if (queryResults == null) return null;
			Strike strike = new();
			strike.GuildId = guildId;
			strike.VictimId = victimId;
			strike.IssuerId = issuerId;
			strike.Reason = new string[] { reason };
			strike.JumpLink = jumpLink;
			strike.VictimMessaged = victimMessaged;
			strike.Dropped = queryResults[0];
			strike.CreatedAt = queryResults[1];
			strike.Id = queryResults[2];
			strike.StrikeCount = queryResults[3];
			return strike;
		}

		public Strike? Drop(int strikeId, string reason)
		{
			List<dynamic> queryResults = ExecuteQuery(StatementType.Drop, new List<SqliteParameter>() { new SqliteParameter("strikeId", strikeId), new SqliteParameter("reason", reason) })?[0];
			if (queryResults == null) return null;
			Strike strike = new();
			strike.GuildId = queryResults[0];
			strike.VictimId = queryResults[1];
			strike.IssuerId = queryResults[2];
			strike.Reason = queryResults[3];
			strike.JumpLink = queryResults[4];
			strike.VictimMessaged = queryResults[5];
			strike.Dropped = true;
			strike.CreatedAt = queryResults[6];
			strike.Id = strikeId;
			strike.StrikeCount = queryResults[7];
			return strike;
		}
		public void Edit(int strikeId, string reason) => ExecuteQuery(StatementType.Edit, new List<SqliteParameter>() { new SqliteParameter("strikeId", strikeId), new SqliteParameter("reason", reason) });
		public Strike? Retrieve(int strikeId)
		{
			List<dynamic> queryResults = ExecuteQuery(StatementType.Edit, new SqliteParameter("strikeId", strikeId), true)?[0];
			if (queryResults == null) return null;
			Strike strike = new();
			strike.GuildId = queryResults[0];
			strike.VictimId = queryResults[1];
			strike.IssuerId = queryResults[2];
			strike.Reason = queryResults[3];
			strike.JumpLink = queryResults[4];
			strike.VictimMessaged = queryResults[5];
			strike.Dropped = queryResults[6];
			strike.CreatedAt = (DateTime)queryResults[7];
			strike.Id = queryResults[8];
			strike.StrikeCount = queryResults[9];
			return strike;
		}

		public Strike[] GetVictim(ulong guildId, ulong victimId)
		{
			Dictionary<int, List<dynamic>> queryResults = ExecuteQuery(StatementType.GetVictim, new List<SqliteParameter>() { new SqliteParameter("guildId", guildId), new SqliteParameter("victimId", victimId) }, true);
			if (queryResults == null) return null;
			List<Strike> strikes = new();
			foreach (List<dynamic> query in queryResults.Values)
			{
				Strike strike = new();
				strike.GuildId = query[0];
				strike.VictimId = query[1];
				strike.IssuerId = query[2];
				strike.Reason = query[3];
				strike.JumpLink = query[4];
				strike.VictimMessaged = query[5];
				strike.Dropped = query[6];
				strike.CreatedAt = query[7];
				strike.Id = query[8];
				strike.StrikeCount = query[9];
				strikes.Add(strike);
			}
			return strikes.ToArray();
		}

		public Strike[] GetIssued(ulong guildId, ulong issuerId)
		{
			Dictionary<int, List<dynamic>> queryResults = ExecuteQuery(StatementType.GetIssued, new List<SqliteParameter>() { new SqliteParameter("guildId", guildId), new SqliteParameter("issuerId", issuerId) });
			if (queryResults == null) return null;
			List<Strike> strikes = new();
			foreach (List<dynamic> query in queryResults.Values)
			{
				Strike strike = new();
				strike.GuildId = query[0];
				strike.VictimId = query[1];
				strike.IssuerId = query[2];
				strike.Reason = query[3];
				strike.JumpLink = query[4];
				strike.VictimMessaged = query[5];
				strike.Dropped = query[6];
				strike.CreatedAt = query[7];
				strike.Id = query[8];
				strikes.Add(strike);
			}
			return strikes.ToArray();
		}
	}
}
