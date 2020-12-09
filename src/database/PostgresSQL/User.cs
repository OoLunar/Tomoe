using System.Collections.Generic;
using System.Linq;
using Npgsql;
using NpgsqlTypes;
using Tomoe.Database.Interfaces;
using Tomoe.Utils;

namespace Tomoe.Database.Drivers.PostgresSQL {
    public class PostgresUser : IUser {
        private static Logger _logger = new Logger("Database/PostgresSQL/User");
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

        private static NpgsqlConnection _connection = new NpgsqlConnection($"Host={Program.Config.Host};Port={Program.Config.Port};Username={Program.Config.Username};Password={Program.Config.Password};Database={Program.Config.DBName};SSL Mode={Program.Config.SslMode}");
        private static Dictionary<statementType, NpgsqlCommand> _preparedStatements = new Dictionary<statementType, NpgsqlCommand>();

        /// <summary>Executes an SQL query. Returns 0 on success, returns null on an empty value, returns the value when there's one value, returns a list when more than one values.</summary>
        private dynamic executeQuery(statementType command, List<NpgsqlParameter> parameters, bool needResult = false) {
            _logger.Trace($"Executing {command.ToString()}");
            NpgsqlCommand statement = _preparedStatements[command];
            Dictionary<string, NpgsqlParameter> sortedParameters = new Dictionary<string, NpgsqlParameter>();
            foreach (NpgsqlParameter param in parameters) sortedParameters.Add(param.ParameterName, param);
            foreach (NpgsqlParameter temp in statement.Parameters) temp.Value = sortedParameters[temp.ParameterName].Value;
            if (needResult) {
                NpgsqlDataReader reader = statement.ExecuteReader();
                List<dynamic> values = new List<dynamic>();
                for (int i = 0; reader.Read(); i++) values.Add(reader.GetValue(i));
                reader.DisposeAsync().ConfigureAwait(false).GetAwaiter();
                if (values.Count == 0) return null;
                if (values.Count == 1) return values[0];
                else return values;
            } else {
                statement.ExecuteNonQueryAsync();
                return null;
            }
        }

        /// <inheritdoc cref="Tomoe.Database.PostgresSQL.executeQuery(PreparedStatments.IndexedCommands, List{NpgsqlParameter}, bool)" />
        private object executeQuery(statementType command, NpgsqlParameter parameters, bool needResult = false) => executeQuery(command, new List<NpgsqlParameter> { parameters }, needResult);

        public PostgresUser() {
            _logger.Info("Opening connection to database...");
            try {
                _connection.Open();
            } catch (System.Net.Sockets.SocketException error) {
                _logger.Critical($"Failed to connect to database. {error.Message}", true);
            }
            _logger.Info("Preparing SQL commands...");
            _logger.Trace($"Preparing {statementType.InsertUser}...");
            NpgsqlCommand insertUser = new NpgsqlCommand("INSERT INTO guild_cache(guild_id, user_id) VALUES(@guildId, @userId)", _connection);
            insertUser.Parameters.Add(new NpgsqlParameter("guildId", NpgsqlDbType.Bigint));
            insertUser.Parameters.Add(new NpgsqlParameter("userId", NpgsqlDbType.Bigint));
            insertUser.Prepare();
            _preparedStatements.Add(statementType.InsertUser, insertUser);

            _logger.Trace($"Preparing {statementType.GetRoles}...");
            NpgsqlCommand getRoles = new NpgsqlCommand("SELECT role_ids FROM guild_cache WHERE user_id=@userId AND guild_id=@guildId", _connection);
            getRoles.Parameters.Add(new NpgsqlParameter("userId", NpgsqlDbType.Bigint));
            getRoles.Parameters.Add(new NpgsqlParameter("guildId", NpgsqlDbType.Bigint));
            getRoles.Prepare();
            _preparedStatements.Add(statementType.GetRoles, getRoles);

            _logger.Trace($"Preparing {statementType.AddRole}...");
            NpgsqlCommand addRole = new NpgsqlCommand("UPDATE guild_cache SET role_ids=array_append(role_ids, @roleId) WHERE user_id=@userId AND guild_id=@guildId", _connection);
            addRole.Parameters.Add(new NpgsqlParameter("roleId", NpgsqlDbType.Bigint));
            addRole.Parameters.Add(new NpgsqlParameter("userId", NpgsqlDbType.Bigint));
            addRole.Parameters.Add(new NpgsqlParameter("guildId", NpgsqlDbType.Bigint));
            addRole.Prepare();
            _preparedStatements.Add(statementType.AddRole, addRole);

            _logger.Trace($"Preparing {statementType.RemoveRole}...");
            NpgsqlCommand removeRole = new NpgsqlCommand("UPDATE guild_cache SET role_ids=array_remove(role_ids, @roleId) WHERE user_id=@userId AND guild_id=@guildId", _connection);
            removeRole.Parameters.Add(new NpgsqlParameter("roleId", NpgsqlDbType.Bigint));
            removeRole.Parameters.Add(new NpgsqlParameter("userId", NpgsqlDbType.Bigint));
            removeRole.Parameters.Add(new NpgsqlParameter("guildId", NpgsqlDbType.Bigint));
            removeRole.Prepare();
            _preparedStatements.Add(statementType.RemoveRole, removeRole);

            _logger.Trace($"Preparing {statementType.SetRoles}...");
            NpgsqlCommand setRoles = new NpgsqlCommand("UPDATE guild_cache SET role_ids=@roleIds WHERE user_id=@userId AND guild_id=@guildId", _connection);
            setRoles.Parameters.Add(new NpgsqlParameter("roleIds", (NpgsqlDbType.Array | NpgsqlDbType.Bigint)));
            setRoles.Parameters.Add(new NpgsqlParameter("userId", NpgsqlDbType.Bigint));
            setRoles.Parameters.Add(new NpgsqlParameter("guildId", NpgsqlDbType.Bigint));
            setRoles.Prepare();
            _preparedStatements.Add(statementType.SetRoles, setRoles);

            _logger.Trace($"Preparing {statementType.AddStrike}...");
            NpgsqlCommand addStrike = new NpgsqlCommand("UPDATE guild_cache SET strikes=strikes + 1 WHERE user_id=@userId AND guild_id=@guildId", _connection);
            addStrike.Parameters.Add(new NpgsqlParameter("userId", NpgsqlDbType.Bigint));
            addStrike.Parameters.Add(new NpgsqlParameter("guildId", NpgsqlDbType.Bigint));
            addStrike.Prepare();
            _preparedStatements.Add(statementType.AddStrike, addStrike);

            _logger.Trace($"Preparing {statementType.RemoveStrike}...");
            NpgsqlCommand removeStrike = new NpgsqlCommand("UPDATE guild_cache SET strikes=strikes - 1 WHERE user_id=@userId AND guild_id=@guildId", _connection);
            removeStrike.Parameters.Add(new NpgsqlParameter("userId", NpgsqlDbType.Bigint));
            removeStrike.Parameters.Add(new NpgsqlParameter("guildId", NpgsqlDbType.Bigint));
            removeStrike.Prepare();
            _preparedStatements.Add(statementType.RemoveStrike, removeStrike);

            _logger.Trace($"Preparing {statementType.GetStrikeCount}...");
            NpgsqlCommand getStrikes = new NpgsqlCommand("SELECT strikes FROM guild_cache WHERE user_id=@userId AND guild_id=@guildId", _connection);
            getStrikes.Parameters.Add(new NpgsqlParameter("userId", NpgsqlDbType.Bigint));
            getStrikes.Parameters.Add(new NpgsqlParameter("guildId", NpgsqlDbType.Bigint));
            getStrikes.Prepare();
            _preparedStatements.Add(statementType.GetStrikeCount, getStrikes);

            _logger.Trace($"Preparing {statementType.SetStrikeCount}...");
            NpgsqlCommand setStrikes = new NpgsqlCommand("UPDATE guild_cache SET strikes=@strikeCount WHERE user_id=@userId AND guild_id=@guildId", _connection);
            setStrikes.Parameters.Add(new NpgsqlParameter("strikeCount", NpgsqlDbType.Smallint));
            setStrikes.Parameters.Add(new NpgsqlParameter("userId", NpgsqlDbType.Bigint));
            setStrikes.Parameters.Add(new NpgsqlParameter("guildId", NpgsqlDbType.Bigint));
            setStrikes.Prepare();
            _preparedStatements.Add(statementType.SetStrikeCount, setStrikes);

            _logger.Trace($"Preparing {statementType.GetIsMuted}...");
            NpgsqlCommand getIsMuted = new NpgsqlCommand("SELECT muted FROM guild_cache WHERE user_id=@userId AND guild_id=@guildId", _connection);
            getIsMuted.Parameters.Add(new NpgsqlParameter("userId", NpgsqlDbType.Bigint));
            getIsMuted.Parameters.Add(new NpgsqlParameter("guildId", NpgsqlDbType.Bigint));
            getIsMuted.Prepare();
            _preparedStatements.Add(statementType.GetIsMuted, getIsMuted);

            _logger.Trace($"Preparing {statementType.SetIsMuted}...");
            NpgsqlCommand setIsMuted = new NpgsqlCommand("UPDATE guild_cache SET muted=@isMuted WHERE user_id=@userId AND guild_id=@guildId", _connection);
            setIsMuted.Parameters.Add(new NpgsqlParameter("isMuted", NpgsqlDbType.Boolean));
            setIsMuted.Parameters.Add(new NpgsqlParameter("userId", NpgsqlDbType.Bigint));
            setIsMuted.Parameters.Add(new NpgsqlParameter("guildId", NpgsqlDbType.Bigint));
            setIsMuted.Prepare();
            _preparedStatements.Add(statementType.SetIsMuted, setIsMuted);

            _logger.Trace($"Preparing {statementType.GetIsNoMemed}...");
            NpgsqlCommand getIsNoMemed = new NpgsqlCommand("SELECT no_memed FROM guild_cache WHERE user_id=@userId AND guild_id=@guildId", _connection);
            getIsNoMemed.Parameters.Add(new NpgsqlParameter("userId", NpgsqlDbType.Bigint));
            getIsNoMemed.Parameters.Add(new NpgsqlParameter("guildId", NpgsqlDbType.Bigint));
            getIsNoMemed.Prepare();
            _preparedStatements.Add(statementType.GetIsNoMemed, getIsNoMemed);

            _logger.Trace($"Preparing {statementType.SetIsNoMemed}...");
            NpgsqlCommand setIsNoMemed = new NpgsqlCommand("UPDATE guild_cache SET no_memed=@isNoMemed WHERE user_id=@userId AND guild_id=@guildId", _connection);
            setIsNoMemed.Parameters.Add(new NpgsqlParameter("isNoMemed", NpgsqlDbType.Boolean));
            setIsNoMemed.Parameters.Add(new NpgsqlParameter("userId", NpgsqlDbType.Bigint));
            setIsNoMemed.Parameters.Add(new NpgsqlParameter("guildId", NpgsqlDbType.Bigint));
            setIsNoMemed.Prepare();
            _preparedStatements.Add(statementType.SetIsNoMemed, setIsNoMemed);

            _logger.Trace($"Preparing {statementType.GetIsNoVC}...");
            NpgsqlCommand getIsNoVC = new NpgsqlCommand("SELECT no_voicechat FROM guild_cache WHERE user_id=@userId AND guild_id=@guildId", _connection);
            getIsNoVC.Parameters.Add(new NpgsqlParameter("userId", NpgsqlDbType.Bigint));
            getIsNoVC.Parameters.Add(new NpgsqlParameter("guildId", NpgsqlDbType.Bigint));
            getIsNoVC.Prepare();
            _preparedStatements.Add(statementType.GetIsNoVC, getIsNoVC);

            _logger.Trace($"Preparing {statementType.SetIsNoVC}...");
            NpgsqlCommand setIsNoVC = new NpgsqlCommand("UPDATE guild_cache SET no_voicechat=@isNoVC WHERE user_id=@userId AND guild_id=@guildId", _connection);
            setIsNoVC.Parameters.Add(new NpgsqlParameter("isNoVC", NpgsqlDbType.Boolean));
            setIsNoVC.Parameters.Add(new NpgsqlParameter("userId", NpgsqlDbType.Bigint));
            setIsNoVC.Parameters.Add(new NpgsqlParameter("guildId", NpgsqlDbType.Bigint));
            setIsNoVC.Prepare();
            _preparedStatements.Add(statementType.SetIsNoVC, setIsNoVC);
            _logger.Debug("Done preparing commands!");
        }

        public void InsertUser(ulong guildId, ulong userId) => executeQuery(statementType.InsertUser, new List<NpgsqlParameter>() { new NpgsqlParameter("guildId", (long) guildId), new NpgsqlParameter("userId", (long) userId) });
        public ulong[] GetRoles(ulong guildId, ulong userId) => (ulong[]) executeQuery(statementType.GetRoles, new List<NpgsqlParameter>() { new NpgsqlParameter("guildId", (long) guildId), new NpgsqlParameter("userId", (long) userId) }, true);
        public void AddRole(ulong guildId, ulong userId, ulong roleId) => executeQuery(statementType.AddRole, new List<NpgsqlParameter>() { new NpgsqlParameter("guildId", (long) guildId), new NpgsqlParameter("userId", (long) userId), new NpgsqlParameter("roleId", (long) roleId) });
        public void RemoveRole(ulong guildId, ulong userId, ulong roleId) => executeQuery(statementType.RemoveRole, new List<NpgsqlParameter>() { new NpgsqlParameter("guildId", (long) guildId), new NpgsqlParameter("userId", (long) userId), new NpgsqlParameter("roleId", (long) roleId) });
        public void SetRoles(ulong guildId, ulong userId, ulong[] roleIds) => executeQuery(statementType.SetRoles, new List<NpgsqlParameter>() { new NpgsqlParameter("guildId", (long) guildId), new NpgsqlParameter("userId", (long) userId), new NpgsqlParameter("roleId", roleIds.Select((role) => long.Parse(role.ToString()))) });

        public void AddStrike(ulong guildId, ulong userId) => executeQuery(statementType.AddStrike, new List<NpgsqlParameter>() { new NpgsqlParameter("guildId", (long) guildId), new NpgsqlParameter("userId", (long) userId) });
        public void RemoveStrike(ulong guildId, ulong userId) => executeQuery(statementType.RemoveStrike, new List<NpgsqlParameter>() { new NpgsqlParameter("guildId", (long) guildId), new NpgsqlParameter("userId", (long) userId) });
        public int GetStrikeCount(ulong guildId, ulong userId) => (int) executeQuery(statementType.GetStrikeCount, new List<NpgsqlParameter>() { new NpgsqlParameter("guildId", (long) guildId), new NpgsqlParameter("userId", (long) userId) }, true);
        public void SetStrikeCount(ulong guildId, ulong userId, int strikeCount) => executeQuery(statementType.SetStrikeCount, new List<NpgsqlParameter>() { new NpgsqlParameter("guildId", (long) guildId), new NpgsqlParameter("userId", (long) userId), new NpgsqlParameter("strikeCount", (short) strikeCount) });

        public bool IsMuted(ulong guildId, ulong userId) => (bool) executeQuery(statementType.GetIsMuted, new List<NpgsqlParameter>() { new NpgsqlParameter("guildId", (long) guildId), new NpgsqlParameter("userId", (long) userId) }, true);
        public void IsMuted(ulong guildId, ulong userId, bool isMuted) => executeQuery(statementType.SetIsMuted, new List<NpgsqlParameter>() { new NpgsqlParameter("guildId", (long) guildId), new NpgsqlParameter("userId", (long) userId), new NpgsqlParameter("isMuted", isMuted) });

        public bool IsNoMemed(ulong guildId, ulong userId) => (bool) executeQuery(statementType.GetIsNoMemed, new List<NpgsqlParameter>() { new NpgsqlParameter("guildId", (long) guildId), new NpgsqlParameter("userId", (long) userId) }, true);
        public void IsNoMemed(ulong guildId, ulong userId, bool isNoMemed) => executeQuery(statementType.SetIsNoMemed, new List<NpgsqlParameter>() { new NpgsqlParameter("guildId", (long) guildId), new NpgsqlParameter("userId", (long) userId), new NpgsqlParameter("isNoMemed", isNoMemed) });

        public bool IsNoVC(ulong guildId, ulong userId) => (bool) executeQuery(statementType.GetIsNoVC, new List<NpgsqlParameter>() { new NpgsqlParameter("guildId", (long) guildId), new NpgsqlParameter("userId", (long) userId) }, true);
        public void IsNoVC(ulong guildId, ulong userId, bool isNoVC) => executeQuery(statementType.SetIsNoVC, new List<NpgsqlParameter>() { new NpgsqlParameter("guildId", (long) guildId), new NpgsqlParameter("userId", (long) userId), new NpgsqlParameter("isNoVC", isNoVC) });
    }
}