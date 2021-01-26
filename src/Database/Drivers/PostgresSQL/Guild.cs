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
	public class PostgresGuild : IGuild
	{
		private static readonly Logger _logger = new("Database.PostgresSQL.Guild");
		private readonly NpgsqlConnection _connection;
		private readonly Dictionary<StatementType, NpgsqlCommand> _preparedStatements = new();
		private int retryCount;
		private enum StatementType
		{
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
			GetAntiMemeRole,
			SetAntiMemeRole,
			GetNoVCRole,
			SetNoVCRole,
			GetAntiRaid,
			SetAntiRaid,
			GetAntiRaidSetOff,
			SetAntiRaidSetOff
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

		public PostgresGuild(string host, int port, string username, string password, string databaseName, SslMode sslMode)
		{
			_connection = new($"Host={host};Port={port};Username={username};Password={password};Database={databaseName};SSL Mode={sslMode}");
			_logger.Info("Opening connection to database...");
			try
			{
				_connection.Open();
				_logger.Debug("Creating guild_config table if it doesn't exist already...");
				NpgsqlCommand createGuildConfigTable = new(File.ReadAllText(Path.Join(FileSystem.ProjectRoot, "res/sql/drivers/postgresql/guild_config_table.sql")), _connection);
				_ = createGuildConfigTable.ExecuteNonQuery();
				createGuildConfigTable.Dispose();
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
			_logger.Debug($"Preparing {StatementType.InsertGuildId}...");
			NpgsqlCommand insertGuildId = new("INSERT INTO guild_config(guild_id) VALUES(@guildId)", _connection);
			_ = insertGuildId.Parameters.Add(new("guildId", NpgsqlDbType.Bigint));
			insertGuildId.Prepare();
			_preparedStatements.Add(StatementType.InsertGuildId, insertGuildId);

			_logger.Debug($"Preparing {StatementType.GuildIdExists}...");
			NpgsqlCommand guildIdExists = new("SELECT exists(select 1 FROM guild_config WHERE guild_id=@guildId)", _connection);
			_ = guildIdExists.Parameters.Add(new("guildId", NpgsqlDbType.Bigint));
			guildIdExists.Prepare();
			_preparedStatements.Add(StatementType.GuildIdExists, guildIdExists);

			_logger.Debug($"Preparing {StatementType.GetAntiInvite}...");
			NpgsqlCommand getAntiInvite = new("SELECT anti_invite FROM guild_config WHERE guild_id=@guildId", _connection);
			_ = getAntiInvite.Parameters.Add(new("guildId", NpgsqlDbType.Bigint));
			getAntiInvite.Prepare();
			_preparedStatements.Add(StatementType.GetAntiInvite, getAntiInvite);

			_logger.Debug($"Preparing {StatementType.SetAntiInvite}...");
			NpgsqlCommand setAntiInvite = new("UPDATE guild_config SET anti_invite=@isEnabled WHERE guild_id=@guildId", _connection);
			_ = setAntiInvite.Parameters.Add(new("isEnabled", NpgsqlDbType.Boolean));
			_ = setAntiInvite.Parameters.Add(new("guildId", NpgsqlDbType.Bigint));
			setAntiInvite.Prepare();
			_preparedStatements.Add(StatementType.SetAntiInvite, setAntiInvite);

			_logger.Debug($"Preparing {StatementType.GetAllowedInvites}...");
			NpgsqlCommand getAllowedInvites = new("SELECT allowed_invites FROM guild_config WHERE guild_id=@guildId", _connection);
			_ = getAllowedInvites.Parameters.Add(new("guildId", NpgsqlDbType.Bigint));
			getAllowedInvites.Prepare();
			_preparedStatements.Add(StatementType.GetAllowedInvites, getAllowedInvites);

			_logger.Debug($"Preparing {StatementType.SetAllowedInvites}...");
			NpgsqlCommand setAllowedInvites = new("UPDATE guild_config SET allowed_invites=@allowedInvites WHERE guild_id=@guildId", _connection);
			_ = setAllowedInvites.Parameters.Add(new("allowedInvites", NpgsqlDbType.Array | NpgsqlDbType.Text));
			_ = setAllowedInvites.Parameters.Add(new("guildId", NpgsqlDbType.Bigint));
			setAllowedInvites.Prepare();
			_preparedStatements.Add(StatementType.SetAllowedInvites, setAllowedInvites);

			_logger.Debug($"Preparing {StatementType.AddAllowedInvite}...");
			NpgsqlCommand addAllowedInvite = new("UPDATE guild_config SET allowed_invites=array_append(allowed_invites, @invite) WHERE guild_id=@guildId", _connection);
			_ = addAllowedInvite.Parameters.Add(new("invite", NpgsqlDbType.Text));
			_ = addAllowedInvite.Parameters.Add(new("guildId", NpgsqlDbType.Bigint));
			addAllowedInvite.Prepare();
			_preparedStatements.Add(StatementType.AddAllowedInvite, addAllowedInvite);

			_logger.Debug($"Preparing {StatementType.RemoveAllowedInvite}...");
			NpgsqlCommand removeAllowedInvite = new("UPDATE guild_config SET allowed_invites=array_remove(allowed_invites, @invite) WHERE guild_id=@guildId", _connection);
			_ = removeAllowedInvite.Parameters.Add(new("invite", NpgsqlDbType.Text));
			_ = removeAllowedInvite.Parameters.Add(new("guildId", NpgsqlDbType.Bigint));
			removeAllowedInvite.Prepare();
			_preparedStatements.Add(StatementType.RemoveAllowedInvite, removeAllowedInvite);

			_logger.Debug($"Preparing {StatementType.IsAllowedInvite}...");
			NpgsqlCommand isAllowedInvite = new("SELECT allowed_invites && @invite::text[] FROM guild_config WHERE guild_id=@guildId", _connection);
			_ = isAllowedInvite.Parameters.Add(new("invite", NpgsqlDbType.Text));
			_ = isAllowedInvite.Parameters.Add(new("guildId", NpgsqlDbType.Bigint));
			isAllowedInvite.Prepare();
			_preparedStatements.Add(StatementType.IsAllowedInvite, isAllowedInvite);

			_logger.Debug($"Preparing {StatementType.GetMaxLines}...");
			NpgsqlCommand getMaxLines = new("SELECT max_lines FROM guild_config WHERE guild_id=@guildId", _connection);
			_ = getMaxLines.Parameters.Add(new("guildId", NpgsqlDbType.Bigint));
			getMaxLines.Prepare();
			_preparedStatements.Add(StatementType.GetMaxLines, getMaxLines);

			_logger.Debug($"Preparing {StatementType.SetMaxLines}...");
			NpgsqlCommand setMaxLines = new("UPDATE guild_config SET max_lines=@maxLines WHERE guild_id=@guildId", _connection);
			_ = setMaxLines.Parameters.Add(new("maxLines", NpgsqlDbType.Integer));
			_ = setMaxLines.Parameters.Add(new("guildId", NpgsqlDbType.Bigint));
			setMaxLines.Prepare();
			_preparedStatements.Add(StatementType.SetMaxLines, setMaxLines);

			_logger.Debug($"Preparing {StatementType.GetMaxMentions}...");
			NpgsqlCommand getMaxMentions = new("SELECT max_mentions FROM guild_config WHERE guild_id=@guildId", _connection);
			_ = getMaxMentions.Parameters.Add(new("guildId", NpgsqlDbType.Bigint));
			getMaxMentions.Prepare();
			_preparedStatements.Add(StatementType.GetMaxMentions, getMaxMentions);

			_logger.Debug($"Preparing {StatementType.SetMaxMentions}...");
			NpgsqlCommand setMaxMentions = new("UPDATE guild_config SET max_mentions=@maxMentions WHERE guild_id=@guildId", _connection);
			_ = setMaxMentions.Parameters.Add(new("maxMentions", NpgsqlDbType.Integer));
			_ = setMaxMentions.Parameters.Add(new("guildId", NpgsqlDbType.Bigint));
			setMaxMentions.Prepare();
			_preparedStatements.Add(StatementType.SetMaxMentions, setMaxMentions);

			_logger.Debug($"Preparing {StatementType.GetAutoDehoist}...");
			NpgsqlCommand getAutoDehoist = new("SELECT auto_dehoist FROM guild_config WHERE guild_id=@guildId", _connection);
			_ = getAutoDehoist.Parameters.Add(new("guildId", NpgsqlDbType.Bigint));
			getAutoDehoist.Prepare();
			_preparedStatements.Add(StatementType.GetAutoDehoist, getAutoDehoist);

			_logger.Debug($"Preparing {StatementType.SetAutoDehoist}...");
			NpgsqlCommand setAutoDehoist = new("UPDATE guild_config SET auto_dehoist=@autoDehoist WHERE guild_id=@guildId", _connection);
			_ = setAutoDehoist.Parameters.Add(new("autoDehoist", NpgsqlDbType.Boolean));
			_ = setAutoDehoist.Parameters.Add(new("guildId", NpgsqlDbType.Bigint));
			setAutoDehoist.Prepare();
			_preparedStatements.Add(StatementType.SetAutoDehoist, setAutoDehoist);

			_logger.Debug($"Preparing {StatementType.GetAutoRaidMode}...");
			NpgsqlCommand getAutoRaidMode = new("SELECT auto_raidmode FROM guild_config WHERE guild_id=@guildId", _connection);
			_ = getAutoRaidMode.Parameters.Add(new("guildId", NpgsqlDbType.Bigint));
			getAutoRaidMode.Prepare();
			_preparedStatements.Add(StatementType.GetAutoRaidMode, getAutoRaidMode);

			_logger.Debug($"Preparing {StatementType.SetAutoRaidMode}...");
			NpgsqlCommand setAutoRaidMode = new("UPDATE guild_config SET auto_raidmode=@autoRaidMode WHERE guild_id=@guildId", _connection);
			_ = setAutoRaidMode.Parameters.Add(new("autoRaidMode", NpgsqlDbType.Boolean));
			_ = setAutoRaidMode.Parameters.Add(new("guildId", NpgsqlDbType.Bigint));
			setAutoRaidMode.Prepare();
			_preparedStatements.Add(StatementType.SetAutoRaidMode, setAutoRaidMode);

			_logger.Debug($"Preparing {StatementType.GetIgnoredChannels}...");
			NpgsqlCommand getIgnoredChannels = new("SELECT ignored_channels FROM guild_config WHERE guild_id=@guildId", _connection);
			_ = getIgnoredChannels.Parameters.Add(new("guildId", NpgsqlDbType.Bigint));
			getIgnoredChannels.Prepare();
			_preparedStatements.Add(StatementType.GetIgnoredChannels, getIgnoredChannels);

			_logger.Debug($"Preparing {StatementType.SetIgnoredChannels}...");
			NpgsqlCommand setIgnoredChannels = new("UPDATE guild_config SET ignored_channels=@ignoredChannels WHERE guild_id=@guildId", _connection);
			_ = setIgnoredChannels.Parameters.Add(new("ignoredChannels", NpgsqlDbType.Array | NpgsqlDbType.Bigint));
			_ = setIgnoredChannels.Parameters.Add(new("guildId", NpgsqlDbType.Bigint));
			setIgnoredChannels.Prepare();
			_preparedStatements.Add(StatementType.SetIgnoredChannels, setIgnoredChannels);

			_logger.Debug($"Preparing {StatementType.AddIgnoredChannel}...");
			NpgsqlCommand addIgnoredChannel = new("UPDATE guild_config SET ignored_channels=array_append(ignored_channels, @channelId) WHERE guild_id=@guildId", _connection);
			_ = addIgnoredChannel.Parameters.Add(new("channelId", NpgsqlDbType.Bigint));
			_ = addIgnoredChannel.Parameters.Add(new("guildId", NpgsqlDbType.Bigint));
			addIgnoredChannel.Prepare();
			_preparedStatements.Add(StatementType.AddIgnoredChannel, addIgnoredChannel);

			_logger.Debug($"Preparing {StatementType.RemoveIgnoredChannel}...");
			NpgsqlCommand removeIgnoredChannel = new("UPDATE guild_config SET ignored_channels=array_remove(ignored_channels, @channelId) WHERE guild_id=@guildId", _connection);
			_ = removeIgnoredChannel.Parameters.Add(new("channelId", NpgsqlDbType.Bigint));
			_ = removeIgnoredChannel.Parameters.Add(new("guildId", NpgsqlDbType.Bigint));
			removeIgnoredChannel.Prepare();
			_preparedStatements.Add(StatementType.RemoveIgnoredChannel, removeIgnoredChannel);

			_logger.Debug($"Preparing {StatementType.IsIgnoredChannel}...");
			NpgsqlCommand isIgnoredChannel = new("SELECT ignored_channels && ARRAY[@channelId] FROM guild_config WHERE guild_id=@guildId", _connection);
			_ = isIgnoredChannel.Parameters.Add(new("channelid", NpgsqlDbType.Bigint));
			_ = isIgnoredChannel.Parameters.Add(new("guildId", NpgsqlDbType.Bigint));
			isIgnoredChannel.Prepare();
			_preparedStatements.Add(StatementType.IsIgnoredChannel, isIgnoredChannel);

			_logger.Debug($"Preparing {StatementType.GetAdminRoles}...");
			NpgsqlCommand getAdminRoles = new("SELECT administraitive_roles FROM guild_config WHERE guild_id=@guildId", _connection);
			_ = getAdminRoles.Parameters.Add(new("guildId", NpgsqlDbType.Bigint));
			getAdminRoles.Prepare();
			_preparedStatements.Add(StatementType.GetAdminRoles, getAdminRoles);

			_logger.Debug($"Preparing {StatementType.SetAdminRoles}...");
			NpgsqlCommand setAdminRoles = new("UPDATE guild_config SET administraitive_roles=@roleIds WHERE guild_id=@guildId", _connection);
			_ = setAdminRoles.Parameters.Add(new("roleIds", NpgsqlDbType.Array | NpgsqlDbType.Bigint));
			_ = setAdminRoles.Parameters.Add(new("guildId", NpgsqlDbType.Bigint));
			setAdminRoles.Prepare();
			_preparedStatements.Add(StatementType.SetAdminRoles, setAdminRoles);

			_logger.Debug($"Preparing {StatementType.AddAdminRole}...");
			NpgsqlCommand addAdminRole = new("UPDATE guild_config SET administraitive_roles=array_append(administraitive_roles, @roleId) WHERE guild_id=@guildId", _connection);
			_ = addAdminRole.Parameters.Add(new("roleId", NpgsqlDbType.Bigint));
			_ = addAdminRole.Parameters.Add(new("guildId", NpgsqlDbType.Bigint));
			addAdminRole.Prepare();
			_preparedStatements.Add(StatementType.AddAdminRole, addAdminRole);

			_logger.Debug($"Preparing {StatementType.RemoveAdminRole}...");
			NpgsqlCommand removeAdminRole = new("UPDATE guild_config SET administraitive_roles=array_remove(administraitive_roles, @roleId) WHERE guild_id=@guildId", _connection);
			_ = removeAdminRole.Parameters.Add(new("roleId", NpgsqlDbType.Bigint));
			_ = removeAdminRole.Parameters.Add(new("guildId", NpgsqlDbType.Bigint));
			removeAdminRole.Prepare();
			_preparedStatements.Add(StatementType.RemoveAdminRole, removeAdminRole);

			_logger.Debug($"Preparing {StatementType.IsAdminRole}...");
			NpgsqlCommand isAdminRole = new("SELECT administraitive_roles && ARRAY[@roleId] FROM guild_config WHERE guild_id=@guildId", _connection);
			_ = isAdminRole.Parameters.Add(new("roleId", NpgsqlDbType.Bigint));
			_ = isAdminRole.Parameters.Add(new("guildId", NpgsqlDbType.Bigint));
			isAdminRole.Prepare();
			_preparedStatements.Add(StatementType.IsAdminRole, isAdminRole);

			_logger.Debug($"Preparing {StatementType.GetMuteRole}...");
			NpgsqlCommand getMuteRole = new("SELECT mute_role FROM guild_config WHERE guild_id=@guildId", _connection);
			_ = getMuteRole.Parameters.Add(new("guildId", NpgsqlDbType.Bigint));
			getMuteRole.Prepare();
			_preparedStatements.Add(StatementType.GetMuteRole, getMuteRole);

			_logger.Debug($"Preparing {StatementType.SetMuteRole}...");
			NpgsqlCommand setMuteRole = new("UPDATE guild_config SET mute_role=@roleId WHERE guild_id=@guildId", _connection);
			_ = setMuteRole.Parameters.Add(new("roleId", NpgsqlDbType.Bigint));
			_ = setMuteRole.Parameters.Add(new("guildId", NpgsqlDbType.Bigint));
			setMuteRole.Prepare();
			_preparedStatements.Add(StatementType.SetMuteRole, setMuteRole);

			_logger.Debug($"Preparing {StatementType.GetAntiMemeRole}...");
			NpgsqlCommand getAntiMemeRole = new("SELECT antimeme_role FROM guild_config WHERE guild_id=@guildId", _connection);
			_ = getAntiMemeRole.Parameters.Add(new("guildId", NpgsqlDbType.Bigint));
			getAntiMemeRole.Prepare();
			_preparedStatements.Add(StatementType.GetAntiMemeRole, getAntiMemeRole);

			_logger.Debug($"Preparing {StatementType.SetAntiMemeRole}...");
			NpgsqlCommand SetAntiMemeRole = new("UPDATE guild_config SET antimeme_role=@roleId WHERE guild_id=@guildId", _connection);
			_ = SetAntiMemeRole.Parameters.Add(new("roleId", NpgsqlDbType.Bigint));
			_ = SetAntiMemeRole.Parameters.Add(new("guildId", NpgsqlDbType.Bigint));
			SetAntiMemeRole.Prepare();
			_preparedStatements.Add(StatementType.SetAntiMemeRole, SetAntiMemeRole);

			_logger.Debug($"Preparing {StatementType.GetNoVCRole}...");
			NpgsqlCommand getVoiceBanRole = new("SELECT voice_ban_role FROM guild_config WHERE guild_id=@guildId", _connection);
			_ = getVoiceBanRole.Parameters.Add(new("guildId", NpgsqlDbType.Bigint));
			getVoiceBanRole.Prepare();
			_preparedStatements.Add(StatementType.GetNoVCRole, getVoiceBanRole);

			_logger.Debug($"Preparing {StatementType.SetNoVCRole}...");
			NpgsqlCommand setVoiceBanRole = new("UPDATE guild_config SET voice_ban_role=@roleId WHERE guild_id=@guildId", _connection);
			_ = setVoiceBanRole.Parameters.Add(new("roleId", NpgsqlDbType.Bigint));
			_ = setVoiceBanRole.Parameters.Add(new("guildId", NpgsqlDbType.Bigint));
			setVoiceBanRole.Prepare();
			_preparedStatements.Add(StatementType.SetNoVCRole, setVoiceBanRole);

			_logger.Debug($"Preparing {StatementType.GetAntiRaid}...");
			NpgsqlCommand getAntiRaid = new("SELECT antiraid FROM guild_config WHERE guild_id=@guildId", _connection);
			_ = getAntiRaid.Parameters.Add(new("guildId", NpgsqlDbType.Bigint));
			getAntiRaid.Prepare();
			_preparedStatements.Add(StatementType.GetAntiRaid, getAntiRaid);

			_logger.Debug($"Preparing {StatementType.SetAntiRaid}...");
			NpgsqlCommand setAntiRaid = new("UPDATE guild_config SET antiraid=@isEnabled WHERE guild_id=@guildId", _connection);
			_ = setAntiRaid.Parameters.Add(new("isEnabled", NpgsqlDbType.Boolean));
			_ = setAntiRaid.Parameters.Add(new("guildId", NpgsqlDbType.Bigint));
			setAntiRaid.Prepare();
			_preparedStatements.Add(StatementType.SetAntiRaid, setAntiRaid);

			_logger.Debug($"Preparing {StatementType.GetAntiRaidSetOff}...");
			NpgsqlCommand getAntiRaidSetOff = new("SELECT antiraid_setoff FROM guild_config WHERE guild_id=@guildId", _connection);
			_ = getAntiRaidSetOff.Parameters.Add(new("guildId", NpgsqlDbType.Bigint));
			getAntiRaidSetOff.Prepare();
			_preparedStatements.Add(StatementType.GetAntiRaidSetOff, getAntiRaidSetOff);

			_logger.Debug($"Preparing {StatementType.SetAntiRaidSetOff}...");
			NpgsqlCommand setAntiRaidSetOff = new("UPDATE guild_config SET antiraid_setoff=@interval WHERE guild_id=@guildId", _connection);
			_ = setAntiRaidSetOff.Parameters.Add(new("interval", NpgsqlDbType.Integer));
			_ = setAntiRaidSetOff.Parameters.Add(new("guildId", NpgsqlDbType.Bigint));
			setAntiRaidSetOff.Prepare();
			_preparedStatements.Add(StatementType.SetAntiRaidSetOff, setAntiRaidSetOff);
		}

		public void Dispose()
		{
			_preparedStatements.Clear();
			_connection.Close();
			_connection.Dispose();
			GC.SuppressFinalize(this);
		}

		public void InsertGuildId(ulong guildId) => ExecuteQuery(StatementType.InsertGuildId, new NpgsqlParameter("guildId", (long)guildId));
		public bool GuildIdExists(ulong guildId) => ExecuteQuery(StatementType.GuildIdExists, new NpgsqlParameter("guildId", (long)guildId), true)?[0][0];

		public bool AntiInvite(ulong guildId) => ExecuteQuery(StatementType.GetAntiInvite, new NpgsqlParameter("guildId", (long)guildId), true)?[0][0];
		public void AntiInvite(ulong guildId, bool isEnabled) => ExecuteQuery(StatementType.SetAntiInvite, new List<NpgsqlParameter>() { new NpgsqlParameter("guildId", (long)guildId), new NpgsqlParameter("isEnabled", isEnabled) });

		public string[] AllowedInvites(ulong guildId) => ExecuteQuery(StatementType.GetAllowedInvites, new NpgsqlParameter("guildId", (long)guildId), true)?[0].ConvertAll<string>(invite => invite.ToString()).ToArray();
		public void AllowedInvites(ulong guildId, string[] allowedInvites) => ExecuteQuery(StatementType.SetAllowedInvites, new List<NpgsqlParameter>() { new NpgsqlParameter("guildId", (long)guildId), new NpgsqlParameter("allowedInvites", allowedInvites) });
		public void AddAllowedInvite(ulong guildId, string invite) => ExecuteQuery(StatementType.AddAllowedInvite, new List<NpgsqlParameter>() { new NpgsqlParameter("guildId", (long)guildId), new NpgsqlParameter("invite", invite) });
		public void RemoveAllowedInvite(ulong guildId, string invite) => ExecuteQuery(StatementType.RemoveAllowedInvite, new List<NpgsqlParameter>() { new NpgsqlParameter("guildId", (long)guildId), new NpgsqlParameter("invite", invite) });
		public bool IsAllowedInvite(ulong guildId, string invite) => ExecuteQuery(StatementType.IsAllowedInvite, new List<NpgsqlParameter>() { new NpgsqlParameter("guildId", (long)guildId), new NpgsqlParameter("invite", invite) }, true)?[0][0];

		public int MaxLines(ulong guildId) => ExecuteQuery(StatementType.GetMaxLines, new NpgsqlParameter("guildId", (long)guildId), true)?[0][0];
		public void MaxLines(ulong guildId, int maxLineCount) => ExecuteQuery(StatementType.SetMaxLines, new List<NpgsqlParameter>() { new NpgsqlParameter("guildId", (long)guildId), new NpgsqlParameter("maxLines", maxLineCount) }, true);

		public int MaxMentions(ulong guildId) => ExecuteQuery(StatementType.GetMaxMentions, new NpgsqlParameter("guildId", (long)guildId), true)?[0][0];
		public void MaxMentions(ulong guildId, int maxMentionCount) => ExecuteQuery(StatementType.SetMaxMentions, new List<NpgsqlParameter>() { new NpgsqlParameter("guildId", (long)guildId), new NpgsqlParameter("maxMentions", maxMentionCount) });

		public bool AutoDehoist(ulong guildId) => ExecuteQuery(StatementType.GetAutoDehoist, new NpgsqlParameter("guildId", (long)guildId), true)?[0][0];
		public void AutoDehoist(ulong guildId, bool isEnabled) => ExecuteQuery(StatementType.SetAutoDehoist, new List<NpgsqlParameter>() { new NpgsqlParameter("guildId", (long)guildId), new NpgsqlParameter("isEnabled", isEnabled) });

		public bool AutoRaidMode(ulong guildId) => ExecuteQuery(StatementType.GetAutoRaidMode, new NpgsqlParameter("guildId", (long)guildId), true)?[0][0];
		public void AutoRaidMode(ulong guildId, bool isEnabled) => ExecuteQuery(StatementType.SetAutoRaidMode, new List<NpgsqlParameter>() { new NpgsqlParameter("guildId", (long)guildId), new NpgsqlParameter("isEnabled", isEnabled) });

		public ulong[] IgnoredChannels(ulong guildId) => ExecuteQuery(StatementType.GetIgnoredChannels, new NpgsqlParameter("guildId", (long)guildId), true)?[0].ConvertAll<ulong>(channelId => ulong.Parse(channelId)).ToArray();
		public void IgnoredChannels(ulong guildId, ulong[] channelIds) => ExecuteQuery(StatementType.SetIgnoredChannels, new List<NpgsqlParameter>() { new NpgsqlParameter("guildId", (long)guildId), new NpgsqlParameter("channelIds", channelIds.Select((role) => long.Parse(role.ToString()))) });
		public void AddIgnoredChannel(ulong guildId, ulong channelId) => ExecuteQuery(StatementType.AddIgnoredChannel, new List<NpgsqlParameter>() { new NpgsqlParameter("guildId", (long)guildId), new NpgsqlParameter("channelId", (long)channelId) });
		public void RemoveIgnoredChannel(ulong guildId, ulong channelId) => ExecuteQuery(StatementType.RemoveIgnoredChannel, new List<NpgsqlParameter>() { new NpgsqlParameter("guildId", (long)guildId), new NpgsqlParameter("channelId", (long)channelId) });
		public bool IsIgnoredChannel(ulong guildId, ulong channelId) => ExecuteQuery(StatementType.IsIgnoredChannel, new List<NpgsqlParameter>() { new NpgsqlParameter("guildId", (long)guildId), new NpgsqlParameter("channelId", (long)channelId) }, true)?[0][0];

		public ulong[] AdminRoles(ulong guildId) => ExecuteQuery(StatementType.GetAdminRoles, new NpgsqlParameter("guildId", (long)guildId), true)?[0].ConvertAll<ulong>(roleId => ulong.Parse(roleId)).ToArray();
		public void AdminRoles(ulong guildId, ulong[] roleIds) => ExecuteQuery(StatementType.SetAdminRoles, new List<NpgsqlParameter>() { new NpgsqlParameter("guildId", (long)guildId), new NpgsqlParameter("roleIds", roleIds.Select((role) => long.Parse(role.ToString()))) });
		public void AddAdminRole(ulong guildId, ulong roleId) => ExecuteQuery(StatementType.AddAdminRole, new List<NpgsqlParameter>() { new NpgsqlParameter("guildId", (long)guildId), new NpgsqlParameter("roleId", (long)roleId) });
		public void RemoveAdminRole(ulong guildId, ulong roleId) => ExecuteQuery(StatementType.RemoveAdminRole, new List<NpgsqlParameter>() { new NpgsqlParameter("guildId", (long)guildId), new NpgsqlParameter("roleId", (long)roleId) });
		public bool IsAdminRole(ulong guildId, ulong roleId) => ExecuteQuery(StatementType.IsAdminRole, new List<NpgsqlParameter>() { new NpgsqlParameter("guildId", (long)guildId), new NpgsqlParameter("roleId", (long)roleId) }, true)?[0][0];

		public ulong? MuteRole(ulong guildId) => (ulong?)ExecuteQuery(StatementType.GetMuteRole, new NpgsqlParameter("guildId", (long)guildId), true)?[0][0];

		public void MuteRole(ulong guildId, ulong roleId) => ExecuteQuery(StatementType.SetMuteRole, new List<NpgsqlParameter>() { new NpgsqlParameter("guildId", (long)guildId), new NpgsqlParameter("roleId", (long)roleId) });

		public ulong? AntimemeRole(ulong guildId) => (ulong?)ExecuteQuery(StatementType.GetAntiMemeRole, new NpgsqlParameter("guildId", (long)guildId), true)?[0][0];
		public void AntiMemeRole(ulong guildId, ulong roleId) => ExecuteQuery(StatementType.SetAntiMemeRole, new List<NpgsqlParameter>() { new NpgsqlParameter("guildId", (long)guildId), new NpgsqlParameter("roleId", (long)roleId) });

		public ulong? VoiceBanRole(ulong guildId) => (ulong?)ExecuteQuery(StatementType.GetNoVCRole, new NpgsqlParameter("guildId", (long)guildId), true)?[0][0];
		public void VoiceBanRole(ulong guildId, ulong roleId) => ExecuteQuery(StatementType.SetNoVCRole, new List<NpgsqlParameter>() { new NpgsqlParameter("guildId", (long)guildId), new NpgsqlParameter("roleId", (long)roleId) });

		public bool AntiRaid(ulong guildId) => ExecuteQuery(StatementType.GetAntiRaid, new NpgsqlParameter("guildId", (long)guildId), true)?[0][0];
		public void AntiRaid(ulong guildId, bool isEnabled) => ExecuteQuery(StatementType.SetAntiRaid, new List<NpgsqlParameter>() { new NpgsqlParameter("guildId", (long)guildId), new NpgsqlParameter("isEnabled", isEnabled) });

		public int AntiRaidSetOff(ulong guildId) => ExecuteQuery(StatementType.GetAntiRaidSetOff, new NpgsqlParameter("guildId", (long)guildId), true)?[0][0];
		public void AntiRaidSetOff(ulong guildId, int interval) => ExecuteQuery(StatementType.SetAntiRaidSetOff, new List<NpgsqlParameter>() { new NpgsqlParameter("guildId", (long)guildId), new NpgsqlParameter("interval", interval) });
	}
}
