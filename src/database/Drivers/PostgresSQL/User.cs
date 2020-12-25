using System.Collections.Generic;
using System.IO;
using System.Linq;
using Npgsql;
using NpgsqlTypes;
using Tomoe.Database.Interfaces;
using Tomoe.Utils;

namespace Tomoe.Database.Drivers.PostgresSQL {
    public class PostgresUser : IUser {
        private readonly static Logger _logger = new Logger("Database.PostgresSQL.User");
        private readonly NpgsqlConnection _connection;
        private readonly Dictionary<statementType, NpgsqlCommand> _preparedStatements = new Dictionary<statementType, NpgsqlCommand>();
        private enum statementType {
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

        private Dictionary<int, List<dynamic>> executeQuery(statementType command, List<NpgsqlParameter> parameters, bool needsResult = false) {
            List<string> keyValue = new List<string>();
            foreach (NpgsqlParameter param in parameters) keyValue.Add($"\"{param.ParameterName}: {param.Value}\"");
            _logger.Trace($"Executing prepared statement \"{command}\" with parameters: {string.Join(", ", keyValue.ToArray())}");

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

        /// <inheritdoc cref="executeQuery(statementType, List{NpgsqlParameter}, bool)" />
        /// <param name="parameter">One <see cref="NpgsqlParameter">NpgsqlParameter</see>, which gets converted into a <see cref="List{T}">List&lt;NpgsqlParameter&gt;</see>.</param>
        private Dictionary<int, List<dynamic>> executeQuery(statementType command, NpgsqlParameter parameter, bool needsResult = false) => executeQuery(command, new List<NpgsqlParameter> { parameter }, needsResult);

        public PostgresUser(string host, int port, string username, string password, string database_name, SslMode sslMode) {
            _connection = new NpgsqlConnection($"Host={host};Port={port};Username={username};Password={password};Database={database_name};SSL Mode={sslMode}");
            _logger.Info("Opening connection to database...");
            try {
                _connection.Open();
                NpgsqlCommand createGuildCacheTable = new NpgsqlCommand(File.ReadAllText(Path.Join(FileSystem.ProjectRoot, "res/sql/guild_cache_table.sql")), _connection);
                createGuildCacheTable.ExecuteNonQuery();
                createGuildCacheTable.Dispose();
            } catch (System.Net.Sockets.SocketException error) {
                _logger.Critical($"Failed to connect to database. {error.Message}", true);
            }
            _logger.Info("Preparing SQL commands...");
            _logger.Debug($"Preparing {statementType.InsertUser}...");
            NpgsqlCommand insertUser = new NpgsqlCommand("INSERT INTO guild_cache(guild_id, user_id) VALUES(@guildId, @userId)", _connection);
            insertUser.Parameters.Add(new NpgsqlParameter("guildId", NpgsqlDbType.Bigint));
            insertUser.Parameters.Add(new NpgsqlParameter("userId", NpgsqlDbType.Bigint));
            insertUser.Prepare();
            _preparedStatements.Add(statementType.InsertUser, insertUser);

            _logger.Debug($"Preparing {statementType.GetRoles}...");
            NpgsqlCommand getRoles = new NpgsqlCommand("SELECT role_ids FROM guild_cache WHERE user_id=@userId AND guild_id=@guildId", _connection);
            getRoles.Parameters.Add(new NpgsqlParameter("userId", NpgsqlDbType.Bigint));
            getRoles.Parameters.Add(new NpgsqlParameter("guildId", NpgsqlDbType.Bigint));
            getRoles.Prepare();
            _preparedStatements.Add(statementType.GetRoles, getRoles);

            _logger.Debug($"Preparing {statementType.AddRole}...");
            NpgsqlCommand addRole = new NpgsqlCommand("UPDATE guild_cache SET role_ids=array_append(role_ids, @roleId) WHERE user_id=@userId AND guild_id=@guildId", _connection);
            addRole.Parameters.Add(new NpgsqlParameter("roleId", NpgsqlDbType.Bigint));
            addRole.Parameters.Add(new NpgsqlParameter("userId", NpgsqlDbType.Bigint));
            addRole.Parameters.Add(new NpgsqlParameter("guildId", NpgsqlDbType.Bigint));
            addRole.Prepare();
            _preparedStatements.Add(statementType.AddRole, addRole);

            _logger.Debug($"Preparing {statementType.RemoveRole}...");
            NpgsqlCommand removeRole = new NpgsqlCommand("UPDATE guild_cache SET role_ids=array_remove(role_ids, @roleId) WHERE user_id=@userId AND guild_id=@guildId", _connection);
            removeRole.Parameters.Add(new NpgsqlParameter("roleId", NpgsqlDbType.Bigint));
            removeRole.Parameters.Add(new NpgsqlParameter("userId", NpgsqlDbType.Bigint));
            removeRole.Parameters.Add(new NpgsqlParameter("guildId", NpgsqlDbType.Bigint));
            removeRole.Prepare();
            _preparedStatements.Add(statementType.RemoveRole, removeRole);

            _logger.Debug($"Preparing {statementType.SetRoles}...");
            NpgsqlCommand setRoles = new NpgsqlCommand("UPDATE guild_cache SET role_ids=@roleIds WHERE user_id=@userId AND guild_id=@guildId", _connection);
            setRoles.Parameters.Add(new NpgsqlParameter("roleIds", (NpgsqlDbType.Array | NpgsqlDbType.Bigint)));
            setRoles.Parameters.Add(new NpgsqlParameter("userId", NpgsqlDbType.Bigint));
            setRoles.Parameters.Add(new NpgsqlParameter("guildId", NpgsqlDbType.Bigint));
            setRoles.Prepare();
            _preparedStatements.Add(statementType.SetRoles, setRoles);

            _logger.Debug($"Preparing {statementType.GetIsMuted}...");
            NpgsqlCommand getIsMuted = new NpgsqlCommand("SELECT muted FROM guild_cache WHERE user_id=@userId AND guild_id=@guildId", _connection);
            getIsMuted.Parameters.Add(new NpgsqlParameter("userId", NpgsqlDbType.Bigint));
            getIsMuted.Parameters.Add(new NpgsqlParameter("guildId", NpgsqlDbType.Bigint));
            getIsMuted.Prepare();
            _preparedStatements.Add(statementType.GetIsMuted, getIsMuted);

            _logger.Debug($"Preparing {statementType.SetIsMuted}...");
            NpgsqlCommand setIsMuted = new NpgsqlCommand("UPDATE guild_cache SET muted=@isMuted WHERE user_id=@userId AND guild_id=@guildId", _connection);
            setIsMuted.Parameters.Add(new NpgsqlParameter("isMuted", NpgsqlDbType.Boolean));
            setIsMuted.Parameters.Add(new NpgsqlParameter("userId", NpgsqlDbType.Bigint));
            setIsMuted.Parameters.Add(new NpgsqlParameter("guildId", NpgsqlDbType.Bigint));
            setIsMuted.Prepare();
            _preparedStatements.Add(statementType.SetIsMuted, setIsMuted);

            _logger.Debug($"Preparing {statementType.GetIsNoMemed}...");
            NpgsqlCommand getIsNoMemed = new NpgsqlCommand("SELECT no_memed FROM guild_cache WHERE user_id=@userId AND guild_id=@guildId", _connection);
            getIsNoMemed.Parameters.Add(new NpgsqlParameter("userId", NpgsqlDbType.Bigint));
            getIsNoMemed.Parameters.Add(new NpgsqlParameter("guildId", NpgsqlDbType.Bigint));
            getIsNoMemed.Prepare();
            _preparedStatements.Add(statementType.GetIsNoMemed, getIsNoMemed);

            _logger.Debug($"Preparing {statementType.SetIsNoMemed}...");
            NpgsqlCommand setIsNoMemed = new NpgsqlCommand("UPDATE guild_cache SET no_memed=@isNoMemed WHERE user_id=@userId AND guild_id=@guildId", _connection);
            setIsNoMemed.Parameters.Add(new NpgsqlParameter("isNoMemed", NpgsqlDbType.Boolean));
            setIsNoMemed.Parameters.Add(new NpgsqlParameter("userId", NpgsqlDbType.Bigint));
            setIsNoMemed.Parameters.Add(new NpgsqlParameter("guildId", NpgsqlDbType.Bigint));
            setIsNoMemed.Prepare();
            _preparedStatements.Add(statementType.SetIsNoMemed, setIsNoMemed);

            _logger.Debug($"Preparing {statementType.GetIsNoVC}...");
            NpgsqlCommand getIsNoVC = new NpgsqlCommand("SELECT no_voicechat FROM guild_cache WHERE user_id=@userId AND guild_id=@guildId", _connection);
            getIsNoVC.Parameters.Add(new NpgsqlParameter("userId", NpgsqlDbType.Bigint));
            getIsNoVC.Parameters.Add(new NpgsqlParameter("guildId", NpgsqlDbType.Bigint));
            getIsNoVC.Prepare();
            _preparedStatements.Add(statementType.GetIsNoVC, getIsNoVC);

            _logger.Debug($"Preparing {statementType.SetIsNoVC}...");
            NpgsqlCommand setIsNoVC = new NpgsqlCommand("UPDATE guild_cache SET no_voicechat=@isNoVC WHERE user_id=@userId AND guild_id=@guildId", _connection);
            setIsNoVC.Parameters.Add(new NpgsqlParameter("isNoVC", NpgsqlDbType.Boolean));
            setIsNoVC.Parameters.Add(new NpgsqlParameter("userId", NpgsqlDbType.Bigint));
            setIsNoVC.Parameters.Add(new NpgsqlParameter("guildId", NpgsqlDbType.Bigint));
            setIsNoVC.Prepare();
            _preparedStatements.Add(statementType.SetIsNoVC, setIsNoVC);
            _logger.Debug("Done preparing commands!");
        }

        public void InsertUser(ulong guildId, ulong userId) => executeQuery(statementType.InsertUser, new List<NpgsqlParameter>() { new NpgsqlParameter("guildId", (long) guildId), new NpgsqlParameter("userId", (long) userId) });
        public ulong[] GetRoles(ulong guildId, ulong userId) => executeQuery(statementType.GetRoles, new List<NpgsqlParameter>() { new NpgsqlParameter("guildId", (long) guildId), new NpgsqlParameter("userId", (long) userId) }, true) ? [0].ConvertAll<ulong>(roleId => ulong.Parse(roleId)).ToArray() ?? null;
        public void AddRole(ulong guildId, ulong userId, ulong roleId) => executeQuery(statementType.AddRole, new List<NpgsqlParameter>() { new NpgsqlParameter("guildId", (long) guildId), new NpgsqlParameter("userId", (long) userId), new NpgsqlParameter("roleId", (long) roleId) });
        public void RemoveRole(ulong guildId, ulong userId, ulong roleId) => executeQuery(statementType.RemoveRole, new List<NpgsqlParameter>() { new NpgsqlParameter("guildId", (long) guildId), new NpgsqlParameter("userId", (long) userId), new NpgsqlParameter("roleId", (long) roleId) });
        public void SetRoles(ulong guildId, ulong userId, ulong[] roleIds) => executeQuery(statementType.SetRoles, new List<NpgsqlParameter>() { new NpgsqlParameter("guildId", (long) guildId), new NpgsqlParameter("userId", (long) userId), new NpgsqlParameter("roleId", roleIds.Select((role) => long.Parse(role.ToString()))) });

        public void AddStrike(ulong guildId, ulong userId) => executeQuery(statementType.AddStrike, new List<NpgsqlParameter>() { new NpgsqlParameter("guildId", (long) guildId), new NpgsqlParameter("userId", (long) userId) });
        public void RemoveStrike(ulong guildId, ulong userId) => executeQuery(statementType.RemoveStrike, new List<NpgsqlParameter>() { new NpgsqlParameter("guildId", (long) guildId), new NpgsqlParameter("userId", (long) userId) });
        public int GetStrikeCount(ulong guildId, ulong userId) => executeQuery(statementType.GetStrikeCount, new List<NpgsqlParameter>() { new NpgsqlParameter("guildId", (long) guildId), new NpgsqlParameter("userId", (long) userId) }, true) ? [0] ? [0] ?? null;
        public void SetStrikeCount(ulong guildId, ulong userId, int strikeCount) => executeQuery(statementType.SetStrikeCount, new List<NpgsqlParameter>() { new NpgsqlParameter("guildId", (long) guildId), new NpgsqlParameter("userId", (long) userId), new NpgsqlParameter("strikeCount", (short) strikeCount) });

        public bool IsMuted(ulong guildId, ulong userId) => executeQuery(statementType.GetIsMuted, new List<NpgsqlParameter>() { new NpgsqlParameter("guildId", (long) guildId), new NpgsqlParameter("userId", (long) userId) }, true) ? [0] ? [0] ?? null;
        public void IsMuted(ulong guildId, ulong userId, bool isMuted) => executeQuery(statementType.SetIsMuted, new List<NpgsqlParameter>() { new NpgsqlParameter("guildId", (long) guildId), new NpgsqlParameter("userId", (long) userId), new NpgsqlParameter("isMuted", isMuted) });

        public bool IsNoMemed(ulong guildId, ulong userId) => executeQuery(statementType.GetIsNoMemed, new List<NpgsqlParameter>() { new NpgsqlParameter("guildId", (long) guildId), new NpgsqlParameter("userId", (long) userId) }, true) ? [0] ? [0] ?? null;
        public void IsNoMemed(ulong guildId, ulong userId, bool isNoMemed) => executeQuery(statementType.SetIsNoMemed, new List<NpgsqlParameter>() { new NpgsqlParameter("guildId", (long) guildId), new NpgsqlParameter("userId", (long) userId), new NpgsqlParameter("isNoMemed", isNoMemed) });

        public bool IsNoVC(ulong guildId, ulong userId) => executeQuery(statementType.GetIsNoVC, new List<NpgsqlParameter>() { new NpgsqlParameter("guildId", (long) guildId), new NpgsqlParameter("userId", (long) userId) }, true) ? [0] ? [0] ?? null;
        public void IsNoVC(ulong guildId, ulong userId, bool isNoVC) => executeQuery(statementType.SetIsNoVC, new List<NpgsqlParameter>() { new NpgsqlParameter("guildId", (long) guildId), new NpgsqlParameter("userId", (long) userId), new NpgsqlParameter("isNoVC", isNoVC) });
    }
}