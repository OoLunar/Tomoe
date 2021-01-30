using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;

using Npgsql;

using NpgsqlTypes;

using Tomoe.Database.Interfaces;
using Tomoe.Utils;

namespace Tomoe.Database.Drivers.PostgreSQL
{
	public class PostgresUser : IUser
	{
		private static readonly Logger _logger = new("Database.PostgresSQL.User");
		private readonly NpgsqlConnection _connection;
		private readonly Dictionary<StatementType, NpgsqlCommand> PreparedStatements = new();
		private int retryCount;
		private enum StatementType
		{
			Insert,
			Exists,
			GetRoles,
			AddRole,
			RemoveRole,
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
		/// Executes an SQL query from <see cref="PreparedStatements">_preparedStatements</see>, using <seealso cref="StatementType">statementType</seealso> as a key.
		///
		/// Returns a list of results if <paramref name="needsResult">needsResult</paramref> is true, otherwise returns null.
		/// </summary>
		/// <param name="command">Which SQL command to execute, using <see cref="StatementType">statementType</see> as an index.</param>
		/// <param name="parameters">A list of <see cref="NpgsqlParameter">NpgsqlParameter's</see>.</param>
		/// <param name="needsResult">Returns a list of results if true, otherwise returns null.</param>
		/// <returns><see cref="List{T}">List&lt;dynamic&gt;</see> if <paramref name="needsResult">needsResult</paramref> is true, otherwise returns null.</returns>
		private Dictionary<int, List<dynamic>> ExecuteQuery(StatementType command, List<NpgsqlParameter> parameters, bool needsResult = false)
		{
			NpgsqlCommand statement = PreparedStatements[command];
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
				if (retryCount > Config.Database.MaxRetryCount) _logger.Critical($"Failed to execute query \"{command}\" after {retryCount} times. Check your internet connection.");
				else retryCount++;
				_logger.Error($"Socket exception occured, retrying... Details: {error.Message}\n{error.StackTrace}");
				return ExecuteQuery(command, parameters, needsResult);
			}
		}

		/// <inheritdoc cref="ExecuteQuery(StatementType, List{NpgsqlParameter}, bool)" />
		/// <param name="parameter">One <see cref="NpgsqlParameter">NpgsqlParameter</see>, which gets converted into a <see cref="List{T}">List&lt;NpgsqlParameter&gt;</see>.</param>
		private Dictionary<int, List<dynamic>> ExecuteQuery(StatementType command, NpgsqlParameter parameter, bool needsResult = false) => ExecuteQuery(command, new List<NpgsqlParameter> { parameter }, needsResult);

		public PostgresUser(string host, int port, string username, string password, string database_name, SslMode sslMode)
		{
			_connection = new($"Host={host};Port={port};Username={username};Password={password};Database={database_name};SSL Mode={sslMode}");
			_logger.Info("Opening connection to database...");
			try
			{
				_connection.Open();
				_logger.Debug("Creating guild_cache table if it doesn't exist...");
				NpgsqlCommand createGuildCacheTable = new(File.ReadAllText(Path.Join(FileSystem.ProjectRoot, "res/sql/drivers/postgresql/guild_cache_table.sql")), _connection);
				_ = createGuildCacheTable.ExecuteNonQuery();
				createGuildCacheTable.Dispose();
			}
			catch (SocketException error) { _logger.Critical($"Failed to connect to database. {error.Message}", true); }
			//catch (PostgresException error) when (error.SqlState == "28P01") { _logger.Critical($"Failed to connect to database. Invalid Password.", true); }
			_logger.Info("Preparing SQL commands...");
			_logger.Debug($"Preparing {StatementType.Insert}...");
			NpgsqlCommand insert = new("INSERT INTO guild_cache(guild_id, user_id) VALUES(@guildId, @userId)", _connection);
			_ = insert.Parameters.Add(new("guildId", NpgsqlDbType.Bigint));
			_ = insert.Parameters.Add(new("userId", NpgsqlDbType.Bigint));
			insert.Prepare();
			PreparedStatements.Add(StatementType.Insert, insert);

			_logger.Debug($"Preparing {StatementType.Insert}...");
			NpgsqlCommand exists = new("SELECT exists(select 1 FROM guild_cache WHERE guild_id=@guildId AND user_id=@userId)", _connection);
			_ = exists.Parameters.Add(new("guildId", NpgsqlDbType.Bigint));
			_ = exists.Parameters.Add(new("userId", NpgsqlDbType.Bigint));
			exists.Prepare();
			PreparedStatements.Add(StatementType.Exists, exists);

			_logger.Debug($"Preparing {StatementType.GetRoles}...");
			NpgsqlCommand getRoles = new("SELECT role_ids FROM guild_cache WHERE user_id=@userId AND guild_id=@guildId", _connection);
			_ = getRoles.Parameters.Add(new("userId", NpgsqlDbType.Bigint));
			_ = getRoles.Parameters.Add(new("guildId", NpgsqlDbType.Bigint));
			getRoles.Prepare();
			PreparedStatements.Add(StatementType.GetRoles, getRoles);

			_logger.Debug($"Preparing {StatementType.AddRole}...");
			NpgsqlCommand addRole = new("UPDATE guild_cache SET role_ids=array_append(role_ids, @roleId) WHERE user_id=@userId AND guild_id=@guildId", _connection);
			_ = addRole.Parameters.Add(new("roleId", NpgsqlDbType.Bigint));
			_ = addRole.Parameters.Add(new("userId", NpgsqlDbType.Bigint));
			_ = addRole.Parameters.Add(new("guildId", NpgsqlDbType.Bigint));
			addRole.Prepare();
			PreparedStatements.Add(StatementType.AddRole, addRole);

			_logger.Debug($"Preparing {StatementType.RemoveRole}...");
			NpgsqlCommand removeRole = new("UPDATE guild_cache SET role_ids=array_remove(role_ids, @roleId) WHERE user_id=@userId AND guild_id=@guildId", _connection);
			_ = removeRole.Parameters.Add(new("roleId", NpgsqlDbType.Bigint));
			_ = removeRole.Parameters.Add(new("userId", NpgsqlDbType.Bigint));
			_ = removeRole.Parameters.Add(new("guildId", NpgsqlDbType.Bigint));
			removeRole.Prepare();
			PreparedStatements.Add(StatementType.RemoveRole, removeRole);

			_logger.Debug($"Preparing {StatementType.SetRoles}...");
			NpgsqlCommand setRoles = new("UPDATE guild_cache SET role_ids=@roleIds WHERE user_id=@userId AND guild_id=@guildId", _connection);
			_ = setRoles.Parameters.Add(new("roleIds", NpgsqlDbType.Array | NpgsqlDbType.Bigint));
			_ = setRoles.Parameters.Add(new("userId", NpgsqlDbType.Bigint));
			_ = setRoles.Parameters.Add(new("guildId", NpgsqlDbType.Bigint));
			setRoles.Prepare();
			PreparedStatements.Add(StatementType.SetRoles, setRoles);

			_logger.Debug($"Preparing {StatementType.GetIsMuted}...");
			NpgsqlCommand getIsMuted = new("SELECT muted FROM guild_cache WHERE user_id=@userId AND guild_id=@guildId", _connection);
			_ = getIsMuted.Parameters.Add(new("userId", NpgsqlDbType.Bigint));
			_ = getIsMuted.Parameters.Add(new("guildId", NpgsqlDbType.Bigint));
			getIsMuted.Prepare();
			PreparedStatements.Add(StatementType.GetIsMuted, getIsMuted);

			_logger.Debug($"Preparing {StatementType.SetIsMuted}...");
			NpgsqlCommand setIsMuted = new("UPDATE guild_cache SET muted=@isMuted WHERE user_id=@userId AND guild_id=@guildId", _connection);
			_ = setIsMuted.Parameters.Add(new("isMuted", NpgsqlDbType.Boolean));
			_ = setIsMuted.Parameters.Add(new("userId", NpgsqlDbType.Bigint));
			_ = setIsMuted.Parameters.Add(new("guildId", NpgsqlDbType.Bigint));
			setIsMuted.Prepare();
			PreparedStatements.Add(StatementType.SetIsMuted, setIsMuted);

			_logger.Debug($"Preparing {StatementType.GetIsAntiMemed}...");
			NpgsqlCommand getIsNoMemed = new("SELECT no_memed FROM guild_cache WHERE user_id=@userId AND guild_id=@guildId", _connection);
			_ = getIsNoMemed.Parameters.Add(new("userId", NpgsqlDbType.Bigint));
			_ = getIsNoMemed.Parameters.Add(new("guildId", NpgsqlDbType.Bigint));
			getIsNoMemed.Prepare();
			PreparedStatements.Add(StatementType.GetIsAntiMemed, getIsNoMemed);

			_logger.Debug($"Preparing {StatementType.SetIsAntiMemed}...");
			NpgsqlCommand setIsNoMemed = new("UPDATE guild_cache SET no_memed=@isNoMemed WHERE user_id=@userId AND guild_id=@guildId", _connection);
			_ = setIsNoMemed.Parameters.Add(new("isNoMemed", NpgsqlDbType.Boolean));
			_ = setIsNoMemed.Parameters.Add(new("userId", NpgsqlDbType.Bigint));
			_ = setIsNoMemed.Parameters.Add(new("guildId", NpgsqlDbType.Bigint));
			setIsNoMemed.Prepare();
			PreparedStatements.Add(StatementType.SetIsAntiMemed, setIsNoMemed);

			_logger.Debug($"Preparing {StatementType.GetIsNoVC}...");
			NpgsqlCommand getIsNoVC = new("SELECT no_voicechat FROM guild_cache WHERE user_id=@userId AND guild_id=@guildId", _connection);
			_ = getIsNoVC.Parameters.Add(new("userId", NpgsqlDbType.Bigint));
			_ = getIsNoVC.Parameters.Add(new("guildId", NpgsqlDbType.Bigint));
			getIsNoVC.Prepare();
			PreparedStatements.Add(StatementType.GetIsNoVC, getIsNoVC);

			_logger.Debug($"Preparing {StatementType.SetIsNoVC}...");
			NpgsqlCommand setIsNoVC = new("UPDATE guild_cache SET no_voicechat=@isNoVC WHERE user_id=@userId AND guild_id=@guildId", _connection);
			_ = setIsNoVC.Parameters.Add(new("isNoVC", NpgsqlDbType.Boolean));
			_ = setIsNoVC.Parameters.Add(new("userId", NpgsqlDbType.Bigint));
			_ = setIsNoVC.Parameters.Add(new("guildId", NpgsqlDbType.Bigint));
			setIsNoVC.Prepare();
			PreparedStatements.Add(StatementType.SetIsNoVC, setIsNoVC);
			_logger.Debug("Done preparing commands!");
		}

		public void Dispose()
		{
			PreparedStatements.Clear();
			_connection.Close();
			_connection.Dispose();
			GC.SuppressFinalize(this);
		}

		public void Insert(ulong guildId, ulong userId) => ExecuteQuery(StatementType.Insert, new List<NpgsqlParameter>() { new NpgsqlParameter("guildId", (long)guildId), new NpgsqlParameter("userId", (long)userId) });
		public bool Exists(ulong guildId, ulong userId) => ExecuteQuery(StatementType.Exists, new List<NpgsqlParameter>() { new NpgsqlParameter("guildId", (long)guildId), new NpgsqlParameter("userId", (long)userId) })[0][0];
		public ulong[] GetRoles(ulong guildId, ulong userId) => ExecuteQuery(StatementType.GetRoles, new List<NpgsqlParameter>() { new NpgsqlParameter("guildId", (long)guildId), new NpgsqlParameter("userId", (long)userId) }, true)?[0].Cast<ulong>().ToArray();
		public void AddRole(ulong guildId, ulong userId, ulong roleId) => ExecuteQuery(StatementType.AddRole, new List<NpgsqlParameter>() { new NpgsqlParameter("guildId", (long)guildId), new NpgsqlParameter("userId", (long)userId), new NpgsqlParameter("roleId", (long)roleId) });
		public void RemoveRole(ulong guildId, ulong userId, ulong roleId) => ExecuteQuery(StatementType.RemoveRole, new List<NpgsqlParameter>() { new NpgsqlParameter("guildId", (long)guildId), new NpgsqlParameter("userId", (long)userId), new NpgsqlParameter("roleId", (long)roleId) });
		public void SetRoles(ulong guildId, ulong userId, ulong[] roleIds) => ExecuteQuery(StatementType.SetRoles, new List<NpgsqlParameter>() { new NpgsqlParameter("guildId", (long)guildId), new NpgsqlParameter("userId", (long)userId), new NpgsqlParameter("roleId", roleIds.Cast<long>()) });

		public void AddStrike(ulong guildId, ulong userId) => ExecuteQuery(StatementType.AddStrike, new List<NpgsqlParameter>() { new NpgsqlParameter("guildId", (long)guildId), new NpgsqlParameter("userId", (long)userId) });
		public void RemoveStrike(ulong guildId, ulong userId) => ExecuteQuery(StatementType.RemoveStrike, new List<NpgsqlParameter>() { new NpgsqlParameter("guildId", (long)guildId), new NpgsqlParameter("userId", (long)userId) });
		public int GetStrikeCount(ulong guildId, ulong userId) => ExecuteQuery(StatementType.GetStrikeCount, new List<NpgsqlParameter>() { new NpgsqlParameter("guildId", (long)guildId), new NpgsqlParameter("userId", (long)userId) }, true)?[0][0];
		public void SetStrikeCount(ulong guildId, ulong userId, int strikeCount) => ExecuteQuery(StatementType.SetStrikeCount, new List<NpgsqlParameter>() { new NpgsqlParameter("guildId", (long)guildId), new NpgsqlParameter("userId", (long)userId), new NpgsqlParameter("strikeCount", (short)strikeCount) });

		public bool IsMuted(ulong guildId, ulong userId) => ExecuteQuery(StatementType.GetIsMuted, new List<NpgsqlParameter>() { new NpgsqlParameter("guildId", (long)guildId), new NpgsqlParameter("userId", (long)userId) }, true)?[0][0];
		public void IsMuted(ulong guildId, ulong userId, bool isMuted) => ExecuteQuery(StatementType.SetIsMuted, new List<NpgsqlParameter>() { new NpgsqlParameter("guildId", (long)guildId), new NpgsqlParameter("userId", (long)userId), new NpgsqlParameter("isMuted", isMuted) });

		public bool IsAntiMemed(ulong guildId, ulong userId) => ExecuteQuery(StatementType.GetIsAntiMemed, new List<NpgsqlParameter>() { new NpgsqlParameter("guildId", (long)guildId), new NpgsqlParameter("userId", (long)userId) }, true)?[0][0];
		public void IsAntiMemed(ulong guildId, ulong userId, bool isNoMemed) => ExecuteQuery(StatementType.SetIsAntiMemed, new List<NpgsqlParameter>() { new NpgsqlParameter("guildId", (long)guildId), new NpgsqlParameter("userId", (long)userId), new NpgsqlParameter("isNoMemed", isNoMemed) });

		public bool IsNoVC(ulong guildId, ulong userId) => ExecuteQuery(StatementType.GetIsNoVC, new List<NpgsqlParameter>() { new NpgsqlParameter("guildId", (long)guildId), new NpgsqlParameter("userId", (long)userId) }, true)?[0][0];
		public void IsNoVC(ulong guildId, ulong userId, bool isNoVC) => ExecuteQuery(StatementType.SetIsNoVC, new List<NpgsqlParameter>() { new NpgsqlParameter("guildId", (long)guildId), new NpgsqlParameter("userId", (long)userId), new NpgsqlParameter("isNoVC", isNoVC) });
	}
}
