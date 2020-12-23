using System.Collections.Generic;
using System.IO;
using Npgsql;
using NpgsqlTypes;
using Tomoe.Database.Interfaces;
using Tomoe.Utils;

namespace Tomoe.Database.Drivers.PostgresSQL {
    public class PostgresTags : ITags {
        private static Logger _logger = new Logger("Database.PostgresSQL.Tags");
        private NpgsqlConnection _connection;
        private Dictionary<statementType, NpgsqlCommand> _preparedStatements = new Dictionary<statementType, NpgsqlCommand>();
        private enum statementType {
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

        private Dictionary<int, List<dynamic>> executeQuery(statementType command, List<NpgsqlParameter> parameters, bool needsResult = false) {
            List<string> keyValue = new List<string>();
            foreach (NpgsqlParameter param in parameters) keyValue.Add($"\"{param.ParameterName}: {param.Value}\"");
            _logger.Trace($"Executing prepared statement \"{command.ToString()}\" with parameters: {string.Join(", ", keyValue.ToArray())}");

            NpgsqlCommand statement = _preparedStatements[command];
            Dictionary<string, NpgsqlParameter> sortedParameters = new Dictionary<string, NpgsqlParameter>();
            foreach (NpgsqlParameter param in parameters) sortedParameters.Add(param.ParameterName, param);
            foreach (NpgsqlParameter temp in statement.Parameters) temp.Value = sortedParameters[temp.ParameterName].Value;
            if (needsResult) {
                NpgsqlDataReader reader = statement.ExecuteReaderAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                Dictionary<int, List<dynamic>> values = new Dictionary<int, List<dynamic>>();
                int indexCount = 0;
                while (reader.Read()) {
                    List<dynamic> list = new List<dynamic>();
                    for (int i = 0; i < reader.FieldCount; i++) {
                        if (reader[i].GetType() == typeof(System.DBNull))
                            list.Add(null);
                        else
                            list.Add(reader[i]);
                        _logger.Trace($"Recieved values: {reader[i]?? "null"} on iteration {i}");
                    }
                    values.Add(indexCount, list);
                    indexCount++;
                }
                reader.DisposeAsync().ConfigureAwait(false).GetAwaiter();
                if (values.Count == 0) values = null;
                return values;
            } else {
                statement.ExecuteNonQuery();
                return null;
            }
        }

        /// <inheritdoc cref="Tomoe.Database.Drivers.PostgresSQL.PostgresTags.executeQuery(statementType, List{NpgsqlParameter}, bool)" />
        /// <param name="parameter">One <see cref="Npgsql.NpgsqlParameter">NpgsqlParameter</see>, which gets converted into a <see cref="System.Collections.Generic.List{T}">List&lt;NpgsqlParameter&gt;</see>.</param>
        private Dictionary<int, List<dynamic>> executeQuery(statementType command, NpgsqlParameter parameter, bool needsResult = false) => executeQuery(command, new List<NpgsqlParameter> { parameter }, needsResult);

        public PostgresTags(string host, int port, string username, string password, string database_name, SslMode sslMode) {
            _connection = new NpgsqlConnection($"Host={host};Port={port};Username={username};Password={password};Database={database_name};SSL Mode={sslMode}");
            _logger.Info("Opening connection to database...");
            try {
                _connection.Open();
                NpgsqlCommand createTagsTable = new NpgsqlCommand(File.ReadAllText(Path.Join(FileSystem.ProjectRoot, "res/sql/tags_table.sql")), _connection);
                createTagsTable.ExecuteNonQuery();
                createTagsTable.Dispose();

                NpgsqlCommand createAliasTable = new NpgsqlCommand(File.ReadAllText(Path.Join(FileSystem.ProjectRoot, "res/sql/tag_aliases_table.sql")), _connection);
                createAliasTable.ExecuteNonQuery();
                createAliasTable.Dispose();

                NpgsqlCommand createFunctions = new NpgsqlCommand(File.ReadAllText(Path.Join(FileSystem.ProjectRoot, "res/sql/tag_functions.sql")), _connection);
                createFunctions.ExecuteNonQuery();
                createFunctions.Dispose();
            } catch (System.Net.Sockets.SocketException error) {
                _logger.Critical($"Failed to connect to database. {error.Message}", true);
            }
            _logger.Info("Preparing SQL commands...");
            _logger.Debug($"Preparing {statementType.Create}...");
            NpgsqlCommand create = new NpgsqlCommand("INSERT INTO tags VALUES(@tagTitle, @guildId, @userId, DEFAULT, DEFAULT, @content)", _connection);
            create.Parameters.Add(new NpgsqlParameter("guildId", NpgsqlDbType.Bigint));
            create.Parameters.Add(new NpgsqlParameter("userId", NpgsqlDbType.Bigint));
            create.Parameters.Add(new NpgsqlParameter("tagTitle", NpgsqlDbType.Varchar));
            create.Parameters.Add(new NpgsqlParameter("content", NpgsqlDbType.Varchar));
            create.Prepare();
            _preparedStatements.Add(statementType.Create, create);

            _logger.Debug($"Preparing {statementType.CreateAlias}...");
            NpgsqlCommand createAlias = new NpgsqlCommand("INSERT INTO tag_aliases VALUES(@tagTitle, @guildId, @userId, (SELECT id FROM tags WHERE guild_id=@guildId AND title=@oldTagTitle))", _connection);
            createAlias.Parameters.Add(new NpgsqlParameter("userId", NpgsqlDbType.Bigint));
            createAlias.Parameters.Add(new NpgsqlParameter("guildId", NpgsqlDbType.Bigint));
            createAlias.Parameters.Add(new NpgsqlParameter("tagTitle", NpgsqlDbType.Varchar));
            createAlias.Parameters.Add(new NpgsqlParameter("oldTagTitle", NpgsqlDbType.Varchar));
            createAlias.Prepare();
            _preparedStatements.Add(statementType.CreateAlias, createAlias);

            _logger.Debug($"Preparing {statementType.Delete}...");
            NpgsqlCommand delete = new NpgsqlCommand("DELETE FROM tags CASCADE WHERE guild_id=@guildId AND title=@tagTitle", _connection);
            delete.Parameters.Add(new NpgsqlParameter("guildId", NpgsqlDbType.Bigint));
            delete.Parameters.Add(new NpgsqlParameter("tagTitle", NpgsqlDbType.Varchar));
            delete.Prepare();
            _preparedStatements.Add(statementType.Delete, delete);

            _logger.Debug($"Preparing {statementType.DeleteAlias}...");
            NpgsqlCommand deleteAlias = new NpgsqlCommand("DELETE FROM tag_aliases WHERE guild_id=@guildId AND title=@tagTitle", _connection);
            deleteAlias.Parameters.Add(new NpgsqlParameter("guildId", NpgsqlDbType.Bigint));
            deleteAlias.Parameters.Add(new NpgsqlParameter("tagTitle", NpgsqlDbType.Varchar));
            deleteAlias.Prepare();
            _preparedStatements.Add(statementType.DeleteAlias, deleteAlias);

            _logger.Debug($"Preparing {statementType.DeleteAllAliases}...");
            NpgsqlCommand deleteAllAliases = new NpgsqlCommand("DELETE FROM tag_aliases WHERE id=(SELECT id FROM tags WHERE guild_id=@guildId AND title=@tagTitle)", _connection);
            deleteAllAliases.Parameters.Add(new NpgsqlParameter("guildId", NpgsqlDbType.Bigint));
            deleteAllAliases.Parameters.Add(new NpgsqlParameter("tagTitle", NpgsqlDbType.Varchar));
            deleteAllAliases.Prepare();
            _preparedStatements.Add(statementType.DeleteAllAliases, deleteAllAliases);

            _logger.Debug($"Preparing {statementType.Edit}...");
            NpgsqlCommand edit = new NpgsqlCommand("UPDATE tags SET content=@content WHERE guild_id=@guildId AND title=@tagTitle", _connection);
            edit.Parameters.Add(new NpgsqlParameter("content", NpgsqlDbType.Varchar));
            edit.Parameters.Add(new NpgsqlParameter("guildId", NpgsqlDbType.Bigint));
            edit.Parameters.Add(new NpgsqlParameter("tagTitle", NpgsqlDbType.Varchar));
            edit.Prepare();
            _preparedStatements.Add(statementType.Edit, edit);

            _logger.Debug($"Preparing {statementType.Get}...");
            NpgsqlCommand get = new NpgsqlCommand("SELECT get_tag_value(@guildId, @tagTitle)", _connection);
            get.Parameters.Add(new NpgsqlParameter("guildId", NpgsqlDbType.Bigint));
            get.Parameters.Add(new NpgsqlParameter("tagTitle", NpgsqlDbType.Varchar));
            get.Prepare();
            _preparedStatements.Add(statementType.Get, get);

            _logger.Debug($"Preparing {statementType.GetAliases}...");
            NpgsqlCommand getAliases = new NpgsqlCommand("SELECT title FROM tag_aliases WHERE id=(SELECT id FROM tags WHERE guild_id=@guildId AND title=@tagTitle)", _connection);
            getAliases.Parameters.Add(new NpgsqlParameter("guildId", NpgsqlDbType.Bigint));
            getAliases.Parameters.Add(new NpgsqlParameter("tagTitle", NpgsqlDbType.Varchar));
            getAliases.Prepare();
            _preparedStatements.Add(statementType.GetAliases, getAliases);

            _logger.Debug($"Preparing {statementType.GetGuild}...");
            NpgsqlCommand getGuild = new NpgsqlCommand("SELECT title FROM tags WHERE guild_id=@guildId UNION ALL SELECT title || '*' FROM tag_aliases WHERE guild_id=@guildId", _connection);
            getGuild.Parameters.Add(new NpgsqlParameter("guildId", NpgsqlDbType.Bigint));
            getGuild.Prepare();
            _preparedStatements.Add(statementType.GetGuild, getGuild);

            _logger.Debug($"Preparing {statementType.GetUserGuild}...");
            NpgsqlCommand getUserGuild = new NpgsqlCommand("SELECT title FROM tags WHERE guild_id=@guildId AND user_id=@userId UNION ALL SELECT title || '*' FROM tag_aliases WHERE guild_id=@guildId AND user_id=@userId", _connection);
            getUserGuild.Parameters.Add(new NpgsqlParameter("userId", NpgsqlDbType.Bigint));
            getUserGuild.Parameters.Add(new NpgsqlParameter("guildId", NpgsqlDbType.Bigint));
            getUserGuild.Prepare();

            _preparedStatements.Add(statementType.GetUserGuild, getUserGuild);

            _logger.Debug($"Preparing {statementType.GetUserOverall}...");
            NpgsqlCommand getUserOverall = new NpgsqlCommand("SELECT title FROM tags WHERE user_id=@userId UNION ALL SELECT title || '*' FROM tag_aliases WHERE user_id=@userId", _connection);
            getUserOverall.Parameters.Add(new NpgsqlParameter("userId", NpgsqlDbType.Bigint));
            getUserOverall.Prepare();
            _preparedStatements.Add(statementType.GetUserOverall, getUserOverall);

            _logger.Debug($"Preparing {statementType.GetAuthor}...");
            NpgsqlCommand getAuthor = new NpgsqlCommand("SELECT get_tag_author(@guildId, @tagTitle)", _connection);
            getAuthor.Parameters.Add(new NpgsqlParameter("guildId", NpgsqlDbType.Bigint));
            getAuthor.Parameters.Add(new NpgsqlParameter("tagTitle", NpgsqlDbType.Varchar));
            getAuthor.Prepare();
            _preparedStatements.Add(statementType.GetAuthor, getAuthor);

            _logger.Debug($"Preparing {statementType.IsAlias}...");
            NpgsqlCommand isAlias = new NpgsqlCommand("SELECT NOT EXISTS(SELECT 1 FROM tags WHERE guild_id=@guildId AND title=@tagTitle)", _connection);
            isAlias.Parameters.Add(new NpgsqlParameter("guildId", NpgsqlDbType.Bigint));
            isAlias.Parameters.Add(new NpgsqlParameter("tagTitle", NpgsqlDbType.Varchar));
            isAlias.Prepare();
            _preparedStatements.Add(statementType.IsAlias, isAlias);

            _logger.Debug($"Preparing {statementType.Exist}...");
            NpgsqlCommand exist = new NpgsqlCommand("SELECT tag_exists(@guildId, @tagTitle)", _connection);
            exist.Parameters.Add(new NpgsqlParameter("guildId", NpgsqlDbType.Bigint));
            exist.Parameters.Add(new NpgsqlParameter("tagTitle", NpgsqlDbType.Varchar));
            exist.Prepare();
            _preparedStatements.Add(statementType.Exist, exist);

            _logger.Debug($"Preparing {statementType.Claim}...");
            NpgsqlCommand claim = new NpgsqlCommand("SELECT tag_claim(@guildId, @userId, @tagTitle)", _connection);
            claim.Parameters.Add(new NpgsqlParameter("guildId", NpgsqlDbType.Bigint));
            claim.Parameters.Add(new NpgsqlParameter("userId", NpgsqlDbType.Bigint));
            claim.Parameters.Add(new NpgsqlParameter("tagTitle", NpgsqlDbType.Varchar));
            claim.Prepare();
            _preparedStatements.Add(statementType.Claim, claim);

            _logger.Debug($"Preparing {statementType.RealName}...");
            NpgsqlCommand realName = new NpgsqlCommand("SELECT title FROM tags WHERE id=(SELECT id FROM tag_aliases WHERE guild_id=@guildId AND title=@tagTitle)", _connection);
            realName.Parameters.Add(new NpgsqlParameter("guildId", NpgsqlDbType.Bigint));
            realName.Parameters.Add(new NpgsqlParameter("tagTitle", NpgsqlDbType.Varchar));
            realName.Prepare();
            _preparedStatements.Add(statementType.RealName, realName);

            _logger.Debug("Done preparing commands!");
        }

        public void Claim(ulong guildId, string tagTitle, ulong newAuthor) => executeQuery(statementType.Claim, new List<NpgsqlParameter>() { new NpgsqlParameter("guildId", (long) guildId), new NpgsqlParameter("userId", (long) newAuthor), new NpgsqlParameter("tagTitle", tagTitle) });
        public void Create(ulong guildId, ulong userId, string tagTitle, string content) => executeQuery(statementType.Create, new List<NpgsqlParameter>() { new NpgsqlParameter("guildId", (long) guildId), new NpgsqlParameter("userId", (long) userId), new NpgsqlParameter("tagTitle", tagTitle), new NpgsqlParameter("content", content) });
        public void CreateAlias(ulong guildId, ulong userId, string tagTitle, string oldTagTitle) => executeQuery(statementType.CreateAlias, new List<NpgsqlParameter>() { new NpgsqlParameter("guildId", (long) guildId), new NpgsqlParameter("userId", (long) userId), new NpgsqlParameter("tagTitle", tagTitle), new NpgsqlParameter("oldTagTitle", oldTagTitle) });
        public void Delete(ulong guildId, string tagTitle) => executeQuery(statementType.Delete, new List<NpgsqlParameter>() { new NpgsqlParameter("guildId", (long) guildId), new NpgsqlParameter("tagTitle", tagTitle) });
        public void DeleteAlias(ulong guildId, string tagTitle) => executeQuery(statementType.DeleteAlias, new List<NpgsqlParameter>() { new NpgsqlParameter("guildId", (long) guildId), new NpgsqlParameter("tagTitle", tagTitle) });
        public void DeleteAllAliases(ulong guildId, string tagTitle) => executeQuery(statementType.DeleteAllAliases, new List<NpgsqlParameter>() { new NpgsqlParameter("guildId", (long) guildId), new NpgsqlParameter("tagTitle", tagTitle) });
        public void Edit(ulong guildId, string tagTitle, string content) => executeQuery(statementType.Edit, new List<NpgsqlParameter>() { new NpgsqlParameter("guildId", (long) guildId), new NpgsqlParameter("tagTitle", tagTitle), new NpgsqlParameter("content", content) });
        public string Get(ulong guildId, string tagTitle) => (string?) executeQuery(statementType.Get, new List<NpgsqlParameter>() { new NpgsqlParameter("guildId", (long) guildId), new NpgsqlParameter("tagTitle", tagTitle) }, true) [0][0];
        public string RealName(ulong guildId, string tagTitle) => (string) executeQuery(statementType.RealName, new List<NpgsqlParameter>() { new NpgsqlParameter("guildId", (long) guildId), new NpgsqlParameter("tagTitle", tagTitle) }, true) [0][0];
        public ulong? GetAuthor(ulong guildId, string tagTitle) => (ulong?) executeQuery(statementType.GetAuthor, new List<NpgsqlParameter>() { new NpgsqlParameter("guildId", (long) guildId), new NpgsqlParameter("tagTitle", tagTitle) }, true) [0][0];
        public bool? IsAlias(ulong guildId, string tagTitle) => (bool?) executeQuery(statementType.IsAlias, new List<NpgsqlParameter>() { new NpgsqlParameter("guildId", (long) guildId), new NpgsqlParameter("tagTitle", tagTitle) }, true) [0][0];
        public bool Exist(ulong guildId, string tagTitle) => (bool) executeQuery(statementType.Exist, new List<NpgsqlParameter>() { new NpgsqlParameter("guildId", (long) guildId), new NpgsqlParameter("tagTitle", tagTitle) }, true) [0][0];
        public string[] GetAliases(ulong guildId, string tagTitle) => executeQuery(statementType.GetAliases, new List<NpgsqlParameter>() { new NpgsqlParameter("guildId", (long) guildId), new NpgsqlParameter("tagTitle", tagTitle) }, true) [0].ConvertAll<string>(tag => tag.ToString()).ToArray();
        public string[] GetGuild(ulong guildId) => executeQuery(statementType.GetGuild, new NpgsqlParameter("guildId", (long) guildId), true) [0].ConvertAll<string>(tag => tag.ToString()).ToArray();
        public string[] GetUser(ulong guildId, ulong userId) => executeQuery(statementType.GetUserGuild, new List<NpgsqlParameter>() { new NpgsqlParameter("guildId", (long) guildId), new NpgsqlParameter("userId", (long) userId) }, true) [0].ConvertAll<string>(tag => tag.ToString()).ToArray();
        public string[] GetUser(ulong userId) => executeQuery(statementType.GetUserOverall, new NpgsqlParameter("userId", (long) userId)) [0].ConvertAll<string>(tag => tag.ToString()).ToArray();
    }
}