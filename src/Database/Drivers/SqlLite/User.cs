using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Microsoft.Data.Sqlite;

using Tomoe.Database.Interfaces;
using Tomoe.Utils;

namespace Tomoe.Database.Drivers.Sqlite
{
	public class SqliteUser : IUser
	{
		private static readonly Logger _logger = new("Database.Sqlite.User");
		private readonly SqliteConnection _connection;
		private readonly Dictionary<StatementType, SqliteCommand> _preparedStatements = new();
		private enum StatementType
		{
			Insert,
			Exists,
			GetRoles,
			SetRoles,
			AddStrike,
			RemoveStrike,
			GetStrikeCount,
			SetStrikeCount,
			GetIsMuted,
			SetIsMuted,
			GetIsAntiMemed,
			SetIsAntiMemed,
			GetIsNoVC,
			SetIsNoVC
		}

		/// <summary>
		/// Executes an SQL query from <see cref="_preparedStatements">_preparedStatements</see>, using <seealso cref="StatementType">statementType</seealso> as a key.
		///
		/// Returns a list of results if <paramref name="needsResult">needsResult</paramref> is true, otherwise returns null.
		/// </summary>
		/// <param name="command">Which SQL command to execute, using <see cref="StatementType">statementType</see> as an index.</param>
		/// <param name="parameters">A list of <see cref="SqliteParameter">SqliteParameter's</see>.</param>
		/// <param name="needsResult">Returns a list of results if true, otherwise returns null.</param>
		/// <returns><see cref="List{T}">List&lt;dynamic&gt;</see> if <paramref name="needsResult">needsResult</paramref> is true, otherwise returns null.</returns>
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

		public SqliteUser(string password, string databaseName, SqliteOpenMode openMode, SqliteCacheMode cacheMode)
		{
			SqliteConnectionStringBuilder connectionString = new();
			connectionString.Mode = openMode;
			connectionString.Cache = cacheMode;
			connectionString.DataSource = databaseName;
			connectionString.Password = password;
			_connection = new(connectionString.ToString());
			_connection.Open();
			_logger.Debug("Creating guild_cache table if it doesn't exist...");
			SqliteCommand createStrikeTable = new(File.ReadAllText(Path.Join(FileSystem.ProjectRoot, "res/sql/drivers/sqlite/guild_cache_table.sql")), _connection);
			_ = createStrikeTable.ExecuteNonQuery();
			createStrikeTable.Dispose();

			_logger.Info("Preparing SQL commands...");
			_logger.Debug($"Preparing {StatementType.Insert}...");
			SqliteCommand insertUser = new("INSERT INTO guild_cache(guild_id, user_id) VALUES(@guildId, @userId)", _connection);
			_ = insertUser.Parameters.Add(new("guildId", SqliteType.Integer, sizeof(ulong), "guild_id"));
			_ = insertUser.Parameters.Add(new("userId", SqliteType.Integer, sizeof(ulong), "user_id"));
			insertUser.Prepare();
			_preparedStatements.Add(StatementType.Insert, insertUser);

			_logger.Debug($"Preparing {StatementType.Exists}...");
			SqliteCommand exists = new("SELECT true FROM guild_config WHERE guild_id=@guildId", _connection);
			_ = exists.Parameters.Add(new("@guildId", SqliteType.Integer, sizeof(ulong), "guild_id"));
			exists.Prepare();
			_preparedStatements.Add(StatementType.Exists, exists);

			_logger.Debug($"Preparing {StatementType.GetRoles}...");
			SqliteCommand getRoles = new("SELECT role_ids FROM guild_cache WHERE user_id=@userId AND guild_id=@guildId", _connection);
			_ = getRoles.Parameters.Add(new("userId", SqliteType.Integer, sizeof(ulong), "user_id"));
			_ = getRoles.Parameters.Add(new("guildId", SqliteType.Integer, sizeof(ulong), "guild_id"));
			getRoles.Prepare();
			_preparedStatements.Add(StatementType.GetRoles, getRoles);

			_logger.Debug($"Preparing {StatementType.SetRoles}...");
			SqliteCommand setRoles = new("UPDATE guild_cache SET role_ids=@roleIds WHERE user_id=@userId AND guild_id=@guildId", _connection);
			_ = setRoles.Parameters.Add(new("roleIds", SqliteType.Text));
			_ = setRoles.Parameters.Add(new("userId", SqliteType.Integer, sizeof(ulong), "user_id"));
			_ = setRoles.Parameters.Add(new("guildId", SqliteType.Integer, sizeof(ulong), "guild_id"));
			setRoles.Prepare();
			_preparedStatements.Add(StatementType.SetRoles, setRoles);

			_logger.Debug($"Preparing {StatementType.GetIsMuted}...");
			SqliteCommand getMuted = new("SELECT muted FROM guild_cache WHERE user_id=@userId AND guild_id=@guildId", _connection);
			_ = getMuted.Parameters.Add(new("userId", SqliteType.Integer, sizeof(ulong), "user_id"));
			_ = getMuted.Parameters.Add(new("guildId", SqliteType.Integer, sizeof(ulong), "guild_id"));
			getMuted.Prepare();
			_preparedStatements.Add(StatementType.GetIsMuted, getMuted);

			_logger.Debug($"Preparing {StatementType.SetIsMuted}...");
			SqliteCommand setMuted = new("UPDATE guild_cache SET muted=@isMuted WHERE user_id=@userId AND guild_id=@guildId", _connection);
			_ = setMuted.Parameters.Add(new("isMuted", SqliteType.Integer, sizeof(bool), "is_muted"));
			_ = setMuted.Parameters.Add(new("userId", SqliteType.Integer, sizeof(ulong), "user_id"));
			_ = setMuted.Parameters.Add(new("guildId", SqliteType.Integer, sizeof(ulong), "guild_id"));
			setMuted.Prepare();
			_preparedStatements.Add(StatementType.SetIsMuted, setMuted);

			_logger.Debug($"Preparing {StatementType.GetIsAntiMemed}...");
			SqliteCommand getAntiMemed = new("SELECT antimemed FROM guild_cache WHERE user_id=@userId AND guild_id=@guildId", _connection);
			_ = getAntiMemed.Parameters.Add(new("userId", SqliteType.Integer, sizeof(ulong), "user_id"));
			_ = getAntiMemed.Parameters.Add(new("guildId", SqliteType.Integer, sizeof(ulong), "guild_id"));
			getAntiMemed.Prepare();
			_preparedStatements.Add(StatementType.GetIsAntiMemed, getAntiMemed);

			_logger.Debug($"Preparing {StatementType.SetIsAntiMemed}...");
			SqliteCommand setAntiMemed = new("UPDATE guild_cache SET antimemed=@isAntiMemed WHERE user_id=@userId AND guild_id=@guildId", _connection);
			_ = setAntiMemed.Parameters.Add(new("isAntiMemed", SqliteType.Integer, sizeof(bool), "antimemed"));
			_ = setAntiMemed.Parameters.Add(new("userId", SqliteType.Integer, sizeof(ulong), "user_id"));
			_ = setAntiMemed.Parameters.Add(new("guildId", SqliteType.Integer, sizeof(ulong), "guild_id"));
			setAntiMemed.Prepare();
			_preparedStatements.Add(StatementType.SetIsAntiMemed, setAntiMemed);

			_logger.Debug($"Preparing {StatementType.GetIsNoVC}...");
			SqliteCommand getVcBanned = new("SELECT vc_banned FROM guild_cache WHERE user_id=@userId AND guild_id=@guildId", _connection);
			_ = getVcBanned.Parameters.Add(new("userId", SqliteType.Integer, sizeof(ulong), "user_id"));
			_ = getVcBanned.Parameters.Add(new("guildId", SqliteType.Integer, sizeof(ulong), "guild_id"));
			getVcBanned.Prepare();
			_preparedStatements.Add(StatementType.GetIsNoVC, getVcBanned);

			_logger.Debug($"Preparing {StatementType.SetIsNoVC}...");
			SqliteCommand setIsNoVC = new("UPDATE guild_cache SET vc_banned=@isVcBanned WHERE user_id=@userId AND guild_id=@guildId", _connection);
			_ = setIsNoVC.Parameters.Add(new("isVcBanned", SqliteType.Integer, sizeof(bool), "vc_banned"));
			_ = setIsNoVC.Parameters.Add(new("userId", SqliteType.Integer, sizeof(ulong), "user_id"));
			_ = setIsNoVC.Parameters.Add(new("guildId", SqliteType.Integer, sizeof(ulong), "guild_id"));
			setIsNoVC.Prepare();
			_preparedStatements.Add(StatementType.SetIsNoVC, setIsNoVC);
			_logger.Debug("Done preparing commands!");
		}

		public void Dispose()
		{
			_preparedStatements.Clear();
			_connection.Dispose();
			GC.SuppressFinalize(this);
		}

		public void Insert(ulong guildId, ulong userId) => ExecuteQuery(StatementType.Insert, new List<SqliteParameter>() { new SqliteParameter("guildId", guildId), new SqliteParameter("userId", userId) });
		public bool Exists(ulong guildId, ulong userId) => ExecuteQuery(StatementType.Exists, new List<SqliteParameter>() { new SqliteParameter("guildId", guildId), new SqliteParameter("userId", userId) })[0][0] != null;
		public ulong[] GetRoles(ulong guildId, ulong userId) => ExecuteQuery(StatementType.GetRoles, new List<SqliteParameter>() { new SqliteParameter("guildId", guildId), new SqliteParameter("userId", userId) }, true)?[0][0].Split(',').Cast<ulong>().ToArray();
		public void AddRole(ulong guildId, ulong userId, ulong roleId)
		{
			string currentRoles = ExecuteQuery(StatementType.GetRoles, new List<SqliteParameter>() { new SqliteParameter("@guildId", guildId), new SqliteParameter("@userId", userId) }, true)?[0][0];
			if (currentRoles == null) _ = ExecuteQuery(StatementType.SetRoles, new List<SqliteParameter>() { new SqliteParameter("@guildId", guildId), new SqliteParameter("@userId", userId), new SqliteParameter("@roleIds", roleId) });
			else _ = ExecuteQuery(StatementType.SetRoles, new List<SqliteParameter>() { new SqliteParameter("@guildId", guildId), new SqliteParameter("@userId", userId), new SqliteParameter("@roleIds", $"{currentRoles},{roleId}") });
		}
		public void RemoveRole(ulong guildId, ulong userId, ulong roleId)
		{
			string currentRoles = ExecuteQuery(StatementType.GetRoles, new List<SqliteParameter>() { new SqliteParameter("@guildId", guildId), new SqliteParameter("@userId", userId) })?[0][0];
			if (currentRoles == null) return;
			else
			{
				string newRoles = currentRoles.Remove(currentRoles.IndexOf(roleId.ToString()), currentRoles.Length);
				_ = ExecuteQuery(StatementType.SetRoles, new List<SqliteParameter>() { new SqliteParameter("@guildId", guildId), new SqliteParameter("@userId", userId), new SqliteParameter("@roleIds", newRoles) });
			}
		}
		public void SetRoles(ulong guildId, ulong userId, ulong[] roleIds) => ExecuteQuery(StatementType.SetRoles, new List<SqliteParameter>() { new SqliteParameter("guildId", guildId), new SqliteParameter("userId", userId), new SqliteParameter("roleId", roleIds) });

		public void AddStrike(ulong guildId, ulong userId) => ExecuteQuery(StatementType.AddStrike, new List<SqliteParameter>() { new SqliteParameter("guildId", guildId), new SqliteParameter("userId", userId) });
		public void RemoveStrike(ulong guildId, ulong userId) => ExecuteQuery(StatementType.RemoveStrike, new List<SqliteParameter>() { new SqliteParameter("guildId", guildId), new SqliteParameter("userId", userId) });
		public int GetStrikeCount(ulong guildId, ulong userId) => ExecuteQuery(StatementType.GetStrikeCount, new List<SqliteParameter>() { new SqliteParameter("guildId", guildId), new SqliteParameter("userId", userId) }, true)?[0][0];
		public void SetStrikeCount(ulong guildId, ulong userId, int strikeCount) => ExecuteQuery(StatementType.SetStrikeCount, new List<SqliteParameter>() { new SqliteParameter("guildId", guildId), new SqliteParameter("userId", userId), new SqliteParameter("strikeCount", (short)strikeCount) });

		public bool IsMuted(ulong guildId, ulong userId) => ExecuteQuery(StatementType.GetIsMuted, new List<SqliteParameter>() { new SqliteParameter("guildId", guildId), new SqliteParameter("userId", userId) }, true)?[0][0];
		public void IsMuted(ulong guildId, ulong userId, bool isMuted) => ExecuteQuery(StatementType.SetIsMuted, new List<SqliteParameter>() { new SqliteParameter("guildId", guildId), new SqliteParameter("userId", userId), new SqliteParameter("isMuted", isMuted) });

		public bool IsAntiMemed(ulong guildId, ulong userId) => ExecuteQuery(StatementType.GetIsAntiMemed, new List<SqliteParameter>() { new SqliteParameter("guildId", guildId), new SqliteParameter("userId", userId) }, true)?[0][0];
		public void IsAntiMemed(ulong guildId, ulong userId, bool isNoMemed) => ExecuteQuery(StatementType.SetIsAntiMemed, new List<SqliteParameter>() { new SqliteParameter("guildId", guildId), new SqliteParameter("userId", userId), new SqliteParameter("isNoMemed", isNoMemed) });

		public bool IsNoVC(ulong guildId, ulong userId) => ExecuteQuery(StatementType.GetIsNoVC, new List<SqliteParameter>() { new SqliteParameter("guildId", guildId), new SqliteParameter("userId", userId) }, true)?[0][0];
		public void IsNoVC(ulong guildId, ulong userId, bool isNoVC) => ExecuteQuery(StatementType.SetIsNoVC, new List<SqliteParameter>() { new SqliteParameter("guildId", guildId), new SqliteParameter("userId", userId), new SqliteParameter("isNoVC", isNoVC) });
	}
}
