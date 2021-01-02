using System.Collections.Generic;
using System.IO;
using Npgsql;
using NpgsqlTypes;
using Tomoe.Database.Interfaces;
using Tomoe.Utils;
using System;
using System.Linq;
using System.Net.Sockets;

namespace Tomoe.Database.Drivers.PostgresSQL
{
	public class PostgresTags : ITags
	{
		private static readonly Logger Logger = new("Database.PostgresSQL.Tags");
		private readonly NpgsqlConnection Connection;
		private readonly Dictionary<StatementType, NpgsqlCommand> PreparedStatements = new();
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

		/// <inheritdoc cref="ExecuteQuery(StatementType, List{NpgsqlParameter}, bool)" />
		/// <param name="parameter">One <see cref="NpgsqlParameter">NpgsqlParameter</see>, which gets converted into a <see cref="List{T}">List&lt;NpgsqlParameter&gt;</see>.</param>
		private Dictionary<int, List<dynamic>> ExecuteQuery(StatementType command, NpgsqlParameter parameter, bool needsResult = false) => ExecuteQuery(command, new List<NpgsqlParameter> { parameter }, needsResult);

		public PostgresTags(string host, int port, string username, string password, string database_name, SslMode sslMode)
		{
			Connection = new($"Host={host};Port={port};Username={username};Password={password};Database={database_name};SSL Mode={sslMode}");
			Logger.Info("Opening connection to database...");
			try
			{
				Connection.Open();
				Logger.Debug("Creating tags table if it doesn't exist...");
				NpgsqlCommand createTagsTable = new(File.ReadAllText(Path.Join(FileSystem.ProjectRoot, "res/sql/drivers/postgresql/tags_table.sql")), Connection);
				_ = createTagsTable.ExecuteNonQuery();
				createTagsTable.Dispose();

				Logger.Debug("Creating tag_aliases table if it doesn't exist...");
				NpgsqlCommand createAliasTable = new(File.ReadAllText(Path.Join(FileSystem.ProjectRoot, "res/sql/drivers/postgresql/tag_aliases_table.sql")), Connection);
				_ = createAliasTable.ExecuteNonQuery();
				createAliasTable.Dispose();

				Logger.Debug("Creating tag functions if they don't exist...");
				NpgsqlCommand createFunctions = new(File.ReadAllText(Path.Join(FileSystem.ProjectRoot, "res/sql/drivers/postgresql/tag_functions.sql")), Connection);
				_ = createFunctions.ExecuteNonQuery();
				createFunctions.Dispose();
			}
			catch (SocketException error)
			{
				Logger.Critical($"Failed to connect to database. {error.Message}", true);
			}
			Logger.Info("Preparing SQL commands...");
			Logger.Debug($"Preparing {StatementType.Create}...");
			NpgsqlCommand create = new("INSERT INTO tags VALUES(@tagTitle, @guildId, @userId, DEFAULT, DEFAULT, @content)", Connection);
			_ = create.Parameters.Add(new("guildId", NpgsqlDbType.Bigint));
			_ = create.Parameters.Add(new("userId", NpgsqlDbType.Bigint));
			_ = create.Parameters.Add(new("tagTitle", NpgsqlDbType.Varchar));
			_ = create.Parameters.Add(new("content", NpgsqlDbType.Varchar));
			create.Prepare();
			PreparedStatements.Add(StatementType.Create, create);

			Logger.Debug($"Preparing {StatementType.CreateAlias}...");
			NpgsqlCommand createAlias = new("INSERT INTO tag_aliases VALUES(@tagTitle, @guildId, @userId, (SELECT id FROM tags WHERE guild_id=@guildId AND title=@oldTagTitle))", Connection);
			_ = createAlias.Parameters.Add(new("userId", NpgsqlDbType.Bigint));
			_ = createAlias.Parameters.Add(new("guildId", NpgsqlDbType.Bigint));
			_ = createAlias.Parameters.Add(new("tagTitle", NpgsqlDbType.Varchar));
			_ = createAlias.Parameters.Add(new("oldTagTitle", NpgsqlDbType.Varchar));
			createAlias.Prepare();
			PreparedStatements.Add(StatementType.CreateAlias, createAlias);

			Logger.Debug($"Preparing {StatementType.Delete}...");
			NpgsqlCommand delete = new("DELETE FROM tags CASCADE WHERE guild_id=@guildId AND title=@tagTitle", Connection);
			_ = delete.Parameters.Add(new("guildId", NpgsqlDbType.Bigint));
			_ = delete.Parameters.Add(new("tagTitle", NpgsqlDbType.Varchar));
			delete.Prepare();
			PreparedStatements.Add(StatementType.Delete, delete);

			Logger.Debug($"Preparing {StatementType.DeleteAlias}...");
			NpgsqlCommand deleteAlias = new("DELETE FROM tag_aliases WHERE guild_id=@guildId AND title=@tagTitle", Connection);
			_ = deleteAlias.Parameters.Add(new("guildId", NpgsqlDbType.Bigint));
			_ = deleteAlias.Parameters.Add(new("tagTitle", NpgsqlDbType.Varchar));
			deleteAlias.Prepare();
			PreparedStatements.Add(StatementType.DeleteAlias, deleteAlias);

			Logger.Debug($"Preparing {StatementType.DeleteAllAliases}...");
			NpgsqlCommand deleteAllAliases = new("DELETE FROM tag_aliases WHERE id=(SELECT id FROM tags WHERE guild_id=@guildId AND title=@tagTitle)", Connection);
			_ = deleteAllAliases.Parameters.Add(new("guildId", NpgsqlDbType.Bigint));
			_ = deleteAllAliases.Parameters.Add(new("tagTitle", NpgsqlDbType.Varchar));
			deleteAllAliases.Prepare();
			PreparedStatements.Add(StatementType.DeleteAllAliases, deleteAllAliases);

			Logger.Debug($"Preparing {StatementType.Edit}...");
			NpgsqlCommand edit = new("UPDATE tags SET content=@content WHERE guild_id=@guildId AND title=@tagTitle", Connection);
			_ = edit.Parameters.Add(new("content", NpgsqlDbType.Varchar));
			_ = edit.Parameters.Add(new("guildId", NpgsqlDbType.Bigint));
			_ = edit.Parameters.Add(new("tagTitle", NpgsqlDbType.Varchar));
			edit.Prepare();
			PreparedStatements.Add(StatementType.Edit, edit);

			Logger.Debug($"Preparing {StatementType.Get}...");
			NpgsqlCommand get = new("SELECT get_tag_value(@guildId, @tagTitle)", Connection);
			_ = get.Parameters.Add(new("guildId", NpgsqlDbType.Bigint));
			_ = get.Parameters.Add(new("tagTitle", NpgsqlDbType.Varchar));
			get.Prepare();
			PreparedStatements.Add(StatementType.Get, get);

			Logger.Debug($"Preparing {StatementType.GetAliases}...");
			NpgsqlCommand getAliases = new("SELECT title FROM tag_aliases WHERE id=(SELECT id FROM tags WHERE guild_id=@guildId AND title=@tagTitle)", Connection);
			_ = getAliases.Parameters.Add(new("guildId", NpgsqlDbType.Bigint));
			_ = getAliases.Parameters.Add(new("tagTitle", NpgsqlDbType.Varchar));
			getAliases.Prepare();
			PreparedStatements.Add(StatementType.GetAliases, getAliases);

			Logger.Debug($"Preparing {StatementType.GetGuild}...");
			NpgsqlCommand getGuild = new("SELECT title FROM tags WHERE guild_id=@guildId UNION ALL SELECT title || '*' FROM tag_aliases WHERE guild_id=@guildId", Connection);
			_ = getGuild.Parameters.Add(new("guildId", NpgsqlDbType.Bigint));
			getGuild.Prepare();
			PreparedStatements.Add(StatementType.GetGuild, getGuild);

			Logger.Debug($"Preparing {StatementType.GetUserGuild}...");
			NpgsqlCommand getUserGuild = new("SELECT title FROM tags WHERE guild_id=@guildId AND user_id=@userId UNION ALL SELECT title || '*' FROM tag_aliases WHERE guild_id=@guildId AND user_id=@userId", Connection);
			_ = getUserGuild.Parameters.Add(new("userId", NpgsqlDbType.Bigint));
			_ = getUserGuild.Parameters.Add(new("guildId", NpgsqlDbType.Bigint));
			getUserGuild.Prepare();

			PreparedStatements.Add(StatementType.GetUserGuild, getUserGuild);

			Logger.Debug($"Preparing {StatementType.GetUserOverall}...");
			NpgsqlCommand getUserOverall = new("SELECT title FROM tags WHERE user_id=@userId UNION ALL SELECT title || '*' FROM tag_aliases WHERE user_id=@userId", Connection);
			_ = getUserOverall.Parameters.Add(new("userId", NpgsqlDbType.Bigint));
			getUserOverall.Prepare();
			PreparedStatements.Add(StatementType.GetUserOverall, getUserOverall);

			Logger.Debug($"Preparing {StatementType.GetAuthor}...");
			NpgsqlCommand getAuthor = new("SELECT get_tag_author(@guildId, @tagTitle)", Connection);
			_ = getAuthor.Parameters.Add(new("guildId", NpgsqlDbType.Bigint));
			_ = getAuthor.Parameters.Add(new("tagTitle", NpgsqlDbType.Varchar));
			getAuthor.Prepare();
			PreparedStatements.Add(StatementType.GetAuthor, getAuthor);

			Logger.Debug($"Preparing {StatementType.IsAlias}...");
			NpgsqlCommand isAlias = new("SELECT NOT EXISTS(SELECT 1 FROM tags WHERE guild_id=@guildId AND title=@tagTitle)", Connection);
			_ = isAlias.Parameters.Add(new("guildId", NpgsqlDbType.Bigint));
			_ = isAlias.Parameters.Add(new("tagTitle", NpgsqlDbType.Varchar));
			isAlias.Prepare();
			PreparedStatements.Add(StatementType.IsAlias, isAlias);

			Logger.Debug($"Preparing {StatementType.Exist}...");
			NpgsqlCommand exist = new("SELECT tag_exists(@guildId, @tagTitle)", Connection);
			_ = exist.Parameters.Add(new("guildId", NpgsqlDbType.Bigint));
			_ = exist.Parameters.Add(new("tagTitle", NpgsqlDbType.Varchar));
			exist.Prepare();
			PreparedStatements.Add(StatementType.Exist, exist);

			Logger.Debug($"Preparing {StatementType.Claim}...");
			NpgsqlCommand claim = new("SELECT tag_claim(@guildId, @userId, @tagTitle)", Connection);
			_ = claim.Parameters.Add(new("guildId", NpgsqlDbType.Bigint));
			_ = claim.Parameters.Add(new("userId", NpgsqlDbType.Bigint));
			_ = claim.Parameters.Add(new("tagTitle", NpgsqlDbType.Varchar));
			claim.Prepare();
			PreparedStatements.Add(StatementType.Claim, claim);

			Logger.Debug($"Preparing {StatementType.RealName}...");
			NpgsqlCommand realName = new("SELECT title FROM tags WHERE id=(SELECT id FROM tag_aliases WHERE guild_id=@guildId AND title=@tagTitle)", Connection);
			_ = realName.Parameters.Add(new("guildId", NpgsqlDbType.Bigint));
			_ = realName.Parameters.Add(new("tagTitle", NpgsqlDbType.Varchar));
			realName.Prepare();
			PreparedStatements.Add(StatementType.RealName, realName);

			Logger.Debug("Done preparing commands!");
		}

		public void Dispose()
		{
			PreparedStatements.Clear();
			Connection.Close();
			Connection.Dispose();
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
