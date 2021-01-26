using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;

using Npgsql;

using NpgsqlTypes;

using Tomoe.Database.Interfaces;
using Tomoe.Utils;

namespace Tomoe.Database.Drivers.PostgresSQL
{
	public class PostgresStrikes : IStrikes
	{
		private static readonly Logger _logger = new("Database.PostgresSQL.Strike");
		private readonly NpgsqlConnection _connection;
		private readonly Dictionary<StatementType, NpgsqlCommand> _preparedStatements = new();
		private int retryCount;
		private enum StatementType
		{
			GetStrike,
			GetVictim,
			GetIssued,
			Add,
			Drop,
			Edit
		}

		/// <summary>
		/// Executes an SQL query from <see cref="_preparedStatements">_preparedStatements</see>, using <seealso cref="StatementType">statementType</seealso> as a key.
		///
		/// Returns a list of results if <paramref name="needsResult">needsResult</paramref> is true, otherwise returns null.
		/// </summary>
		/// <param name="command">Which SQL command to execute, using <see cref="StatementType">statementType</see> as an index.</param>
		/// <param name="parameters">A list of <see cref="NpgsqlParameter">NpgsqlParameter's</see>.</param>
		/// <param name="needsResult">Returns a list of results if true, otherwise returns null.</param>
		/// <returns><see cref="List{T}">List&lt;dynamic&gt;</see> if <paramref name="needsResult">needsResult</paramref> is true, otherwise returns null.</returns>
		private Dictionary<int, List<dynamic>> ExecuteQuery(StatementType command, List<NpgsqlParameter> parameters, bool needsResult = false)
		{
			NpgsqlCommand statement = _preparedStatements[command];
			if (statement.Parameters.Count != parameters.Count) throw new NpgsqlException("Prepared parameters count do not line up with given parameters count.");
			Dictionary<string, NpgsqlParameter> sortedParameters = new();
			foreach (NpgsqlParameter parameter in parameters) sortedParameters.Add(parameter.ParameterName, parameter);
			foreach (NpgsqlParameter parameter in statement.Parameters) parameter.Value = sortedParameters[parameter.ParameterName].Value;
			_logger.Trace($"Executing prepared statement \"{command}\" with parameters: {string.Join(", ", statement.Parameters.Select(param => param.Value).ToArray())}");

			try
			{
				if (needsResult)
				{
					NpgsqlDataReader reader = statement.ExecuteReader();
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
					retryCount = 0;
					if (values.Count == 0 || (values.Count == 1 && values[0] == null)) values = null;
					return values;
				}
				else
				{
					_ = statement.ExecuteNonQuery();
					retryCount = 0;
					return null;
				}
			}
			catch (SocketException error)
			{
				if (retryCount > DatabaseLoader.RetryCount) _logger.Critical($"Failed to execute query \"{command}\" after {retryCount} times. Check your internet connection.");
				else retryCount++;
				_logger.Error($"Socket exception occured, retrying... Details: {error.Message}\n{error.StackTrace}");
				return ExecuteQuery(command, parameters, needsResult);
			}
		}

		/// <inheritdoc cref="ExecuteQuery(StatementType, List{NpgsqlParameter}, bool)" />
		/// <param name="parameter">One <see cref="NpgsqlParameter">NpgsqlParameter</see>, which gets converted into a <see cref="List{T}">List&lt;NpgsqlParameter&gt;</see>.</param>
		private Dictionary<int, List<dynamic>> ExecuteQuery(StatementType command, NpgsqlParameter parameter, bool needsResult = false) => ExecuteQuery(command, new List<NpgsqlParameter> { parameter }, needsResult);

		public PostgresStrikes(string host, int port, string username, string password, string databaseName, SslMode sslMode)
		{
			_connection = new NpgsqlConnection($"Host={host};Port={port};Username={username};Password={password};Database={databaseName};SSL Mode={sslMode}");
			_logger.Info("Opening connection to database...");
			try
			{
				_connection.Open();
				_logger.Debug("Creating strikes table if it doesn't exist...");
				NpgsqlCommand createTagsTable = new(File.ReadAllText(Path.Join(FileSystem.ProjectRoot, "res/sql/drivers/postgresql/strike_table.sql")), _connection);
				_ = createTagsTable.ExecuteNonQuery();
				createTagsTable.Dispose();
			}
			catch (SocketException error)
			{
				_logger.Critical($"Failed to connect to database. {error.Message}", true);
			}
			catch (PostgresException error) when (error.SqlState == "28P01")
			{
				_logger.Critical($"Failed to connect to database. Check your password.");
			}
			_logger.Info("Preparing SQL commands...");
			_logger.Debug($"Preparing {StatementType.Add}...");
			NpgsqlCommand add = new("INSERT INTO strikes VALUES(@guildId, @victimId, @issuerId, ARRAY[@reason], @jumpLink, @victimMessaged, DEFAULT, DEFAULT, DEFAULT, calc_strike_count(@guildId, @victimId)) RETURNING dropped, created_at, id, strike_count", _connection);
			_ = add.Parameters.Add(new("guildId", NpgsqlDbType.Bigint));
			_ = add.Parameters.Add(new("victimId", NpgsqlDbType.Bigint));
			_ = add.Parameters.Add(new("issuerId", NpgsqlDbType.Bigint));
			_ = add.Parameters.Add(new("reason", NpgsqlDbType.Varchar));
			_ = add.Parameters.Add(new("jumpLink", NpgsqlDbType.Varchar));
			_ = add.Parameters.Add(new("victimMessaged", NpgsqlDbType.Boolean));
			add.Prepare();
			_preparedStatements.Add(StatementType.Add, add);

			_logger.Debug($"Preparing {StatementType.Drop}...");
			NpgsqlCommand drop = new("UPDATE strikes SET dropped=true, reason=array_append(reason, @reason) WHERE id=@strikeId RETURNING guild_id, victim_id, issuer_id, reason, victim_messaged, created_at, strike_count", _connection);
			_ = drop.Parameters.Add(new("strikeId", NpgsqlDbType.Bigint));
			_ = drop.Parameters.Add(new("reason", NpgsqlDbType.Varchar));
			drop.Prepare();
			_preparedStatements.Add(StatementType.Drop, drop);

			_logger.Debug($"Preparing {StatementType.Edit}...");
			NpgsqlCommand edit = new("UPDATE strikes SET reason=array_append(reason, @reason) WHERE id=@strikeId", _connection);
			_ = edit.Parameters.Add(new("reason", NpgsqlDbType.Varchar));
			_ = edit.Parameters.Add(new("strikeId", NpgsqlDbType.Integer));
			edit.Prepare();
			_preparedStatements.Add(StatementType.Edit, edit);

			_logger.Debug($"Preparing {StatementType.GetIssued}...");
			NpgsqlCommand getIssued = new("SELECT guild_id, victim_id, issuer_id, reason, jumplink, victim_messaged, dropped, created_at, id, strike_count FROM strikes WHERE guild_id=@guildId AND issuer_id=@issuerId", _connection);
			_ = getIssued.Parameters.Add(new("guildId", NpgsqlDbType.Bigint));
			_ = getIssued.Parameters.Add(new("issuerId", NpgsqlDbType.Bigint));
			getIssued.Prepare();
			_preparedStatements.Add(StatementType.GetIssued, getIssued);

			_logger.Debug($"Preparing {StatementType.GetStrike}...");
			NpgsqlCommand getStrike = new("SELECT * FROM strikes WHERE id=@strikeId", _connection);
			_ = getStrike.Parameters.Add(new("strikeId", NpgsqlDbType.Integer));
			getStrike.Prepare();
			_preparedStatements.Add(StatementType.GetStrike, getStrike);

			_logger.Debug($"Preparing {StatementType.GetVictim}...");
			NpgsqlCommand getStrikes = new("SELECT * FROM strikes WHERE guild_id=@guildId AND victim_id=@victimId", _connection);
			_ = getStrikes.Parameters.Add(new("guildId", NpgsqlDbType.Bigint));
			_ = getStrikes.Parameters.Add(new("victimId", NpgsqlDbType.Bigint));
			getStrikes.Prepare();
			_preparedStatements.Add(StatementType.GetVictim, getStrikes);
		}

		public void Dispose()
		{
			_preparedStatements.Clear();
			_connection.Close();
			_connection.Dispose();
			GC.SuppressFinalize(this);
		}

		public Strike? Add(ulong guildId, ulong victimId, ulong issuerId, string reason, string jumpLink, bool victimMessaged)
		{
			List<dynamic> queryResults = ExecuteQuery(StatementType.Add, new List<NpgsqlParameter>() { new NpgsqlParameter("guildId", (long)guildId), new NpgsqlParameter("victimId", (long)victimId), new NpgsqlParameter("issuerId", (long)issuerId), new NpgsqlParameter("reason", reason), new NpgsqlParameter("jumpLink", jumpLink), new NpgsqlParameter("victimMessaged", victimMessaged) }, true)?[0];
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
			List<dynamic> queryResults = ExecuteQuery(StatementType.Drop, new List<NpgsqlParameter>() { new NpgsqlParameter("strikeId", strikeId), new NpgsqlParameter("reason", reason) })?[0];
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
		public void Edit(int strikeId, string reason) => ExecuteQuery(StatementType.Edit, new List<NpgsqlParameter>() { new NpgsqlParameter("strikeId", strikeId), new NpgsqlParameter("reason", reason) });
		public Strike? Retrieve(int strikeId)
		{
			List<dynamic> queryResults = ExecuteQuery(StatementType.Edit, new NpgsqlParameter("strikeId", strikeId), true)?[0];
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
			Dictionary<int, List<dynamic>> queryResults = ExecuteQuery(StatementType.GetVictim, new List<NpgsqlParameter>() { new NpgsqlParameter("guildId", (long)guildId), new NpgsqlParameter("victimId", (long)victimId) }, true);
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
			Dictionary<int, List<dynamic>> queryResults = ExecuteQuery(StatementType.GetIssued, new List<NpgsqlParameter>() { new NpgsqlParameter("guildId", (long)guildId), new NpgsqlParameter("issuerId", (long)issuerId) });
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
