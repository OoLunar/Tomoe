using System.Collections.Generic;
using System.Linq;
using Npgsql;
using Npgsql.Logging;
using NpgsqlTypes;
using Tomoe.Database.Interfaces;
using Tomoe.Utils;

namespace Tomoe.Database.Drivers.PostgresSQL {
    public class PostgresGuild : IGuild {
        private enum statementType {
            GetAntiInvite,
            SetAntiInvite,
            GetAllowedInvites,
            SetAllowedInvites,
            IsAllowedInvite,
            AddAllowedInvite,
            RemoveAllowedInvite,
            GetMaxLines,
            SetMaxLines,
            GetMaxMentions,
            SetMaxMentions,
            GetAutoDehoist,
            SetAutoDehoist,
            GetAutoRaidMode,
            SetAutoRaidMode,
            GetIgnoredChannels,
            SetIgnoredChannels,
            IsIgnoredChannel,
            AddIgnoredChannel,
            RemoveIgnoredChannel,
            GetAdminRoles,
            SetAdminRoles,
            IsAdminRole,
            AddAdminRole,
            RemoveAdminRole,
            InsertGuildId,
            GuildIdExists,
            GetMuteRole,
            SetMuteRole,
            GetNoMemeRole,
            SetNoMemeRole,
            GetNoVCRole,
            SetNoVCRole,
            GetAntiRaid,
            SetAntiRaid,
            GetAntiRaidSetOff,
            SetAntiRaidSetOff,
            GetTag
        }

        private Dictionary<statementType, NpgsqlCommand> _preparedStatements = new Dictionary<statementType, NpgsqlCommand>();
        private static Logger _logger = new Logger("Database/PostgresSQL/Guild");

        /// <summary>
        /// Executes an SQL query from <see cref="Tomoe.Database.Drivers.PostgresSQL.PostgresGuild._preparedStatements">_preparedStatements</see>, using <seealso cref="Tomoe.Database.Drivers.PostgresSQL.PostgresGuild.statementType">statementType</seealso> as a key.
        /// 
        /// Returns a list of results if <paramref name="needsResult">needsResult</paramref> is true, otherwise returns null.
        /// </summary>
        /// <param name="command">Which SQL command to execute, using <see cref="Tomoe.Database.Drivers.PostgresSQL.PostgresGuild.statementType">statementType</see> as an index.</param>
        /// <param name="parameters">A list of <see cref="Npgsql.NpgsqlParameter">NpgsqlParameter's</see>.</param>
        /// <param name="needsResult">Returns a list of results if true, otherwise returns null.</param>
        /// <returns><see cref="System.Collections.Generic.List{T}">List&lt;dynamic&gt;</see> if <paramref name="needsResult">needsResult</paramref> is true, otherwise returns null.</returns>
        private List<dynamic> executeQuery(statementType command, List<NpgsqlParameter> parameters, bool needsResult = false) {
            _logger.Trace($"Executing {command.ToString()}");
            NpgsqlCommand statement = _preparedStatements[command];
            Dictionary<string, NpgsqlParameter> sortedParameters = new Dictionary<string, NpgsqlParameter>();
            foreach (NpgsqlParameter param in parameters) sortedParameters.Add(param.ParameterName, param);
            foreach (NpgsqlParameter temp in statement.Parameters) temp.Value = sortedParameters[temp.ParameterName].Value;
            if (needsResult) {
                NpgsqlDataReader reader = statement.ExecuteReader();
                List<dynamic> values = new List<dynamic>();
                while (reader.Read())
                    for (int i = 0; i < reader.FieldCount; i++) {
                        if (reader[i].GetType() == typeof(System.DBNull))
                            values.Add(null);
                        else
                            values.Add(reader[i]);
                    }
                reader.DisposeAsync().ConfigureAwait(false).GetAwaiter();
                if (values.Count == 0) values.Add(null);
                return values;
            } else {
                statement.ExecuteNonQuery();
                return null;
            }
        }

        /// <inheritdoc cref="Tomoe.Database.Drivers.PostgresSQL.PostgresGuild.executeQuery(statementType, List{NpgsqlParameter}, bool)" />
        /// <param name="parameter">One <see cref="Npgsql.NpgsqlParameter">NpgsqlParameter</see>, which gets converted into a <see cref="System.Collections.Generic.List{T}">List&lt;NpgsqlParameter&gt;</see>.</param>
        private List<dynamic> executeQuery(statementType command, NpgsqlParameter parameter, bool needsResult = false) => executeQuery(command, new List<NpgsqlParameter> { parameter }, needsResult);

        private NpgsqlConnection _connection;

        public PostgresGuild(string host, int port, string username, string password, string database_name, SslMode sslMode) {
            _connection = new NpgsqlConnection($"Host={host};Port={port};Username={username};Password={password};Database={database_name};SSL Mode={sslMode}");
            _logger.Info("Opening connection to database...");
            try {
                _connection.Open();
            } catch (System.TypeInitializationException error) {
                _logger.Critical($"Failed to connect to database. {error.Message}", true);
            }
            _logger.Info("Preparing SQL commands...");
            _logger.Trace($"Preparing {statementType.InsertGuildId}...");
            NpgsqlCommand insertGuildId = new NpgsqlCommand("INSERT INTO guild_config(guild_id) VALUES(@guildId)", _connection);
            insertGuildId.Parameters.Add(new NpgsqlParameter("guildId", NpgsqlDbType.Bigint));
            insertGuildId.Prepare();
            _preparedStatements.Add(statementType.InsertGuildId, insertGuildId);

            _logger.Trace($"Preparing {statementType.GuildIdExists}...");
            NpgsqlCommand guildIdExists = new NpgsqlCommand("SELECT exists(select 1 FROM guild_config WHERE guild_id=@guildId)", _connection);
            guildIdExists.Parameters.Add(new NpgsqlParameter("guildId", NpgsqlDbType.Bigint));
            guildIdExists.Prepare();
            _preparedStatements.Add(statementType.GuildIdExists, guildIdExists);

            _logger.Trace($"Preparing {statementType.GetAntiInvite}...");
            NpgsqlCommand getAntiInvite = new NpgsqlCommand("SELECT anti_invite FROM guild_config WHERE guild_id=@guildId", _connection);
            getAntiInvite.Parameters.Add(new NpgsqlParameter("guildId", NpgsqlDbType.Bigint));
            getAntiInvite.Prepare();
            _preparedStatements.Add(statementType.GetAntiInvite, getAntiInvite);

            _logger.Trace($"Preparing {statementType.SetAntiInvite}...");
            NpgsqlCommand setAntiInvite = new NpgsqlCommand("UPDATE guild_config SET anti_invite=@isEnabled WHERE guild_id=@guildId", _connection);
            setAntiInvite.Parameters.Add(new NpgsqlParameter("isEnabled", NpgsqlDbType.Boolean));
            setAntiInvite.Parameters.Add(new NpgsqlParameter("guildId", NpgsqlDbType.Bigint));
            setAntiInvite.Prepare();
            _preparedStatements.Add(statementType.SetAntiInvite, setAntiInvite);

            _logger.Trace($"Preparing {statementType.GetAllowedInvites}...");
            NpgsqlCommand getAllowedInvites = new NpgsqlCommand("SELECT allowed_invites FROM guild_config WHERE guild_id=@guildId", _connection);
            getAllowedInvites.Parameters.Add(new NpgsqlParameter("guildId", NpgsqlDbType.Bigint));
            getAllowedInvites.Prepare();
            _preparedStatements.Add(statementType.GetAllowedInvites, getAllowedInvites);

            _logger.Trace($"Preparing {statementType.SetAllowedInvites}...");
            NpgsqlCommand setAllowedInvites = new NpgsqlCommand("UPDATE guild_config SET allowed_invites=@allowedInvites WHERE guild_id=@guildId", _connection);
            setAllowedInvites.Parameters.Add(new NpgsqlParameter("allowedInvites", (NpgsqlDbType.Array | NpgsqlDbType.Text)));
            setAllowedInvites.Parameters.Add(new NpgsqlParameter("guildId", NpgsqlDbType.Bigint));
            setAllowedInvites.Prepare();
            _preparedStatements.Add(statementType.SetAllowedInvites, setAllowedInvites);

            _logger.Trace($"Preparing {statementType.AddAllowedInvite}...");
            NpgsqlCommand addAllowedInvite = new NpgsqlCommand("UPDATE guild_config SET allowed_invites=array_append(allowed_invites, @invite) WHERE guild_id=@guildId", _connection);
            addAllowedInvite.Parameters.Add(new NpgsqlParameter("invite", NpgsqlDbType.Text));
            addAllowedInvite.Parameters.Add(new NpgsqlParameter("guildId", NpgsqlDbType.Bigint));
            addAllowedInvite.Prepare();
            _preparedStatements.Add(statementType.AddAllowedInvite, addAllowedInvite);

            _logger.Trace($"Preparing {statementType.RemoveAllowedInvite}...");
            NpgsqlCommand removeAllowedInvite = new NpgsqlCommand("UPDATE guild_config SET allowed_invites=array_remove(allowed_invites, @invite) WHERE guild_id=@guildId", _connection);
            removeAllowedInvite.Parameters.Add(new NpgsqlParameter("invite", NpgsqlDbType.Text));
            removeAllowedInvite.Parameters.Add(new NpgsqlParameter("guildId", NpgsqlDbType.Bigint));
            removeAllowedInvite.Prepare();
            _preparedStatements.Add(statementType.RemoveAllowedInvite, removeAllowedInvite);

            _logger.Trace($"Preparing {statementType.IsAllowedInvite}...");
            NpgsqlCommand isAllowedInvite = new NpgsqlCommand("SELECT allowed_invites && @invite::text[] FROM guild_config WHERE guild_id=@guildId", _connection);
            isAllowedInvite.Parameters.Add(new NpgsqlParameter("invite", NpgsqlDbType.Text));
            isAllowedInvite.Parameters.Add(new NpgsqlParameter("guildId", NpgsqlDbType.Bigint));
            isAllowedInvite.Prepare();
            _preparedStatements.Add(statementType.IsAllowedInvite, isAllowedInvite);

            _logger.Trace($"Preparing {statementType.GetMaxLines}...");
            NpgsqlCommand getMaxLines = new NpgsqlCommand("SELECT max_lines FROM guild_config WHERE guild_id=@guildId", _connection);
            getMaxLines.Parameters.Add(new NpgsqlParameter("guildId", NpgsqlDbType.Bigint));
            getMaxLines.Prepare();
            _preparedStatements.Add(statementType.GetMaxLines, getMaxLines);

            _logger.Trace($"Preparing {statementType.SetMaxLines}...");
            NpgsqlCommand setMaxLines = new NpgsqlCommand("UPDATE guild_config SET max_lines=@maxLines WHERE guild_id=@guildId", _connection);
            setMaxLines.Parameters.Add(new NpgsqlParameter("maxLines", NpgsqlDbType.Integer));
            setMaxLines.Parameters.Add(new NpgsqlParameter("guildId", NpgsqlDbType.Bigint));
            setMaxLines.Prepare();
            _preparedStatements.Add(statementType.SetMaxLines, setMaxLines);

            _logger.Trace($"Preparing {statementType.GetMaxMentions}...");
            NpgsqlCommand getMaxMentions = new NpgsqlCommand("SELECT max_mentions FROM guild_config WHERE guild_id=@guildId", _connection);
            getMaxMentions.Parameters.Add(new NpgsqlParameter("guildId", NpgsqlDbType.Bigint));
            getMaxMentions.Prepare();
            _preparedStatements.Add(statementType.GetMaxMentions, getMaxMentions);

            _logger.Trace($"Preparing {statementType.SetMaxMentions}...");
            NpgsqlCommand setMaxMentions = new NpgsqlCommand("UPDATE guild_config SET max_mentions=@maxMentions WHERE guild_id=@guildId", _connection);
            setMaxMentions.Parameters.Add(new NpgsqlParameter("maxMentions", NpgsqlDbType.Integer));
            setMaxMentions.Parameters.Add(new NpgsqlParameter("guildId", NpgsqlDbType.Bigint));
            setMaxMentions.Prepare();
            _preparedStatements.Add(statementType.SetMaxMentions, setMaxMentions);

            _logger.Trace($"Preparing {statementType.GetAutoDehoist}...");
            NpgsqlCommand getAutoDehoist = new NpgsqlCommand("SELECT auto_dehoist FROM guild_config WHERE guild_id=@guildId", _connection);
            getAutoDehoist.Parameters.Add(new NpgsqlParameter("guildId", NpgsqlDbType.Bigint));
            getAutoDehoist.Prepare();
            _preparedStatements.Add(statementType.GetAutoDehoist, getAutoDehoist);

            _logger.Trace($"Preparing {statementType.SetAutoDehoist}...");
            NpgsqlCommand setAutoDehoist = new NpgsqlCommand("UPDATE guild_config SET auto_dehoist=@autoDehoist WHERE guild_id=@guildId", _connection);
            setAutoDehoist.Parameters.Add(new NpgsqlParameter("autoDehoist", NpgsqlDbType.Boolean));
            setAutoDehoist.Parameters.Add(new NpgsqlParameter("guildId", NpgsqlDbType.Bigint));
            setAutoDehoist.Prepare();
            _preparedStatements.Add(statementType.SetAutoDehoist, setAutoDehoist);

            _logger.Trace($"Preparing {statementType.GetAutoRaidMode}...");
            NpgsqlCommand getAutoRaidMode = new NpgsqlCommand("SELECT auto_raidmode FROM guild_config WHERE guild_id=@guildId", _connection);
            getAutoRaidMode.Parameters.Add(new NpgsqlParameter("guildId", NpgsqlDbType.Bigint));
            getAutoRaidMode.Prepare();
            _preparedStatements.Add(statementType.GetAutoRaidMode, getAutoRaidMode);

            _logger.Trace($"Preparing {statementType.SetAutoRaidMode}...");
            NpgsqlCommand setAutoRaidMode = new NpgsqlCommand("UPDATE guild_config SET auto_raidmode=@autoRaidMode WHERE guild_id=@guildId", _connection);
            setAutoRaidMode.Parameters.Add(new NpgsqlParameter("autoRaidMode", NpgsqlDbType.Boolean));
            setAutoRaidMode.Parameters.Add(new NpgsqlParameter("guildId", NpgsqlDbType.Bigint));
            setAutoRaidMode.Prepare();
            _preparedStatements.Add(statementType.SetAutoRaidMode, setAutoRaidMode);

            _logger.Trace($"Preparing {statementType.GetIgnoredChannels}...");
            NpgsqlCommand getIgnoredChannels = new NpgsqlCommand("SELECT ignored_channels FROM guild_config WHERE guild_id=@guildId", _connection);
            getIgnoredChannels.Parameters.Add(new NpgsqlParameter("guildId", NpgsqlDbType.Bigint));
            getIgnoredChannels.Prepare();
            _preparedStatements.Add(statementType.GetIgnoredChannels, getIgnoredChannels);

            _logger.Trace($"Preparing {statementType.SetIgnoredChannels}...");
            NpgsqlCommand setIgnoredChannels = new NpgsqlCommand("UPDATE guild_config SET ignored_channels=@ignoredChannels WHERE guild_id=@guildId", _connection);
            setIgnoredChannels.Parameters.Add(new NpgsqlParameter("ignoredChannels", (NpgsqlDbType.Array | NpgsqlDbType.Bigint)));
            setIgnoredChannels.Parameters.Add(new NpgsqlParameter("guildId", NpgsqlDbType.Bigint));
            setIgnoredChannels.Prepare();
            _preparedStatements.Add(statementType.SetIgnoredChannels, setIgnoredChannels);

            _logger.Trace($"Preparing {statementType.AddIgnoredChannel}...");
            NpgsqlCommand addIgnoredChannel = new NpgsqlCommand("UPDATE guild_config SET ignored_channels=array_append(ignored_channels, @channelId) WHERE guild_id=@guildId", _connection);
            addIgnoredChannel.Parameters.Add(new NpgsqlParameter("channelId", NpgsqlDbType.Bigint));
            addIgnoredChannel.Parameters.Add(new NpgsqlParameter("guildId", NpgsqlDbType.Bigint));
            addIgnoredChannel.Prepare();
            _preparedStatements.Add(statementType.AddIgnoredChannel, addIgnoredChannel);

            _logger.Trace($"Preparing {statementType.RemoveIgnoredChannel}...");
            NpgsqlCommand removeIgnoredChannel = new NpgsqlCommand("UPDATE guild_config SET ignored_channels=array_remove(ignored_channels, @channelId) WHERE guild_id=@guildId", _connection);
            removeIgnoredChannel.Parameters.Add(new NpgsqlParameter("channelId", NpgsqlDbType.Bigint));
            removeIgnoredChannel.Parameters.Add(new NpgsqlParameter("guildId", NpgsqlDbType.Bigint));
            removeIgnoredChannel.Prepare();
            _preparedStatements.Add(statementType.RemoveIgnoredChannel, removeIgnoredChannel);

            _logger.Trace($"Preparing {statementType.IsIgnoredChannel}...");
            NpgsqlCommand isIgnoredChannel = new NpgsqlCommand("SELECT ignored_channels && ARRAY[@channelId] FROM guild_config WHERE guild_id=@guildId", _connection);
            isIgnoredChannel.Parameters.Add(new NpgsqlParameter("channelid", NpgsqlDbType.Bigint));
            isIgnoredChannel.Parameters.Add(new NpgsqlParameter("guildId", NpgsqlDbType.Bigint));
            isIgnoredChannel.Prepare();
            _preparedStatements.Add(statementType.IsIgnoredChannel, isIgnoredChannel);

            _logger.Trace($"Preparing {statementType.GetAdminRoles}...");
            NpgsqlCommand getAdminRoles = new NpgsqlCommand("SELECT administraitive_roles FROM guild_config WHERE guild_id=@guildId", _connection);
            getAdminRoles.Parameters.Add(new NpgsqlParameter("guildId", NpgsqlDbType.Bigint));
            getAdminRoles.Prepare();
            _preparedStatements.Add(statementType.GetAdminRoles, getAdminRoles);

            _logger.Trace($"Preparing {statementType.SetAdminRoles}...");
            NpgsqlCommand setAdminRoles = new NpgsqlCommand("UPDATE guild_config SET administraitive_roles=@roleIds WHERE guild_id=@guildId", _connection);
            setAdminRoles.Parameters.Add(new NpgsqlParameter("roleIds", (NpgsqlDbType.Array | NpgsqlDbType.Bigint)));
            setAdminRoles.Parameters.Add(new NpgsqlParameter("guildId", NpgsqlDbType.Bigint));
            setAdminRoles.Prepare();
            _preparedStatements.Add(statementType.SetAdminRoles, setAdminRoles);

            _logger.Trace($"Preparing {statementType.AddAdminRole}...");
            NpgsqlCommand addAdminRole = new NpgsqlCommand("UPDATE guild_config SET administraitive_roles=array_append(administraitive_roles, @roleId) WHERE guild_id=@guildId", _connection);
            addAdminRole.Parameters.Add(new NpgsqlParameter("roleId", NpgsqlDbType.Bigint));
            addAdminRole.Parameters.Add(new NpgsqlParameter("guildId", NpgsqlDbType.Bigint));
            addAdminRole.Prepare();
            _preparedStatements.Add(statementType.AddAdminRole, addAdminRole);

            _logger.Trace($"Preparing {statementType.RemoveAdminRole}...");
            NpgsqlCommand removeAdminRole = new NpgsqlCommand("UPDATE guild_config SET administraitive_roles=array_remove(administraitive_roles, @roleId) WHERE guild_id=@guildId", _connection);
            removeAdminRole.Parameters.Add(new NpgsqlParameter("roleId", NpgsqlDbType.Bigint));
            removeAdminRole.Parameters.Add(new NpgsqlParameter("guildId", NpgsqlDbType.Bigint));
            removeAdminRole.Prepare();
            _preparedStatements.Add(statementType.RemoveAdminRole, removeAdminRole);

            _logger.Trace($"Preparing {statementType.IsAdminRole}...");
            NpgsqlCommand isAdminRole = new NpgsqlCommand("SELECT administraitive_roles && ARRAY[@roleId] FROM guild_config WHERE guild_id=@guildId", _connection);
            isAdminRole.Parameters.Add(new NpgsqlParameter("roleId", NpgsqlDbType.Bigint));
            isAdminRole.Parameters.Add(new NpgsqlParameter("guildId", NpgsqlDbType.Bigint));
            isAdminRole.Prepare();
            _preparedStatements.Add(statementType.IsAdminRole, isAdminRole);

            //TODO: Setup logging queries

            _logger.Trace($"Preparing {statementType.GetMuteRole}...");
            NpgsqlCommand getMuteRole = new NpgsqlCommand("SELECT mute_role FROM guild_config WHERE guild_id=@guildId", _connection);
            getMuteRole.Parameters.Add(new NpgsqlParameter("guildId", NpgsqlDbType.Bigint));
            getMuteRole.Prepare();
            _preparedStatements.Add(statementType.GetMuteRole, getMuteRole);

            _logger.Trace($"Preparing {statementType.SetMuteRole}...");
            NpgsqlCommand setMuteRole = new NpgsqlCommand("UPDATE guild_config SET mute_role=@roleId WHERE guild_id=@guildId", _connection);
            setMuteRole.Parameters.Add(new NpgsqlParameter("roleId", NpgsqlDbType.Bigint));
            setMuteRole.Parameters.Add(new NpgsqlParameter("guildId", NpgsqlDbType.Bigint));
            setMuteRole.Prepare();
            _preparedStatements.Add(statementType.SetMuteRole, setMuteRole);

            _logger.Trace($"Preparing {statementType.GetNoMemeRole}...");
            NpgsqlCommand getNoMemeRole = new NpgsqlCommand("SELECT nomeme_role FROM guild_config WHERE guild_id=@guildId", _connection);
            getNoMemeRole.Parameters.Add(new NpgsqlParameter("guildId", NpgsqlDbType.Bigint));
            getNoMemeRole.Prepare();
            _preparedStatements.Add(statementType.GetNoMemeRole, getNoMemeRole);

            _logger.Trace($"Preparing {statementType.SetNoMemeRole}...");
            NpgsqlCommand setNoMemeRole = new NpgsqlCommand("UPDATE guild_config SET nomeme_role=@roleId WHERE guild_id=@guildId", _connection);
            setNoMemeRole.Parameters.Add(new NpgsqlParameter("roleId", NpgsqlDbType.Bigint));
            setNoMemeRole.Parameters.Add(new NpgsqlParameter("guildId", NpgsqlDbType.Bigint));
            setNoMemeRole.Prepare();
            _preparedStatements.Add(statementType.SetNoMemeRole, setNoMemeRole);

            _logger.Trace($"Preparing {statementType.GetNoVCRole}...");
            NpgsqlCommand getNoVCRole = new NpgsqlCommand("SELECT novc_role FROM guild_config WHERE guild_id=@guildId", _connection);
            getNoVCRole.Parameters.Add(new NpgsqlParameter("guildId", NpgsqlDbType.Bigint));
            getNoVCRole.Prepare();
            _preparedStatements.Add(statementType.GetNoVCRole, getNoVCRole);

            _logger.Trace($"Preparing {statementType.SetNoVCRole}...");
            NpgsqlCommand setNoVCRole = new NpgsqlCommand("UPDATE guild_config SET novc_role=@roleId WHERE guild_id=@guildId", _connection);
            setNoVCRole.Parameters.Add(new NpgsqlParameter("roleId", NpgsqlDbType.Bigint));
            setNoVCRole.Parameters.Add(new NpgsqlParameter("guildId", NpgsqlDbType.Bigint));
            setNoVCRole.Prepare();
            _preparedStatements.Add(statementType.SetNoVCRole, setNoVCRole);

            _logger.Trace($"Preparing {statementType.GetAntiRaid}...");
            NpgsqlCommand getAntiRaid = new NpgsqlCommand("SELECT antiraid FROM guild_config WHERE guild_id=@guildId", _connection);
            getAntiRaid.Parameters.Add(new NpgsqlParameter("guildId", NpgsqlDbType.Bigint));
            getAntiRaid.Prepare();
            _preparedStatements.Add(statementType.GetAntiRaid, getAntiRaid);

            _logger.Trace($"Preparing {statementType.SetAntiRaid}...");
            NpgsqlCommand setAntiRaid = new NpgsqlCommand("UPDATE guild_config SET antiraid=@isEnabled WHERE guild_id=@guildId", _connection);
            setAntiRaid.Parameters.Add(new NpgsqlParameter("isEnabled", NpgsqlDbType.Boolean));
            setAntiRaid.Parameters.Add(new NpgsqlParameter("guildId", NpgsqlDbType.Bigint));
            setAntiRaid.Prepare();
            _preparedStatements.Add(statementType.SetAntiRaid, setAntiRaid);

            _logger.Trace($"Preparing {statementType.GetAntiRaidSetOff}...");
            NpgsqlCommand getAntiRaidSetOff = new NpgsqlCommand("SELECT antiraid_setoff FROM guild_config WHERE guild_id=@guildId", _connection);
            getAntiRaidSetOff.Parameters.Add(new NpgsqlParameter("guildId", NpgsqlDbType.Bigint));
            getAntiRaidSetOff.Prepare();
            _preparedStatements.Add(statementType.GetAntiRaidSetOff, getAntiRaidSetOff);

            _logger.Trace($"Preparing {statementType.SetAntiRaidSetOff}...");
            NpgsqlCommand setAntiRaidSetOff = new NpgsqlCommand("UPDATE guild_config SET antiraid_setoff=@interval WHERE guild_id=@guildId", _connection);
            setAntiRaidSetOff.Parameters.Add(new NpgsqlParameter("interval", NpgsqlDbType.Integer));
            setAntiRaidSetOff.Parameters.Add(new NpgsqlParameter("guildId", NpgsqlDbType.Bigint));
            setAntiRaidSetOff.Prepare();
            _preparedStatements.Add(statementType.SetAntiRaidSetOff, setAntiRaidSetOff);

            _logger.Debug("Done preparing commands!");
        }

        public void InsertGuildId(ulong guildId) => executeQuery(statementType.InsertGuildId, new NpgsqlParameter("guildId", (long) guildId));
        public bool GuildIdExists(ulong guildId) => (bool) executeQuery(statementType.GuildIdExists, new NpgsqlParameter("guildId", (long) guildId), true) [0];

        public bool AntiInvite(ulong guildId) => (bool) executeQuery(statementType.GetAntiInvite, new NpgsqlParameter("guildId", (long) guildId), true) [0];
        public void AntiInvite(ulong guildId, bool isEnabled) => executeQuery(statementType.SetAntiInvite, new List<NpgsqlParameter>() { new NpgsqlParameter("guildId", (long) guildId), new NpgsqlParameter("isEnabled", isEnabled) });

        public string[] AllowedInvites(ulong guildId) => (string[]) executeQuery(statementType.GetAllowedInvites, new NpgsqlParameter("guildId", (long) guildId), true) [0];
        public void AllowedInvites(ulong guildId, string[] allowedInvites) => executeQuery(statementType.SetAllowedInvites, new List<NpgsqlParameter>() { new NpgsqlParameter("guildId", (long) guildId), new NpgsqlParameter("allowedInvites", allowedInvites) });
        public void AddAllowedInvite(ulong guildId, string invite) => executeQuery(statementType.AddAllowedInvite, new List<NpgsqlParameter>() { new NpgsqlParameter("guildId", (long) guildId), new NpgsqlParameter("invite", invite) });
        public void RemoveAllowedInvite(ulong guildId, string invite) => executeQuery(statementType.RemoveAllowedInvite, new List<NpgsqlParameter>() { new NpgsqlParameter("guildId", (long) guildId), new NpgsqlParameter("invite", invite) });
        public bool IsAllowedInvite(ulong guildId, string invite) => (bool) executeQuery(statementType.IsAllowedInvite, new List<NpgsqlParameter>() { new NpgsqlParameter("guildId", (long) guildId), new NpgsqlParameter("invite", invite) }, true) [0];

        public int MaxLines(ulong guildId) => (int) executeQuery(statementType.GetMaxLines, new NpgsqlParameter("guildId", (long) guildId), true) [0];
        public void MaxLines(ulong guildId, int maxLines) => executeQuery(statementType.SetMaxLines, new List<NpgsqlParameter>() { new NpgsqlParameter("guildId", (long) guildId), new NpgsqlParameter("maxLines", maxLines) }, true);

        public int MaxMentions(ulong guildId) => (int) executeQuery(statementType.GetMaxMentions, new NpgsqlParameter("guildId", (long) guildId), true) [0];
        public void MaxMentions(ulong guildId, int maxMentions) => executeQuery(statementType.SetMaxMentions, new List<NpgsqlParameter>() { new NpgsqlParameter("guildId", (long) guildId), new NpgsqlParameter("maxMentions", maxMentions) });

        public bool AutoDehoist(ulong guildId) => (bool) executeQuery(statementType.GetAutoDehoist, new NpgsqlParameter("guildId", (long) guildId), true) [0];
        public void AutoDehoist(ulong guildId, bool isEnabled) => executeQuery(statementType.SetAutoDehoist, new List<NpgsqlParameter>() { new NpgsqlParameter("guildId", (long) guildId), new NpgsqlParameter("isEnabled", isEnabled) });

        public bool AutoRaidMode(ulong guildId) => (bool) executeQuery(statementType.GetAutoRaidMode, new NpgsqlParameter("guildId", (long) guildId), true) [0];
        public void AutoRaidMode(ulong guildId, bool isEnabled) => executeQuery(statementType.SetAutoRaidMode, new List<NpgsqlParameter>() { new NpgsqlParameter("guildId", (long) guildId), new NpgsqlParameter("isEnabled", isEnabled) });

        public ulong[] IgnoredChannels(ulong guildId) => (ulong[]) executeQuery(statementType.GetIgnoredChannels, new NpgsqlParameter("guildId", (long) guildId), true) [0];
        public void IgnoredChannels(ulong guildId, ulong[] ignoredChannels) => executeQuery(statementType.SetIgnoredChannels, new List<NpgsqlParameter>() { new NpgsqlParameter("guildId", (long) guildId), new NpgsqlParameter("channelIds", ignoredChannels.Select((role) => long.Parse(role.ToString()))) });
        public void AddIgnoredChannel(ulong guildId, ulong channelId) => executeQuery(statementType.AddIgnoredChannel, new List<NpgsqlParameter>() { new NpgsqlParameter("guildId", (long) guildId), new NpgsqlParameter("channelId", (long) channelId) });
        public void RemoveIgnoredChannel(ulong guildId, ulong channelId) => executeQuery(statementType.RemoveIgnoredChannel, new List<NpgsqlParameter>() { new NpgsqlParameter("guildId", (long) guildId), new NpgsqlParameter("channelId", (long) channelId) });
        public bool IsIgnoredChannel(ulong guildId, ulong channelId) => (bool) executeQuery(statementType.IsIgnoredChannel, new List<NpgsqlParameter>() { new NpgsqlParameter("guildId", (long) guildId), new NpgsqlParameter("channelId", (long) channelId) }, true) [0];

        public ulong[] AdminRoles(ulong guildId) => (ulong[]) executeQuery(statementType.GetAdminRoles, new NpgsqlParameter("guildId", (long) guildId), true) [0];
        public void AdminRoles(ulong guildId, ulong[] roleIds) => executeQuery(statementType.SetAdminRoles, new List<NpgsqlParameter>() { new NpgsqlParameter("guildId", (long) guildId), new NpgsqlParameter("roleIds", roleIds.Select((role) => long.Parse(role.ToString()))) });
        public void AddAdminRole(ulong guildId, ulong roleId) => executeQuery(statementType.AddAdminRole, new List<NpgsqlParameter>() { new NpgsqlParameter("guildId", (long) guildId), new NpgsqlParameter("roleId", (long) roleId) });
        public void RemoveAdminRole(ulong guildId, ulong roleId) => executeQuery(statementType.RemoveAdminRole, new List<NpgsqlParameter>() { new NpgsqlParameter("guildId", (long) guildId), new NpgsqlParameter("roleId", (long) roleId) });
        public bool IsAdminRole(ulong guildId, ulong roleId) => (bool) executeQuery(statementType.IsAdminRole, new List<NpgsqlParameter>() { new NpgsqlParameter("guildId", (long) guildId), new NpgsqlParameter("roleId", (long) roleId) }, true) [0];

        public ulong? MuteRole(ulong guildId) => (ulong?) executeQuery(statementType.GetMuteRole, new NpgsqlParameter("guildId", (long) guildId), true) [0];
        public void MuteRole(ulong guildId, ulong roleId) => executeQuery(statementType.SetMuteRole, new List<NpgsqlParameter>() { new NpgsqlParameter("guildId", (long) guildId), new NpgsqlParameter("roleId", (long) roleId) });

        public ulong? NoMemeRole(ulong guildId) => (ulong?) executeQuery(statementType.GetNoMemeRole, new NpgsqlParameter("guildId", (long) guildId), true) [0];
        public void NoMemeRole(ulong guildId, ulong roleId) => executeQuery(statementType.SetNoMemeRole, new List<NpgsqlParameter>() { new NpgsqlParameter("guildId", (long) guildId), new NpgsqlParameter("roleId", (long) roleId) });

        public ulong? NoVCRole(ulong guildId) => (ulong?) executeQuery(statementType.GetNoVCRole, new NpgsqlParameter("guildId", (long) guildId), true) [0];
        public void NoVCRole(ulong guildId, ulong roleId) => executeQuery(statementType.SetNoVCRole, new List<NpgsqlParameter>() { new NpgsqlParameter("guildId", (long) guildId), new NpgsqlParameter("roleId", (long) roleId) });

        public bool AntiRaid(ulong guildId) => (bool) executeQuery(statementType.GetAntiRaid, new NpgsqlParameter("guildId", (long) guildId), true) [0];
        public void AntiRaid(ulong guildId, bool isEnabled) => executeQuery(statementType.SetAntiRaid, new List<NpgsqlParameter>() { new NpgsqlParameter("guildId", (long) guildId), new NpgsqlParameter("isEnabled", isEnabled) });

        public int AntiRaidSetOff(ulong guildId) => (int) executeQuery(statementType.GetAntiRaidSetOff, new NpgsqlParameter("guildId", (long) guildId), true) [0];
        public void AntiRaidSetOff(ulong guildId, int interval) => executeQuery(statementType.SetAntiRaidSetOff, new List<NpgsqlParameter>() { new NpgsqlParameter("guildId", (long) guildId), new NpgsqlParameter("interval", interval) });
    }
}