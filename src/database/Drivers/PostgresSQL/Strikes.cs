using System;
using System.Collections.Generic;
using System.IO;
using Npgsql;
using NpgsqlTypes;
using Tomoe.Database.Interfaces;
using Tomoe.Utils;

namespace Tomoe.Database.Drivers.PostgresSQL {
    public class PostgresStrikes : IStrikes {
        private static readonly Logger _logger = new Logger("Database.PostgresSQL.Strike");
        private readonly NpgsqlConnection _connection;
        private readonly Dictionary<statementType, NpgsqlCommand> _preparedStatements = new Dictionary<statementType, NpgsqlCommand>();
        private enum statementType {
            GetStrike,
            GetVictim,
            GetIssued,
            Add,
            Drop,
            Edit
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
                        if (reader[i].GetType() == typeof(DBNull))
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

        /// <inheritdoc cref="Tomoe.Database.Drivers.PostgresSQL.PostgresStrikes.executeQuery(statementType, List{NpgsqlParameter}, bool)" />
        /// <param name="parameter">One <see cref="Npgsql.NpgsqlParameter">NpgsqlParameter</see>, which gets converted into a <see cref="System.Collections.Generic.List{T}">List&lt;NpgsqlParameter&gt;</see>.</param>
        private Dictionary<int, List<dynamic>> executeQuery(statementType command, NpgsqlParameter parameter, bool needsResult = false) => executeQuery(command, new List<NpgsqlParameter> { parameter }, needsResult);

        public PostgresStrikes(string host, int port, string username, string password, string database_name, SslMode sslMode) {
            _connection = new NpgsqlConnection($"Host={host};Port={port};Username={username};Password={password};Database={database_name};SSL Mode={sslMode}");
            _logger.Info("Opening connection to database...");
            try {
                _connection.Open();
                NpgsqlCommand createTagsTable = new NpgsqlCommand(File.ReadAllText(Path.Join(FileSystem.ProjectRoot, "res/sql/strike_table.sql")), _connection);
                createTagsTable.ExecuteNonQuery();
                createTagsTable.Dispose();
            } catch (System.Net.Sockets.SocketException error) {
                _logger.Critical($"Failed to connect to database. {error.Message}", true);
            }
            _logger.Info("Preparing SQL commands...");
            _logger.Debug($"Preparing {statementType.Add}...");
            NpgsqlCommand add = new NpgsqlCommand("INSERT INTO strikes VALUES(@guildId, @victimId, @issuerId, ARRAY[@reason], @jumpLink, @victimMessaged, DEFAULT, DEFAULT, DEFAULT, calc_strike_count(@guildId, @victimId)) RETURNING dropped, created_at, id, strike_count", _connection);
            add.Parameters.Add(new NpgsqlParameter("guildId", NpgsqlDbType.Bigint));
            add.Parameters.Add(new NpgsqlParameter("victimId", NpgsqlDbType.Bigint));
            add.Parameters.Add(new NpgsqlParameter("issuerId", NpgsqlDbType.Bigint));
            add.Parameters.Add(new NpgsqlParameter("reason", NpgsqlDbType.Varchar));
            add.Parameters.Add(new NpgsqlParameter("jumpLink", NpgsqlDbType.Varchar));
            add.Parameters.Add(new NpgsqlParameter("victimMessaged", NpgsqlDbType.Boolean));
            add.Prepare();
            _preparedStatements.Add(statementType.Add, add);

            _logger.Debug($"Preparing {statementType.Drop}...");
            NpgsqlCommand drop = new NpgsqlCommand("UPDATE strikes SET dropped=true, reason=array_append(reason, @reason) WHERE id=@strikeId RETURNING guild_id, victim_id, issuer_id, reason, victim_messaged, created_at, strike_count", _connection);
            drop.Parameters.Add(new NpgsqlParameter("strikeId", NpgsqlDbType.Bigint));
            drop.Parameters.Add(new NpgsqlParameter("reason", NpgsqlDbType.Varchar));
            drop.Prepare();
            _preparedStatements.Add(statementType.Drop, drop);

            _logger.Debug($"Preparing {statementType.Edit}...");
            NpgsqlCommand edit = new NpgsqlCommand("UPDATE strikes SET reason=array_append(reason, @reason) WHERE id=@strikeId", _connection);
            edit.Parameters.Add(new NpgsqlParameter("reason", NpgsqlDbType.Varchar));
            edit.Parameters.Add(new NpgsqlParameter("strikeId", NpgsqlDbType.Integer));
            edit.Prepare();
            _preparedStatements.Add(statementType.Edit, edit);

            _logger.Debug($"Preparing {statementType.GetIssued}...");
            NpgsqlCommand getIssued = new NpgsqlCommand("SELECT guild_id, victim_id, issuer_id, reason, jumplink, victim_messaged, dropped, created_at, id, strike_count FROM strikes WHERE guild_id=@guildId AND issuer_id=@issuerId", _connection);
            getIssued.Parameters.Add(new NpgsqlParameter("guildId", NpgsqlDbType.Bigint));
            getIssued.Parameters.Add(new NpgsqlParameter("issuerId", NpgsqlDbType.Bigint));
            getIssued.Prepare();
            _preparedStatements.Add(statementType.GetIssued, getIssued);

            _logger.Debug($"Preparing {statementType.GetStrike}...");
            NpgsqlCommand getStrike = new NpgsqlCommand("SELECT * FROM strikes WHERE id=@strikeId", _connection);
            getStrike.Parameters.Add(new NpgsqlParameter("strikeId", NpgsqlDbType.Integer));
            getStrike.Prepare();
            _preparedStatements.Add(statementType.GetStrike, getStrike);

            _logger.Debug($"Preparing {statementType.GetVictim}...");
            NpgsqlCommand getStrikes = new NpgsqlCommand("SELECT * FROM strikes WHERE guild_id=@guildId AND victim_id=@victimId", _connection);
            getStrikes.Parameters.Add(new NpgsqlParameter("guildId", NpgsqlDbType.Bigint));
            getStrikes.Parameters.Add(new NpgsqlParameter("victimId", NpgsqlDbType.Bigint));
            getStrikes.Prepare();
            _preparedStatements.Add(statementType.GetVictim, getStrikes);
        }

        public Strike Add(ulong guildId, ulong victimId, ulong issuerId, string reason, string jumpLink, bool victimMessaged) {
            List<dynamic> queryResults = executeQuery(statementType.Add, new List<NpgsqlParameter>() { new NpgsqlParameter("guildId", (long) guildId), new NpgsqlParameter("victimId", (long) victimId), new NpgsqlParameter("issuerId", (long) issuerId), new NpgsqlParameter("reason", reason), new NpgsqlParameter("jumpLink", jumpLink), new NpgsqlParameter("victimMessaged", victimMessaged) }, true) [0];
            Strike strike = new();
            strike.GuildId = guildId;
            strike.VictimId = victimId;
            strike.IssuerId = issuerId;
            strike.Reason = new string[] { reason };
            strike.JumpLink = jumpLink;
            strike.VictimMessaged = victimMessaged;
            strike.Dropped = (bool) queryResults[0];
            strike.CreatedAt = (DateTime) queryResults[1];
            strike.Id = (int) queryResults[2];
            strike.StrikeCount = (int) queryResults[3];
            return strike;
        }

        public Strike Drop(int strikeId, string reason) {
            List<dynamic> queryResults = executeQuery(statementType.Drop, new List<NpgsqlParameter>() { new NpgsqlParameter("strikeId", strikeId), new NpgsqlParameter("reason", reason) }) [0];
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
        public void Edit(int strikeId, string reason) => executeQuery(statementType.Edit, new List<NpgsqlParameter>() { new NpgsqlParameter("strikeId", strikeId), new NpgsqlParameter("reason", reason) });
        public Strike? Get(int strikeId) {
            List<dynamic> queryResults = executeQuery(statementType.Edit, new NpgsqlParameter("strikeId", strikeId), true) [0];
            Strike strike = new();
            if (queryResults == null) return null;
            strike.GuildId = (ulong) queryResults[0];
            strike.VictimId = (ulong) queryResults[1];
            strike.IssuerId = (ulong) queryResults[2];
            strike.Reason = (string[]) queryResults[3];
            strike.JumpLink = (string) queryResults[4];
            strike.VictimMessaged = (bool) queryResults[5];
            strike.Dropped = (bool) queryResults[6];
            strike.CreatedAt = (DateTime) queryResults[7];
            strike.Id = (int) queryResults[8];
            strike.StrikeCount = (int) queryResults[9];
            return strike;
        }

        public Strike[] GetVictim(ulong guildId, ulong victimId) {
            Dictionary<int, List<dynamic>> queryResults = executeQuery(statementType.GetVictim, new List<NpgsqlParameter>() { new NpgsqlParameter("guildId", (long) guildId), new NpgsqlParameter("victimId", (long) victimId) }, true);
            List<Strike> strikes = new List<Strike>();
            if (queryResults == null) return null;
            foreach (List<dynamic> query in queryResults.Values) {
                Strike strike = new();
                strike.GuildId = (ulong) query[0];
                strike.VictimId = (ulong) query[1];
                strike.IssuerId = (ulong) query[2];
                strike.Reason = (string[]) query[3];
                strike.JumpLink = (string) query[4];
                strike.VictimMessaged = (bool) query[5];
                strike.Dropped = (bool) query[6];
                strike.CreatedAt = (DateTime) query[7];
                strike.Id = (int) query[8];
                strike.StrikeCount = (int) query[9];
                strikes.Add(strike);
            }
            return strikes.ToArray();
        }
        public Strike[] GetIssued(ulong guildId, ulong issuerId) {
            Dictionary<int, List<dynamic>> queryResults = executeQuery(statementType.GetIssued, new List<NpgsqlParameter>() { new NpgsqlParameter("guildId", (long) guildId), new NpgsqlParameter("issuerId", (long) issuerId) });
            List<Strike> strikes = new List<Strike>();
            if (queryResults == null) return null;
            foreach (List<dynamic> query in queryResults.Values) {
                Strike strike = new();
                strike.GuildId = (ulong) query[0];
                strike.VictimId = (ulong) query[1];
                strike.IssuerId = (ulong) query[2];
                strike.Reason = (string[]) query[3];
                strike.JumpLink = (string) query[4];
                strike.VictimMessaged = (bool) query[5];
                strike.Dropped = (bool) query[6];
                strike.CreatedAt = (DateTime) query[7];
                strike.Id = (int) query[8];
                strikes.Add(strike);
            }
            return strikes.ToArray();
        }
    }
}