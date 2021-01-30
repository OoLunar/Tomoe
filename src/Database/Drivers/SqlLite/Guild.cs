using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Microsoft.Data.Sqlite;

using Tomoe.Database.Interfaces;
using Tomoe.Utils;

namespace Tomoe.Database.Drivers.Sqlite
{
	public class SqliteGuild : IGuild
	{
		private static readonly Logger _logger = new("Database.PostgresSQL.Guild");
		private readonly SqliteConnection _connection;
		private readonly Dictionary<StatementType, SqliteCommand> _preparedStatements = new();
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
		/// <param name="parameters">A list of <see cref="SqliteParameter">SqliteParameter's</see>.</param>
		/// <param name="needsResult">Returns a list of results if true, otherwise returns null.</param>
		/// <returns><see cref="List{T}">List&lt;dynamic&gt;</see> if <paramref name="needsResult">needsResult</paramref> is true, otherwise returns null.</returns>
		private Dictionary<int, List<dynamic>> ExecuteQuery(StatementType command, List<SqliteParameter> parameters, bool needsResult = false)
		{
			SqliteCommand statement = _preparedStatements[command];
			if (statement.Parameters.Count != parameters.Count) throw new SqliteException("Prepared parameters count do not line up with given parameters count.", 1);
			Dictionary<string, SqliteParameter> sortedParameters = new();
			foreach (SqliteParameter parameter in parameters) sortedParameters.Add(parameter.ParameterName, parameter);
			foreach (SqliteParameter parameter in statement.Parameters) parameter.Value = sortedParameters[parameter.ParameterName].Value;
			string parameterValues = string.Join(", ", sortedParameters.Select(param => param.Value.Value));
			_logger.Trace($"Executing prepared statement \"{command}\" with parameters: {(parameterValues == string.Empty ? "None" : parameterValues)}");
			try
			{
				if (needsResult)
				{
					SqliteDataReader reader = statement.ExecuteReader();
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
					if (values.Count == 0 || (values.Count == 1 && values[0] == null)) values = null;
					return values;
				}
				else
				{
					_ = statement.ExecuteNonQuery();
					return null;
				}
			}
			catch (SqliteException error) when (error.ErrorCode == 11)
			{
				_logger.Critical("Database image is malformed!");
				return null;
			}
		}

		private Dictionary<int, List<dynamic>> ExecuteQuery(StatementType command, SqliteParameter parameter, bool needsResult = false) => ExecuteQuery(command, new List<SqliteParameter>() { parameter }, needsResult);

		public SqliteGuild(string password, string databaseName, SqliteOpenMode openMode, SqliteCacheMode cacheMode)
		{
			SqliteConnectionStringBuilder connectionString = new();
			connectionString.Mode = openMode;
			connectionString.Cache = cacheMode;
			connectionString.DataSource = databaseName;
			connectionString.Password = password;
			_connection = new(connectionString.ToString());
			_logger.Info("Opening connection to database...");
			_connection.Open();
			_logger.Debug("Creating strikes table if it doesn't exist...");
			SqliteCommand createStrikeTable = new(File.ReadAllText(Path.Join(FileSystem.ProjectRoot, "res/sql/drivers/sqlite/guild_config_table.sql")), _connection);
			_ = createStrikeTable.ExecuteNonQuery();
			createStrikeTable.Dispose();

			_logger.Info("Preparing SQL commands...");
			_logger.Debug($"Preparing {StatementType.InsertGuildId}...");
			SqliteCommand insertGuildId = new("INSERT INTO guild_config(guild_id) VALUES(@guildId)", _connection);
			_ = insertGuildId.Parameters.Add(new("@guildId", SqliteType.Integer, sizeof(ulong), "guild_id"));
			insertGuildId.Prepare();
			_preparedStatements.Add(StatementType.InsertGuildId, insertGuildId);

			_logger.Debug($"Preparing {StatementType.GuildIdExists}...");
			SqliteCommand guildIdExists = new("SELECT true FROM guild_config WHERE guild_id=@guildId", _connection);
			_ = guildIdExists.Parameters.Add(new("@guildId", SqliteType.Integer, sizeof(ulong), "guild_id"));
			guildIdExists.Prepare();
			_preparedStatements.Add(StatementType.GuildIdExists, guildIdExists);

			_logger.Debug($"Preparing {StatementType.GetAntiInvite}...");
			SqliteCommand getAntiInvite = new("SELECT anti_invite FROM guild_config WHERE guild_id=@guildId", _connection);
			_ = getAntiInvite.Parameters.Add(new("@guildId", SqliteType.Integer, sizeof(ulong), "guild_id"));
			getAntiInvite.Prepare();
			_preparedStatements.Add(StatementType.GetAntiInvite, getAntiInvite);

			_logger.Debug($"Preparing {StatementType.SetAntiInvite}...");
			SqliteCommand setAntiInvite = new("UPDATE guild_config SET anti_invite=@isEnabled WHERE guild_id=@guildId", _connection);
			_ = setAntiInvite.Parameters.Add(new("@isEnabled", SqliteType.Integer, sizeof(bool), "is_enabled"));
			_ = setAntiInvite.Parameters.Add(new("@guildId", SqliteType.Integer, sizeof(ulong), "guild_id"));
			setAntiInvite.Prepare();
			_preparedStatements.Add(StatementType.SetAntiInvite, setAntiInvite);

			_logger.Debug($"Preparing {StatementType.GetAllowedInvites}...");
			SqliteCommand getAllowedInvites = new("SELECT allowed_invites FROM guild_config WHERE guild_id=@guildId", _connection);
			_ = getAllowedInvites.Parameters.Add(new("@guildId", SqliteType.Integer, sizeof(ulong), "guild_id"));
			getAllowedInvites.Prepare();
			_preparedStatements.Add(StatementType.GetAllowedInvites, getAllowedInvites);

			_logger.Debug($"Preparing {StatementType.SetAllowedInvites}...");
			SqliteCommand setAllowedInvites = new("UPDATE guild_config SET allowed_invites=@allowedInvites WHERE guild_id=@guildId", _connection);
			_ = setAllowedInvites.Parameters.Add(new("@allowedInvites", SqliteType.Text));
			_ = setAllowedInvites.Parameters.Add(new("@guildId", SqliteType.Integer, sizeof(ulong), "guild_id"));
			setAllowedInvites.Prepare();
			_preparedStatements.Add(StatementType.SetAllowedInvites, setAllowedInvites);

			_logger.Debug($"Preparing {StatementType.IsAllowedInvite}...");
			SqliteCommand isAllowedInvite = new("SELECT exists(SELECT * FROM guild_config WHERE allowed_invites LIKE '%@invite%' AND guild_id=@guildId)", _connection);
			_ = isAllowedInvite.Parameters.Add(new("@invite", SqliteType.Text));
			_ = isAllowedInvite.Parameters.Add(new("@guildId", SqliteType.Integer, sizeof(ulong), "guild_id"));
			isAllowedInvite.Prepare();
			_preparedStatements.Add(StatementType.IsAllowedInvite, isAllowedInvite);

			_logger.Debug($"Preparing {StatementType.GetMaxLines}...");
			SqliteCommand getMaxLines = new("SELECT max_lines FROM guild_config WHERE guild_id=@guildId", _connection);
			_ = getMaxLines.Parameters.Add(new("@guildId", SqliteType.Integer, sizeof(ulong), "guild_id"));
			getMaxLines.Prepare();
			_preparedStatements.Add(StatementType.GetMaxLines, getMaxLines);

			_logger.Debug($"Preparing {StatementType.SetMaxLines}...");
			SqliteCommand setMaxLines = new("UPDATE guild_config SET max_lines=@maxLines WHERE guild_id=@guildId", _connection);
			_ = setMaxLines.Parameters.Add(new("@maxLines", SqliteType.Integer, sizeof(int), "max_lines"));
			_ = setMaxLines.Parameters.Add(new("@guildId", SqliteType.Integer, sizeof(ulong), "guild_id"));
			setMaxLines.Prepare();
			_preparedStatements.Add(StatementType.SetMaxLines, setMaxLines);

			_logger.Debug($"Preparing {StatementType.GetMaxMentions}...");
			SqliteCommand getMaxMentions = new("SELECT max_mentions FROM guild_config WHERE guild_id=@guildId", _connection);
			_ = getMaxMentions.Parameters.Add(new("@guildId", SqliteType.Integer, sizeof(ulong), "guild_id"));
			getMaxMentions.Prepare();
			_preparedStatements.Add(StatementType.GetMaxMentions, getMaxMentions);

			_logger.Debug($"Preparing {StatementType.SetMaxMentions}...");
			SqliteCommand setMaxMentions = new("UPDATE guild_config SET max_mentions=@maxMentions WHERE guild_id=@guildId", _connection);
			_ = setMaxMentions.Parameters.Add(new("@maxMentions", SqliteType.Integer, sizeof(int), "max_mentions"));
			_ = setMaxMentions.Parameters.Add(new("@guildId", SqliteType.Integer, sizeof(ulong), "guild_id"));
			setMaxMentions.Prepare();
			_preparedStatements.Add(StatementType.SetMaxMentions, setMaxMentions);

			_logger.Debug($"Preparing {StatementType.GetAutoDehoist}...");
			SqliteCommand getAutoDehoist = new("SELECT auto_dehoist FROM guild_config WHERE guild_id=@guildId", _connection);
			_ = getAutoDehoist.Parameters.Add(new("@guildId", SqliteType.Integer, sizeof(ulong), "auto_dehoist"));
			getAutoDehoist.Prepare();
			_preparedStatements.Add(StatementType.GetAutoDehoist, getAutoDehoist);

			_logger.Debug($"Preparing {StatementType.SetAutoDehoist}...");
			SqliteCommand setAutoDehoist = new("UPDATE guild_config SET auto_dehoist=@autoDehoist WHERE guild_id=@guildId", _connection);
			_ = setAutoDehoist.Parameters.Add(new("@autoDehoist", SqliteType.Integer, sizeof(bool), "auto_dehoist"));
			_ = setAutoDehoist.Parameters.Add(new("@guildId", SqliteType.Integer, sizeof(ulong), "guild_id"));
			setAutoDehoist.Prepare();
			_preparedStatements.Add(StatementType.SetAutoDehoist, setAutoDehoist);

			_logger.Debug($"Preparing {StatementType.GetAutoRaidMode}...");
			SqliteCommand getAutoRaidMode = new("SELECT auto_raidmode FROM guild_config WHERE guild_id=@guildId", _connection);
			_ = getAutoRaidMode.Parameters.Add(new("@guildId", SqliteType.Integer, sizeof(ulong), "guild_id"));
			getAutoRaidMode.Prepare();
			_preparedStatements.Add(StatementType.GetAutoRaidMode, getAutoRaidMode);

			_logger.Debug($"Preparing {StatementType.SetAutoRaidMode}...");
			SqliteCommand setAutoRaidMode = new("UPDATE guild_config SET auto_raidmode=@autoRaidMode WHERE guild_id=@guildId", _connection);
			_ = setAutoRaidMode.Parameters.Add(new("@autoRaidMode", SqliteType.Integer, sizeof(bool), "auto_raidmode"));
			_ = setAutoRaidMode.Parameters.Add(new("@guildId", SqliteType.Integer, sizeof(ulong), "guild_id"));
			setAutoRaidMode.Prepare();
			_preparedStatements.Add(StatementType.SetAutoRaidMode, setAutoRaidMode);

			_logger.Debug($"Preparing {StatementType.GetIgnoredChannels}...");
			SqliteCommand getIgnoredChannels = new("SELECT ignored_channels FROM guild_config WHERE guild_id=@guildId", _connection);
			_ = getIgnoredChannels.Parameters.Add(new("@guildId", SqliteType.Integer, sizeof(ulong), "guild_id"));
			getIgnoredChannels.Prepare();
			_preparedStatements.Add(StatementType.GetIgnoredChannels, getIgnoredChannels);

			_logger.Debug($"Preparing {StatementType.SetIgnoredChannels}...");
			SqliteCommand setIgnoredChannels = new("UPDATE guild_config SET ignored_channels=@ignoredChannels WHERE guild_id=@guildId", _connection);
			_ = setIgnoredChannels.Parameters.Add(new("@ignoredChannels", SqliteType.Text));
			_ = setIgnoredChannels.Parameters.Add(new("@guildId", SqliteType.Integer, sizeof(ulong), "guild_id"));
			setIgnoredChannels.Prepare();
			_preparedStatements.Add(StatementType.SetIgnoredChannels, setIgnoredChannels);

			_logger.Debug($"Preparing {StatementType.IsIgnoredChannel}...");
			SqliteCommand isIgnoredChannel = new("SELECT exists(SELECT * FROM guild_config WHERE ignored_channels LIKE '%@channelId%' AND guild_id=@guildId)", _connection);
			_ = isIgnoredChannel.Parameters.Add(new("@channelId", SqliteType.Integer, sizeof(ulong), "channel_id"));
			_ = isIgnoredChannel.Parameters.Add(new("@guildId", SqliteType.Integer, sizeof(ulong), "guild_id"));
			isIgnoredChannel.Prepare();
			_preparedStatements.Add(StatementType.IsIgnoredChannel, isIgnoredChannel);

			_logger.Debug($"Preparing {StatementType.GetAdminRoles}...");
			SqliteCommand getAdminRoles = new("SELECT admin_roles FROM guild_config WHERE guild_id=@guildId", _connection);
			_ = getAdminRoles.Parameters.Add(new("@guildId", SqliteType.Integer, sizeof(ulong), "guild_id"));
			getAdminRoles.Prepare();
			_preparedStatements.Add(StatementType.GetAdminRoles, getAdminRoles);

			_logger.Debug($"Preparing {StatementType.SetAdminRoles}...");
			SqliteCommand setAdminRoles = new("UPDATE guild_config SET admin_roles=@roleIds WHERE guild_id=@guildId", _connection);
			_ = setAdminRoles.Parameters.Add(new("@roleIds", SqliteType.Text, sizeof(ulong), "admin_roles"));
			_ = setAdminRoles.Parameters.Add(new("@guildId", SqliteType.Integer, sizeof(ulong), "guild_id"));
			setAdminRoles.Prepare();
			_preparedStatements.Add(StatementType.SetAdminRoles, setAdminRoles);

			_logger.Debug($"Preparing {StatementType.IsAdminRole}...");
			SqliteCommand isAdminRole = new("SELECT exists(SELECT * FROM guild_config WHERE admin_roles LIKE '%@roleId%' AND guild_id=@guildId)", _connection);
			_ = isAdminRole.Parameters.Add(new("@roleId", SqliteType.Integer, sizeof(ulong), "admin_roles"));
			_ = isAdminRole.Parameters.Add(new("@guildId", SqliteType.Integer, sizeof(ulong), "guild_id"));
			isAdminRole.Prepare();
			_preparedStatements.Add(StatementType.IsAdminRole, isAdminRole);

			_logger.Debug($"Preparing {StatementType.GetMuteRole}...");
			SqliteCommand getMuteRole = new("SELECT mute_role FROM guild_config WHERE guild_id=@guildId", _connection);
			_ = getMuteRole.Parameters.Add(new("@guildId", SqliteType.Integer, sizeof(ulong), "guild_id"));
			getMuteRole.Prepare();
			_preparedStatements.Add(StatementType.GetMuteRole, getMuteRole);

			_logger.Debug($"Preparing {StatementType.SetMuteRole}...");
			SqliteCommand setMuteRole = new("UPDATE guild_config SET mute_role=@roleId WHERE guild_id=@guildId", _connection);
			_ = setMuteRole.Parameters.Add(new("@roleId", SqliteType.Integer, sizeof(ulong), "mute_role"));
			_ = setMuteRole.Parameters.Add(new("@guildId", SqliteType.Integer, sizeof(ulong), "guild_id"));
			setMuteRole.Prepare();
			_preparedStatements.Add(StatementType.SetMuteRole, setMuteRole);

			_logger.Debug($"Preparing {StatementType.GetAntiMemeRole}...");
			SqliteCommand getAntiMemeRole = new("SELECT antimeme_role FROM guild_config WHERE guild_id=@guildId", _connection);
			_ = getAntiMemeRole.Parameters.Add(new("@guildId", SqliteType.Integer, sizeof(ulong), "guild_id"));
			getAntiMemeRole.Prepare();
			_preparedStatements.Add(StatementType.GetAntiMemeRole, getAntiMemeRole);

			_logger.Debug($"Preparing {StatementType.SetAntiMemeRole}...");
			SqliteCommand setAntiMemeRole = new("UPDATE guild_config SET antimeme_role=@roleId WHERE guild_id=@guildId", _connection);
			_ = setAntiMemeRole.Parameters.Add(new("@roleId", SqliteType.Integer, sizeof(ulong), "role_id"));
			_ = setAntiMemeRole.Parameters.Add(new("@guildId", SqliteType.Integer, sizeof(ulong), "guild_id"));
			setAntiMemeRole.Prepare();
			_preparedStatements.Add(StatementType.SetAntiMemeRole, setAntiMemeRole);

			_logger.Debug($"Preparing {StatementType.GetNoVCRole}...");
			SqliteCommand getNoVCRole = new("SELECT novc_role FROM guild_config WHERE guild_id=@guildId", _connection);
			_ = getNoVCRole.Parameters.Add(new("@guildId", SqliteType.Integer, sizeof(ulong), "guild_id"));
			getNoVCRole.Prepare();
			_preparedStatements.Add(StatementType.GetNoVCRole, getNoVCRole);

			_logger.Debug($"Preparing {StatementType.SetNoVCRole}...");
			SqliteCommand setNoVCRole = new("UPDATE guild_config SET novc_role=@roleId WHERE guild_id=@guildId", _connection);
			_ = setNoVCRole.Parameters.Add(new("@roleId", SqliteType.Integer, sizeof(ulong), "novc_role"));
			_ = setNoVCRole.Parameters.Add(new("@guildId", SqliteType.Integer, sizeof(ulong), "guild_id"));
			setNoVCRole.Prepare();
			_preparedStatements.Add(StatementType.SetNoVCRole, setNoVCRole);

			_logger.Debug($"Preparing {StatementType.GetAntiRaid}...");
			SqliteCommand getAntiRaid = new("SELECT antiraid FROM guild_config WHERE guild_id=@guildId", _connection);
			_ = getAntiRaid.Parameters.Add(new("@guildId", SqliteType.Integer, sizeof(ulong), "guild_id"));
			getAntiRaid.Prepare();
			_preparedStatements.Add(StatementType.GetAntiRaid, getAntiRaid);

			_logger.Debug($"Preparing {StatementType.SetAntiRaid}...");
			SqliteCommand setAntiRaid = new("UPDATE guild_config SET antiraid=@isEnabled WHERE guild_id=@guildId", _connection);
			_ = setAntiRaid.Parameters.Add(new("@isEnabled", SqliteType.Integer, sizeof(bool), "antiraid"));
			_ = setAntiRaid.Parameters.Add(new("@guildId", SqliteType.Integer, sizeof(ulong), "guild_id"));
			setAntiRaid.Prepare();
			_preparedStatements.Add(StatementType.SetAntiRaid, setAntiRaid);

			_logger.Debug($"Preparing {StatementType.GetAntiRaidSetOff}...");
			SqliteCommand getAntiRaidSetOff = new("SELECT antiraid_setoff FROM guild_config WHERE guild_id=@guildId", _connection);
			_ = getAntiRaidSetOff.Parameters.Add(new("@guildId", SqliteType.Integer, sizeof(ulong), "guild_id"));
			getAntiRaidSetOff.Prepare();
			_preparedStatements.Add(StatementType.GetAntiRaidSetOff, getAntiRaidSetOff);

			_logger.Debug($"Preparing {StatementType.SetAntiRaidSetOff}...");
			SqliteCommand setAntiRaidSetOff = new("UPDATE guild_config SET antiraid_setoff=@interval WHERE guild_id=@guildId", _connection);
			_ = setAntiRaidSetOff.Parameters.Add(new("@interval", SqliteType.Integer, sizeof(int), "antiraid_setoff"));
			_ = setAntiRaidSetOff.Parameters.Add(new("@guildId", SqliteType.Integer, sizeof(ulong), "guild_id"));
			setAntiRaidSetOff.Prepare();
			_preparedStatements.Add(StatementType.SetAntiRaidSetOff, setAntiRaidSetOff);
		}

		public void Dispose()
		{
			_preparedStatements.Clear();
			_connection.Dispose();
			GC.SuppressFinalize(this);
		}

		public void InsertGuildId(ulong guildId) => ExecuteQuery(StatementType.InsertGuildId, new SqliteParameter("@guildId", guildId));
		public bool GuildIdExists(ulong guildId) => ExecuteQuery(StatementType.GuildIdExists, new SqliteParameter("@guildId", guildId), true)?[0][0] != null;

		public bool AntiInvite(ulong guildId) => ExecuteQuery(StatementType.GetAntiInvite, new SqliteParameter("@guildId", guildId), true)[0][0];
		public void AntiInvite(ulong guildId, bool isEnabled) => ExecuteQuery(StatementType.SetAntiInvite, new List<SqliteParameter>() { new SqliteParameter("@guildId", guildId), new SqliteParameter("@isEnabled", isEnabled) });

		public string[] AllowedInvites(ulong guildId) => ExecuteQuery(StatementType.GetAllowedInvites, new SqliteParameter("@guildId", guildId), true)?[0].Cast<string>().ToArray();
		public void AllowedInvites(ulong guildId, string[] allowedInvites) => ExecuteQuery(StatementType.SetAllowedInvites, new List<SqliteParameter>() { new SqliteParameter("@guildId", guildId), new SqliteParameter("@allowedInvites", allowedInvites) });

		public void AddAllowedInvite(ulong guildId, string invite)
		{
			string allowedInvites = ExecuteQuery(StatementType.GetAllowedInvites, new SqliteParameter("@guildId", guildId), true)?[0][0];
			if (allowedInvites == null) _ = ExecuteQuery(StatementType.SetAllowedInvites, new List<SqliteParameter>() { new SqliteParameter("@guildId", guildId), new SqliteParameter("@allowedInvites", invite) });
			else _ = ExecuteQuery(StatementType.SetAllowedInvites, new List<SqliteParameter>() { new SqliteParameter("@guildId", guildId), new SqliteParameter("@allowedInvites", $"{allowedInvites},{invite}") });
		}

		public void RemoveAllowedInvite(ulong guildId, string invite)
		{
			string allowedInvites = ExecuteQuery(StatementType.GetAllowedInvites, new List<SqliteParameter>() { new SqliteParameter("@guildId", guildId) })?[0][0];
			if (allowedInvites == null) return;
			else
			{
				string invites = allowedInvites.Remove(allowedInvites.IndexOf(invite), allowedInvites.Length);
				_ = ExecuteQuery(StatementType.SetAllowedInvites, new List<SqliteParameter>() { new SqliteParameter("@guildId", guildId), new SqliteParameter("@allowedInvites", invites) });
			}
		}

		public bool IsAllowedInvite(ulong guildId, string invite) => ExecuteQuery(StatementType.IsAllowedInvite, new List<SqliteParameter>() { new SqliteParameter("@guildId", guildId), new SqliteParameter("@invite", invite) }, true)[0][0];

		public int MaxLines(ulong guildId) => ExecuteQuery(StatementType.GetMaxLines, new SqliteParameter("@guildId", guildId), true)[0][0];
		public void MaxLines(ulong guildId, int maxLineCount) => ExecuteQuery(StatementType.SetMaxLines, new List<SqliteParameter>() { new SqliteParameter("@guildId", guildId), new SqliteParameter("@maxLines", maxLineCount) }, true);

		public int MaxMentions(ulong guildId) => ExecuteQuery(StatementType.GetMaxMentions, new SqliteParameter("@guildId", guildId), true)[0][0];
		public void MaxMentions(ulong guildId, int maxMentionCount) => ExecuteQuery(StatementType.SetMaxMentions, new List<SqliteParameter>() { new SqliteParameter("@guildId", guildId), new SqliteParameter("@maxMentions", maxMentionCount) });

		public bool AutoDehoist(ulong guildId) => ExecuteQuery(StatementType.GetAutoDehoist, new SqliteParameter("@guildId", guildId), true)[0][0];
		public void AutoDehoist(ulong guildId, bool isEnabled) => ExecuteQuery(StatementType.SetAutoDehoist, new List<SqliteParameter>() { new SqliteParameter("@guildId", guildId), new SqliteParameter("@isEnabled", isEnabled) });

		public bool AutoRaidMode(ulong guildId) => ExecuteQuery(StatementType.GetAutoRaidMode, new SqliteParameter("@guildId", guildId), true)[0][0];
		public void AutoRaidMode(ulong guildId, bool isEnabled) => ExecuteQuery(StatementType.SetAutoRaidMode, new List<SqliteParameter>() { new SqliteParameter("@guildId", guildId), new SqliteParameter("@isEnabled", isEnabled) });

		public ulong[] IgnoredChannels(ulong guildId) => ExecuteQuery(StatementType.GetIgnoredChannels, new SqliteParameter("@guildId", guildId), true)?[0][0].Split(',').Cast<ulong>().ToArray();
		public void IgnoredChannels(ulong guildId, ulong[] channelIds) => ExecuteQuery(StatementType.SetIgnoredChannels, new List<SqliteParameter>() { new SqliteParameter("@guildId", guildId), new SqliteParameter("@channelIds", channelIds.Select((role) => long.Parse(role.ToString()))) });
		public void AddIgnoredChannel(ulong guildId, ulong channelId)
		{
			string currentIgnoredChannels = ExecuteQuery(StatementType.GetAllowedInvites, new SqliteParameter("@guildId", guildId), true)?[0][0];
			if (currentIgnoredChannels == null) _ = ExecuteQuery(StatementType.SetAllowedInvites, new List<SqliteParameter>() { new SqliteParameter("@guildId", guildId), new SqliteParameter("@allowedInvites", channelId) });
			else _ = ExecuteQuery(StatementType.SetAllowedInvites, new List<SqliteParameter>() { new SqliteParameter("@guildId", guildId), new SqliteParameter("@allowedInvites", $"{currentIgnoredChannels},{channelId}") });
		}

		public void RemoveIgnoredChannel(ulong guildId, ulong channelId)
		{
			string currentIgnoredChannels = ExecuteQuery(StatementType.GetAdminRoles, new List<SqliteParameter>() { new SqliteParameter("@guildId", guildId) })?[0][0];
			if (currentIgnoredChannels == null) return;
			else
			{
				string ignoredChannels = currentIgnoredChannels.Remove(currentIgnoredChannels.IndexOf(channelId.ToString()), currentIgnoredChannels.Length);
				_ = ExecuteQuery(StatementType.SetAdminRoles, new List<SqliteParameter>() { new SqliteParameter("@guildId", guildId), new SqliteParameter("@allowedInvites", ignoredChannels) });
			}
		}

		public bool IsIgnoredChannel(ulong guildId, ulong channelId) => ExecuteQuery(StatementType.IsIgnoredChannel, new List<SqliteParameter>() { new SqliteParameter("@guildId", guildId), new SqliteParameter("@channelId", channelId) }, true)[0][0];

		public ulong[] AdminRoles(ulong guildId) => ExecuteQuery(StatementType.GetAdminRoles, new SqliteParameter("@guildId", guildId), true)?[0][0].Split(',').Cast<ulong>().ToArray();
		public void AdminRoles(ulong guildId, ulong[] roleIds) => ExecuteQuery(StatementType.SetAdminRoles, new List<SqliteParameter>() { new SqliteParameter("@guildId", guildId), new SqliteParameter("@roleIds", string.Join(',', roleIds)) });
		public void AddAdminRole(ulong guildId, ulong roleId)
		{
			string currentAdminRoles = ExecuteQuery(StatementType.GetAllowedInvites, new SqliteParameter("@guildId", guildId), true)?[0][0];
			if (currentAdminRoles == null) _ = ExecuteQuery(StatementType.SetAllowedInvites, new List<SqliteParameter>() { new SqliteParameter("@guildId", guildId), new SqliteParameter("@allowedInvites", roleId) });
			else _ = ExecuteQuery(StatementType.SetAllowedInvites, new List<SqliteParameter>() { new SqliteParameter("@guildId", guildId), new SqliteParameter("@allowedInvites", $"{currentAdminRoles},{roleId}") });
		}

		public void RemoveAdminRole(ulong guildId, ulong roleId)
		{
			string currentAdminRoles = ExecuteQuery(StatementType.GetAllowedInvites, new List<SqliteParameter>() { new SqliteParameter("@guildId", guildId) })?[0][0];
			if (currentAdminRoles == null) return;
			else
			{
				string adminRoles = currentAdminRoles.Remove(currentAdminRoles.IndexOf(roleId.ToString()), currentAdminRoles.Length);
				_ = ExecuteQuery(StatementType.SetAllowedInvites, new List<SqliteParameter>() { new SqliteParameter("@guildId", guildId), new SqliteParameter("@allowedInvites", adminRoles) });
			}
		}

		public bool IsAdminRole(ulong guildId, ulong roleId) => ExecuteQuery(StatementType.IsAdminRole, new List<SqliteParameter>() { new SqliteParameter("@guildId", guildId), new SqliteParameter("@roleId", roleId) }, true)[0][0];

		public ulong? MuteRole(ulong guildId) => (ulong?)ExecuteQuery(StatementType.GetMuteRole, new SqliteParameter("@guildId", guildId), true)?[0][0];
		public void MuteRole(ulong guildId, ulong roleId) => ExecuteQuery(StatementType.SetMuteRole, new List<SqliteParameter>() { new SqliteParameter("@guildId", guildId), new SqliteParameter("@roleId", roleId) });

		public ulong? AntimemeRole(ulong guildId) => (ulong?)ExecuteQuery(StatementType.GetAntiMemeRole, new SqliteParameter("@guildId", guildId), true)?[0][0];
		public void AntimemeRole(ulong guildId, ulong roleId) => ExecuteQuery(StatementType.SetAntiMemeRole, new List<SqliteParameter>() { new SqliteParameter("@guildId", guildId), new SqliteParameter("@roleId", roleId) });

		public ulong? VoiceBanRole(ulong guildId) => (ulong?)ExecuteQuery(StatementType.GetNoVCRole, new SqliteParameter("@guildId", guildId), true)?[0][0];
		public void VoiceBanRole(ulong guildId, ulong roleId) => ExecuteQuery(StatementType.SetNoVCRole, new List<SqliteParameter>() { new SqliteParameter("@guildId", guildId), new SqliteParameter("@roleId", roleId) });

		public bool AntiRaid(ulong guildId) => ExecuteQuery(StatementType.GetAntiRaid, new SqliteParameter("@guildId", guildId), true)[0][0];
		public void AntiRaid(ulong guildId, bool isEnabled) => ExecuteQuery(StatementType.SetAntiRaid, new List<SqliteParameter>() { new SqliteParameter("@guildId", guildId), new SqliteParameter("@isEnabled", isEnabled) });

		public int AntiRaidSetOff(ulong guildId) => ExecuteQuery(StatementType.GetAntiRaidSetOff, new SqliteParameter("@guildId", guildId), true)[0][0];
		public void AntiRaidSetOff(ulong guildId, int interval) => ExecuteQuery(StatementType.SetAntiRaidSetOff, new List<SqliteParameter>() { new SqliteParameter("@guildId", guildId), new SqliteParameter("@interval", interval) });
	}
}
