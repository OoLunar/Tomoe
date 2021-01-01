using System.Collections.Generic;
using System.IO;
using System.Linq;
using Npgsql;
using NpgsqlTypes;
using Tomoe.Database.Interfaces;
using Tomoe.Utils;
using System;

namespace Tomoe.Database.Drivers.PostgresSQL
{
	public class PostgresUser : IUser
	{
		private readonly static Logger Logger = new Logger("Database.PostgresSQL.User");
		private readonly NpgsqlConnection Connection;
		private readonly Dictionary<StatementType, NpgsqlCommand> PreparedStatements = new Dictionary<StatementType, NpgsqlCommand>();
		private enum StatementType
		{
			InsertUser,
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
			GetIsNoMemed,
			SetIsNoMemed,
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
			Logger.Trace($"Executing prepared statement \"{command}\" with parameters: {string.Join(", ", statement.Parameters.Select(param => param.Value).ToArray())}");

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
						Logger.Trace($"Recieved values: {reader[i] ?? "null"} on iteration {i}");
					}

					if (list.Count == 1 && list[0] == null) values.Add(indexCount, null);
					else values.Add(indexCount, list);
					indexCount++;
				}
				reader.DisposeAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				if (values.Count == 0 || (values.Count == 1 && values[0] == null)) values = null;
				return values;
			}
			else
			{
				_ = statement.ExecuteNonQuery();
				return null;
			}
		}

		/// <inheritdoc cref="ExecuteQuery(StatementType, List{NpgsqlParameter}, bool)" />
		/// <param name="parameter">One <see cref="NpgsqlParameter">NpgsqlParameter</see>, which gets converted into a <see cref="List{T}">List&lt;NpgsqlParameter&gt;</see>.</param>
		private Dictionary<int, List<dynamic>> ExecuteQuery(StatementType command, NpgsqlParameter parameter, bool needsResult = false) => ExecuteQuery(command, new List<NpgsqlParameter> { parameter }, needsResult);

		public PostgresUser(string host, int port, string username, string password, string database_name, SslMode sslMode)
		{
			Connection = new NpgsqlConnection($"Host={host};Port={port};Username={username};Password={password};Database={database_name};SSL Mode={sslMode}");
			Logger.Info("Opening connection to database...");
			try
			{
				Connection.Open();
				NpgsqlCommand createGuildCacheTable = new NpgsqlCommand(File.ReadAllText(Path.Join(FileSystem.ProjectRoot, "res/sql/drivers/postgresql/guild_cache_table.sql")), Connection);
				_ = createGuildCacheTable.ExecuteNonQuery();
				createGuildCacheTable.Dispose();
			}
			catch (System.Net.Sockets.SocketException error)
			{
				Logger.Critical($"Failed to connect to database. {error.Message}", true);
			}
			Logger.Info("Preparing SQL commands...");
			Logger.Debug($"Preparing {StatementType.InsertUser}...");
			NpgsqlCommand insertUser = new NpgsqlCommand("INSERT INTO guild_cache(guild_id, user_id) VALUES(@guildId, @userId)", Connection);
			_ = insertUser.Parameters.Add(new NpgsqlParameter("guildId", NpgsqlDbType.Bigint));
			_ = insertUser.Parameters.Add(new NpgsqlParameter("userId", NpgsqlDbType.Bigint));
			insertUser.Prepare();
			PreparedStatements.Add(StatementType.InsertUser, insertUser);

			Logger.Debug($"Preparing {StatementType.GetRoles}...");
			NpgsqlCommand getRoles = new NpgsqlCommand("SELECT role_ids FROM guild_cache WHERE user_id=@userId AND guild_id=@guildId", Connection);
			_ = getRoles.Parameters.Add(new NpgsqlParameter("userId", NpgsqlDbType.Bigint));
			_ = getRoles.Parameters.Add(new NpgsqlParameter("guildId", NpgsqlDbType.Bigint));
			getRoles.Prepare();
			PreparedStatements.Add(StatementType.GetRoles, getRoles);

			Logger.Debug($"Preparing {StatementType.AddRole}...");
			NpgsqlCommand addRole = new NpgsqlCommand("UPDATE guild_cache SET role_ids=array_append(role_ids, @roleId) WHERE user_id=@userId AND guild_id=@guildId", Connection);
			_ = addRole.Parameters.Add(new NpgsqlParameter("roleId", NpgsqlDbType.Bigint));
			_ = addRole.Parameters.Add(new NpgsqlParameter("userId", NpgsqlDbType.Bigint));
			_ = addRole.Parameters.Add(new NpgsqlParameter("guildId", NpgsqlDbType.Bigint));
			addRole.Prepare();
			PreparedStatements.Add(StatementType.AddRole, addRole);

			Logger.Debug($"Preparing {StatementType.RemoveRole}...");
			NpgsqlCommand removeRole = new NpgsqlCommand("UPDATE guild_cache SET role_ids=array_remove(role_ids, @roleId) WHERE user_id=@userId AND guild_id=@guildId", Connection);
			_ = removeRole.Parameters.Add(new NpgsqlParameter("roleId", NpgsqlDbType.Bigint));
			_ = removeRole.Parameters.Add(new NpgsqlParameter("userId", NpgsqlDbType.Bigint));
			_ = removeRole.Parameters.Add(new NpgsqlParameter("guildId", NpgsqlDbType.Bigint));
			removeRole.Prepare();
			PreparedStatements.Add(StatementType.RemoveRole, removeRole);

			Logger.Debug($"Preparing {StatementType.SetRoles}...");
			NpgsqlCommand setRoles = new NpgsqlCommand("UPDATE guild_cache SET role_ids=@roleIds WHERE user_id=@userId AND guild_id=@guildId", Connection);
			_ = setRoles.Parameters.Add(new NpgsqlParameter("roleIds", NpgsqlDbType.Array | NpgsqlDbType.Bigint));
			_ = setRoles.Parameters.Add(new NpgsqlParameter("userId", NpgsqlDbType.Bigint));
			_ = setRoles.Parameters.Add(new NpgsqlParameter("guildId", NpgsqlDbType.Bigint));
			setRoles.Prepare();
			PreparedStatements.Add(StatementType.SetRoles, setRoles);

			Logger.Debug($"Preparing {StatementType.GetIsMuted}...");
			NpgsqlCommand getIsMuted = new NpgsqlCommand("SELECT muted FROM guild_cache WHERE user_id=@userId AND guild_id=@guildId", Connection);
			_ = getIsMuted.Parameters.Add(new NpgsqlParameter("userId", NpgsqlDbType.Bigint));
			_ = getIsMuted.Parameters.Add(new NpgsqlParameter("guildId", NpgsqlDbType.Bigint));
			getIsMuted.Prepare();
			PreparedStatements.Add(StatementType.GetIsMuted, getIsMuted);

			Logger.Debug($"Preparing {StatementType.SetIsMuted}...");
			NpgsqlCommand setIsMuted = new NpgsqlCommand("UPDATE guild_cache SET muted=@isMuted WHERE user_id=@userId AND guild_id=@guildId", Connection);
			_ = setIsMuted.Parameters.Add(new NpgsqlParameter("isMuted", NpgsqlDbType.Boolean));
			_ = setIsMuted.Parameters.Add(new NpgsqlParameter("userId", NpgsqlDbType.Bigint));
			_ = setIsMuted.Parameters.Add(new NpgsqlParameter("guildId", NpgsqlDbType.Bigint));
			setIsMuted.Prepare();
			PreparedStatements.Add(StatementType.SetIsMuted, setIsMuted);

			Logger.Debug($"Preparing {StatementType.GetIsNoMemed}...");
			NpgsqlCommand getIsNoMemed = new NpgsqlCommand("SELECT no_memed FROM guild_cache WHERE user_id=@userId AND guild_id=@guildId", Connection);
			_ = getIsNoMemed.Parameters.Add(new NpgsqlParameter("userId", NpgsqlDbType.Bigint));
			_ = getIsNoMemed.Parameters.Add(new NpgsqlParameter("guildId", NpgsqlDbType.Bigint));
			getIsNoMemed.Prepare();
			PreparedStatements.Add(StatementType.GetIsNoMemed, getIsNoMemed);

			Logger.Debug($"Preparing {StatementType.SetIsNoMemed}...");
			NpgsqlCommand setIsNoMemed = new NpgsqlCommand("UPDATE guild_cache SET no_memed=@isNoMemed WHERE user_id=@userId AND guild_id=@guildId", Connection);
			_ = setIsNoMemed.Parameters.Add(new NpgsqlParameter("isNoMemed", NpgsqlDbType.Boolean));
			_ = setIsNoMemed.Parameters.Add(new NpgsqlParameter("userId", NpgsqlDbType.Bigint));
			_ = setIsNoMemed.Parameters.Add(new NpgsqlParameter("guildId", NpgsqlDbType.Bigint));
			setIsNoMemed.Prepare();
			PreparedStatements.Add(StatementType.SetIsNoMemed, setIsNoMemed);

			Logger.Debug($"Preparing {StatementType.GetIsNoVC}...");
			NpgsqlCommand getIsNoVC = new NpgsqlCommand("SELECT no_voicechat FROM guild_cache WHERE user_id=@userId AND guild_id=@guildId", Connection);
			_ = getIsNoVC.Parameters.Add(new NpgsqlParameter("userId", NpgsqlDbType.Bigint));
			_ = getIsNoVC.Parameters.Add(new NpgsqlParameter("guildId", NpgsqlDbType.Bigint));
			getIsNoVC.Prepare();
			PreparedStatements.Add(StatementType.GetIsNoVC, getIsNoVC);

			Logger.Debug($"Preparing {StatementType.SetIsNoVC}...");
			NpgsqlCommand setIsNoVC = new NpgsqlCommand("UPDATE guild_cache SET no_voicechat=@isNoVC WHERE user_id=@userId AND guild_id=@guildId", Connection);
			_ = setIsNoVC.Parameters.Add(new NpgsqlParameter("isNoVC", NpgsqlDbType.Boolean));
			_ = setIsNoVC.Parameters.Add(new NpgsqlParameter("userId", NpgsqlDbType.Bigint));
			_ = setIsNoVC.Parameters.Add(new NpgsqlParameter("guildId", NpgsqlDbType.Bigint));
			setIsNoVC.Prepare();
			PreparedStatements.Add(StatementType.SetIsNoVC, setIsNoVC);
			Logger.Debug("Done preparing commands!");
		}

		public void Dispose()
		{
			PreparedStatements.Clear();
			Connection.Close();
			Connection.Dispose();
			GC.SuppressFinalize(this);
		}

		public void InsertUser(ulong guildId, ulong userId) => ExecuteQuery(StatementType.InsertUser, new List<NpgsqlParameter>() { new NpgsqlParameter("guildId", (long)guildId), new NpgsqlParameter("userId", (long)userId) });
		public ulong[] GetRoles(ulong guildId, ulong userId) => ExecuteQuery(StatementType.GetRoles, new List<NpgsqlParameter>() { new NpgsqlParameter("guildId", (long)guildId), new NpgsqlParameter("userId", (long)userId) }, true)?[0].ConvertAll<ulong>(roleId => ulong.Parse(roleId)).ToArray() ?? null;
		public void AddRole(ulong guildId, ulong userId, ulong roleId) => ExecuteQuery(StatementType.AddRole, new List<NpgsqlParameter>() { new NpgsqlParameter("guildId", (long)guildId), new NpgsqlParameter("userId", (long)userId), new NpgsqlParameter("roleId", (long)roleId) });
		public void RemoveRole(ulong guildId, ulong userId, ulong roleId) => ExecuteQuery(StatementType.RemoveRole, new List<NpgsqlParameter>() { new NpgsqlParameter("guildId", (long)guildId), new NpgsqlParameter("userId", (long)userId), new NpgsqlParameter("roleId", (long)roleId) });
		public void SetRoles(ulong guildId, ulong userId, ulong[] roleIds) => ExecuteQuery(StatementType.SetRoles, new List<NpgsqlParameter>() { new NpgsqlParameter("guildId", (long)guildId), new NpgsqlParameter("userId", (long)userId), new NpgsqlParameter("roleId", roleIds.Select((role) => long.Parse(role.ToString()))) });

		public void AddStrike(ulong guildId, ulong userId) => ExecuteQuery(StatementType.AddStrike, new List<NpgsqlParameter>() { new NpgsqlParameter("guildId", (long)guildId), new NpgsqlParameter("userId", (long)userId) });
		public void RemoveStrike(ulong guildId, ulong userId) => ExecuteQuery(StatementType.RemoveStrike, new List<NpgsqlParameter>() { new NpgsqlParameter("guildId", (long)guildId), new NpgsqlParameter("userId", (long)userId) });
		public int GetStrikeCount(ulong guildId, ulong userId) => ExecuteQuery(StatementType.GetStrikeCount, new List<NpgsqlParameter>() { new NpgsqlParameter("guildId", (long)guildId), new NpgsqlParameter("userId", (long)userId) }, true)?[0][0] ?? null;
		public void SetStrikeCount(ulong guildId, ulong userId, int strikeCount) => ExecuteQuery(StatementType.SetStrikeCount, new List<NpgsqlParameter>() { new NpgsqlParameter("guildId", (long)guildId), new NpgsqlParameter("userId", (long)userId), new NpgsqlParameter("strikeCount", (short)strikeCount) });

		public bool IsMuted(ulong guildId, ulong userId) => ExecuteQuery(StatementType.GetIsMuted, new List<NpgsqlParameter>() { new NpgsqlParameter("guildId", (long)guildId), new NpgsqlParameter("userId", (long)userId) }, true)?[0][0] ?? null;
		public void IsMuted(ulong guildId, ulong userId, bool isMuted) => ExecuteQuery(StatementType.SetIsMuted, new List<NpgsqlParameter>() { new NpgsqlParameter("guildId", (long)guildId), new NpgsqlParameter("userId", (long)userId), new NpgsqlParameter("isMuted", isMuted) });

		public bool IsNoMemed(ulong guildId, ulong userId) => ExecuteQuery(StatementType.GetIsNoMemed, new List<NpgsqlParameter>() { new NpgsqlParameter("guildId", (long)guildId), new NpgsqlParameter("userId", (long)userId) }, true)?[0][0] ?? null;
		public void IsNoMemed(ulong guildId, ulong userId, bool isNoMemed) => ExecuteQuery(StatementType.SetIsNoMemed, new List<NpgsqlParameter>() { new NpgsqlParameter("guildId", (long)guildId), new NpgsqlParameter("userId", (long)userId), new NpgsqlParameter("isNoMemed", isNoMemed) });

		public bool IsNoVC(ulong guildId, ulong userId) => ExecuteQuery(StatementType.GetIsNoVC, new List<NpgsqlParameter>() { new NpgsqlParameter("guildId", (long)guildId), new NpgsqlParameter("userId", (long)userId) }, true)?[0][0] ?? null;
		public void IsNoVC(ulong guildId, ulong userId, bool isNoVC) => ExecuteQuery(StatementType.SetIsNoVC, new List<NpgsqlParameter>() { new NpgsqlParameter("guildId", (long)guildId), new NpgsqlParameter("userId", (long)userId), new NpgsqlParameter("isNoVC", isNoVC) });
	}
}
