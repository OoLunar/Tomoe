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
	public class PostgresTags : ITags
	{
		private static readonly Logger _logger = new("Database.PostgresSQL.Tags");
		private readonly NpgsqlConnection _connection;
		private readonly Dictionary<StatementType, NpgsqlCommand> _preparedStatements = new();
		private int retryCount;
		private enum StatementType
		{
			Get,
			GetGuild,
			GetUserOverall,
			GetUserGuild,
			GetAliases,
			Delete,
			DeleteAlias,
			DeleteAllAliases,
			Edit,
			Create,
			CreateAlias,
			GetAuthor,
			IsAlias,
			Exist,
			Claim,
			RealName
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

		public PostgresTags(string host, int port, string username, string password, string database_name, SslMode sslMode)
		{
			_connection = new($"Host={host};Port={port};Username={username};Password={password};Database={database_name};SSL Mode={sslMode}");
			_logger.Info("Opening connection to database...");
			try
			{
				_connection.Open();
				_logger.Debug("Creating tags table if it doesn't exist...");
				NpgsqlCommand createTagsTable = new(File.ReadAllText(Path.Join(FileSystem.ProjectRoot, "res/sql/drivers/postgresql/tags_table.sql")), _connection);
				_ = createTagsTable.ExecuteNonQuery();
				createTagsTable.Dispose();

				_logger.Debug("Creating tag_aliases table if it doesn't exist...");
				NpgsqlCommand createAliasTable = new(File.ReadAllText(Path.Join(FileSystem.ProjectRoot, "res/sql/drivers/postgresql/tag_aliases_table.sql")), _connection);
				_ = createAliasTable.ExecuteNonQuery();
				createAliasTable.Dispose();

				_logger.Debug("Creating tag functions if they don't exist...");
				NpgsqlCommand createFunctions = new(File.ReadAllText(Path.Join(FileSystem.ProjectRoot, "res/sql/drivers/postgresql/tag_functions.sql")), _connection);
				_ = createFunctions.ExecuteNonQuery();
				createFunctions.Dispose();
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
			_logger.Debug($"Preparing {StatementType.Create}...");
			NpgsqlCommand create = new("INSERT INTO tags VALUES(@tagTitle, @guildId, @userId, DEFAULT, DEFAULT, @content)", _connection);
			_ = create.Parameters.Add(new("guildId", NpgsqlDbType.Bigint));
			_ = create.Parameters.Add(new("userId", NpgsqlDbType.Bigint));
			_ = create.Parameters.Add(new("tagTitle", NpgsqlDbType.Varchar));
			_ = create.Parameters.Add(new("content", NpgsqlDbType.Varchar));
			create.Prepare();
			_preparedStatements.Add(StatementType.Create, create);

			_logger.Debug($"Preparing {StatementType.CreateAlias}...");
			NpgsqlCommand createAlias = new("INSERT INTO tag_aliases VALUES(@tagTitle, @guildId, @userId, (SELECT id FROM tags WHERE guild_id=@guildId AND title=@oldTagTitle))", _connection);
			_ = createAlias.Parameters.Add(new("userId", NpgsqlDbType.Bigint));
			_ = createAlias.Parameters.Add(new("guildId", NpgsqlDbType.Bigint));
			_ = createAlias.Parameters.Add(new("tagTitle", NpgsqlDbType.Varchar));
			_ = createAlias.Parameters.Add(new("oldTagTitle", NpgsqlDbType.Varchar));
			createAlias.Prepare();
			_preparedStatements.Add(StatementType.CreateAlias, createAlias);

			_logger.Debug($"Preparing {StatementType.Delete}...");
			NpgsqlCommand delete = new("DELETE FROM tags CASCADE WHERE guild_id=@guildId AND title=@tagTitle", _connection);
			_ = delete.Parameters.Add(new("guildId", NpgsqlDbType.Bigint));
			_ = delete.Parameters.Add(new("tagTitle", NpgsqlDbType.Varchar));
			delete.Prepare();
			_preparedStatements.Add(StatementType.Delete, delete);

			_logger.Debug($"Preparing {StatementType.DeleteAlias}...");
			NpgsqlCommand deleteAlias = new("DELETE FROM tag_aliases WHERE guild_id=@guildId AND title=@tagTitle", _connection);
			_ = deleteAlias.Parameters.Add(new("guildId", NpgsqlDbType.Bigint));
			_ = deleteAlias.Parameters.Add(new("tagTitle", NpgsqlDbType.Varchar));
			deleteAlias.Prepare();
			_preparedStatements.Add(StatementType.DeleteAlias, deleteAlias);

			_logger.Debug($"Preparing {StatementType.DeleteAllAliases}...");
			NpgsqlCommand deleteAllAliases = new("DELETE FROM tag_aliases WHERE id=(SELECT id FROM tags WHERE guild_id=@guildId AND title=@tagTitle)", _connection);
			_ = deleteAllAliases.Parameters.Add(new("guildId", NpgsqlDbType.Bigint));
			_ = deleteAllAliases.Parameters.Add(new("tagTitle", NpgsqlDbType.Varchar));
			deleteAllAliases.Prepare();
			_preparedStatements.Add(StatementType.DeleteAllAliases, deleteAllAliases);

			_logger.Debug($"Preparing {StatementType.Edit}...");
			NpgsqlCommand edit = new("UPDATE tags SET content=@content WHERE guild_id=@guildId AND title=@tagTitle", _connection);
			_ = edit.Parameters.Add(new("content", NpgsqlDbType.Varchar));
			_ = edit.Parameters.Add(new("guildId", NpgsqlDbType.Bigint));
			_ = edit.Parameters.Add(new("tagTitle", NpgsqlDbType.Varchar));
			edit.Prepare();
			_preparedStatements.Add(StatementType.Edit, edit);

			_logger.Debug($"Preparing {StatementType.Get}...");
			NpgsqlCommand get = new("SELECT get_tag_value(@guildId, @tagTitle)", _connection);
			_ = get.Parameters.Add(new("guildId", NpgsqlDbType.Bigint));
			_ = get.Parameters.Add(new("tagTitle", NpgsqlDbType.Varchar));
			get.Prepare();
			_preparedStatements.Add(StatementType.Get, get);

			_logger.Debug($"Preparing {StatementType.GetAliases}...");
			NpgsqlCommand getAliases = new("SELECT title FROM tag_aliases WHERE id=(SELECT id FROM tags WHERE guild_id=@guildId AND title=@tagTitle)", _connection);
			_ = getAliases.Parameters.Add(new("guildId", NpgsqlDbType.Bigint));
			_ = getAliases.Parameters.Add(new("tagTitle", NpgsqlDbType.Varchar));
			getAliases.Prepare();
			_preparedStatements.Add(StatementType.GetAliases, getAliases);

			_logger.Debug($"Preparing {StatementType.GetGuild}...");
			NpgsqlCommand getGuild = new("SELECT title FROM tags WHERE guild_id=@guildId UNION ALL SELECT title || '*' FROM tag_aliases WHERE guild_id=@guildId", _connection);
			_ = getGuild.Parameters.Add(new("guildId", NpgsqlDbType.Bigint));
			getGuild.Prepare();
			_preparedStatements.Add(StatementType.GetGuild, getGuild);

			_logger.Debug($"Preparing {StatementType.GetUserGuild}...");
			NpgsqlCommand getUserGuild = new("SELECT title FROM tags WHERE guild_id=@guildId AND user_id=@userId UNION ALL SELECT title || '*' FROM tag_aliases WHERE guild_id=@guildId AND user_id=@userId", _connection);
			_ = getUserGuild.Parameters.Add(new("userId", NpgsqlDbType.Bigint));
			_ = getUserGuild.Parameters.Add(new("guildId", NpgsqlDbType.Bigint));
			getUserGuild.Prepare();

			_preparedStatements.Add(StatementType.GetUserGuild, getUserGuild);

			_logger.Debug($"Preparing {StatementType.GetUserOverall}...");
			NpgsqlCommand getUserOverall = new("SELECT title FROM tags WHERE user_id=@userId UNION ALL SELECT title || '*' FROM tag_aliases WHERE user_id=@userId", _connection);
			_ = getUserOverall.Parameters.Add(new("userId", NpgsqlDbType.Bigint));
			getUserOverall.Prepare();
			_preparedStatements.Add(StatementType.GetUserOverall, getUserOverall);

			_logger.Debug($"Preparing {StatementType.GetAuthor}...");
			NpgsqlCommand getAuthor = new("SELECT get_tag_author(@guildId, @tagTitle)", _connection);
			_ = getAuthor.Parameters.Add(new("guildId", NpgsqlDbType.Bigint));
			_ = getAuthor.Parameters.Add(new("tagTitle", NpgsqlDbType.Varchar));
			getAuthor.Prepare();
			_preparedStatements.Add(StatementType.GetAuthor, getAuthor);

			_logger.Debug($"Preparing {StatementType.IsAlias}...");
			NpgsqlCommand isAlias = new("SELECT NOT EXISTS(SELECT 1 FROM tags WHERE guild_id=@guildId AND title=@tagTitle)", _connection);
			_ = isAlias.Parameters.Add(new("guildId", NpgsqlDbType.Bigint));
			_ = isAlias.Parameters.Add(new("tagTitle", NpgsqlDbType.Varchar));
			isAlias.Prepare();
			_preparedStatements.Add(StatementType.IsAlias, isAlias);

			_logger.Debug($"Preparing {StatementType.Exist}...");
			NpgsqlCommand exist = new("SELECT tag_exists(@guildId, @tagTitle)", _connection);
			_ = exist.Parameters.Add(new("guildId", NpgsqlDbType.Bigint));
			_ = exist.Parameters.Add(new("tagTitle", NpgsqlDbType.Varchar));
			exist.Prepare();
			_preparedStatements.Add(StatementType.Exist, exist);

			_logger.Debug($"Preparing {StatementType.Claim}...");
			NpgsqlCommand claim = new("SELECT tag_claim(@guildId, @userId, @tagTitle)", _connection);
			_ = claim.Parameters.Add(new("guildId", NpgsqlDbType.Bigint));
			_ = claim.Parameters.Add(new("userId", NpgsqlDbType.Bigint));
			_ = claim.Parameters.Add(new("tagTitle", NpgsqlDbType.Varchar));
			claim.Prepare();
			_preparedStatements.Add(StatementType.Claim, claim);

			_logger.Debug($"Preparing {StatementType.RealName}...");
			NpgsqlCommand realName = new("SELECT title FROM tags WHERE id=(SELECT id FROM tag_aliases WHERE guild_id=@guildId AND title=@tagTitle)", _connection);
			_ = realName.Parameters.Add(new("guildId", NpgsqlDbType.Bigint));
			_ = realName.Parameters.Add(new("tagTitle", NpgsqlDbType.Varchar));
			realName.Prepare();
			_preparedStatements.Add(StatementType.RealName, realName);

			_logger.Debug("Done preparing commands!");
		}

		public void Dispose()
		{
			_preparedStatements.Clear();
			_connection.Close();
			_connection.Dispose();
			GC.SuppressFinalize(this);
		}

		public void Claim(ulong guildId, string tagTitle, ulong newAuthor) => ExecuteQuery(StatementType.Claim, new List<NpgsqlParameter>() { new NpgsqlParameter("guildId", (long)guildId), new NpgsqlParameter("userId", (long)newAuthor), new NpgsqlParameter("tagTitle", tagTitle) });
		public void Create(ulong guildId, ulong userId, string tagTitle, string content) => ExecuteQuery(StatementType.Create, new List<NpgsqlParameter>() { new NpgsqlParameter("guildId", (long)guildId), new NpgsqlParameter("userId", (long)userId), new NpgsqlParameter("tagTitle", tagTitle), new NpgsqlParameter("content", content) });
		public void CreateAlias(ulong guildId, ulong userId, string tagTitle, string oldTagTitle) => ExecuteQuery(StatementType.CreateAlias, new List<NpgsqlParameter>() { new NpgsqlParameter("guildId", (long)guildId), new NpgsqlParameter("userId", (long)userId), new NpgsqlParameter("tagTitle", tagTitle), new NpgsqlParameter("oldTagTitle", oldTagTitle) });
		public void Delete(ulong guildId, string tagTitle) => ExecuteQuery(StatementType.Delete, new List<NpgsqlParameter>() { new NpgsqlParameter("guildId", (long)guildId), new NpgsqlParameter("tagTitle", tagTitle) });
		public void DeleteAlias(ulong guildId, string tagTitle) => ExecuteQuery(StatementType.DeleteAlias, new List<NpgsqlParameter>() { new NpgsqlParameter("guildId", (long)guildId), new NpgsqlParameter("tagTitle", tagTitle) });
		public void DeleteAllAliases(ulong guildId, string tagTitle) => ExecuteQuery(StatementType.DeleteAllAliases, new List<NpgsqlParameter>() { new NpgsqlParameter("guildId", (long)guildId), new NpgsqlParameter("tagTitle", tagTitle) });
		public void Edit(ulong guildId, string tagTitle, string content) => ExecuteQuery(StatementType.Edit, new List<NpgsqlParameter>() { new NpgsqlParameter("guildId", (long)guildId), new NpgsqlParameter("tagTitle", tagTitle), new NpgsqlParameter("content", content) });
		public string Retrieve(ulong guildId, string tagTitle) => ExecuteQuery(StatementType.Get, new List<NpgsqlParameter>() { new NpgsqlParameter("guildId", (long)guildId), new NpgsqlParameter("tagTitle", tagTitle) }, true)?[0][0];
		public string RealName(ulong guildId, string tagTitle) => ExecuteQuery(StatementType.RealName, new List<NpgsqlParameter>() { new NpgsqlParameter("guildId", (long)guildId), new NpgsqlParameter("tagTitle", tagTitle) }, true)?[0][0];
		public ulong? GetAuthor(ulong guildId, string tagTitle) => ExecuteQuery(StatementType.GetAuthor, new List<NpgsqlParameter>() { new NpgsqlParameter("guildId", (long)guildId), new NpgsqlParameter("tagTitle", tagTitle) }, true)?[0][0];
		public bool? IsAlias(ulong guildId, string tagTitle) => ExecuteQuery(StatementType.IsAlias, new List<NpgsqlParameter>() { new NpgsqlParameter("guildId", (long)guildId), new NpgsqlParameter("tagTitle", tagTitle) }, true)?[0][0];
		public bool Exist(ulong guildId, string tagTitle) => ExecuteQuery(StatementType.Exist, new List<NpgsqlParameter>() { new NpgsqlParameter("guildId", (long)guildId), new NpgsqlParameter("tagTitle", tagTitle) }, true)?[0][0];
		public string[] GetAliases(ulong guildId, string tagTitle) => ExecuteQuery(StatementType.GetAliases, new List<NpgsqlParameter>() { new NpgsqlParameter("guildId", (long)guildId), new NpgsqlParameter("tagTitle", tagTitle) }, true)?[0].ConvertAll<string>(tag => tag.ToString()).ToArray();
		public string[] GetGuild(ulong guildId) => ExecuteQuery(StatementType.GetGuild, new NpgsqlParameter("guildId", (long)guildId), true)?[0].ConvertAll<string>(tag => tag.ToString()).ToArray();
		public string[] GetUser(ulong guildId, ulong userId) => ExecuteQuery(StatementType.GetUserGuild, new List<NpgsqlParameter>() { new NpgsqlParameter("guildId", (long)guildId), new NpgsqlParameter("userId", (long)userId) }, true)?[0].ConvertAll<string>(tag => tag.ToString()).ToArray();
		public string[] GetUser(ulong userId) => ExecuteQuery(StatementType.GetUserOverall, new NpgsqlParameter("userId", (long)userId))?[0].ConvertAll<string>(tag => tag.ToString()).ToArray();
	}
}
