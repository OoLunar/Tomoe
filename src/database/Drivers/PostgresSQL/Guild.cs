using System.Collections.Generic;
using System.IO;
using System.Linq;
using Npgsql;
using NpgsqlTypes;
using Tomoe.Database.Interfaces;
using Tomoe.Utils;
using System;
using System.Net.Sockets;

namespace Tomoe.Database.Drivers.PostgresSQL
{
	public class PostgresGuild : IGuild
	{
		private static readonly Logger Logger = new("Database.PostgresSQL.Guild");
		private readonly NpgsqlConnection Connection;
		private readonly Dictionary<StatementType, NpgsqlCommand> PreparedStatements = new();
		private int retryCount = 0;
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
			GetNoMemeRole,
			SetNoMemeRole,
			GetNoVCRole,
			SetNoVCRole,
			GetAntiRaid,
			SetAntiRaid,
			GetAntiRaidSetOff,
			SetAntiRaidSetOff
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
							Logger.Trace($"Recieved values: {reader[i] ?? "null"} on iteration {i}");
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
				if (retryCount > DatabaseLoader.MaxRetryCount) Logger.Critical($"Failed to execute query \"{command}\" after {retryCount} times. Check your internet connection.");
				else retryCount++;
				Logger.Error($"Socket exception occured, retrying... Details: {error.Message}\n{error.StackTrace}");
				return ExecuteQuery(command, parameters, needsResult);
			}
		}

		/// <inheritdoc cref="ExecuteQuery(StatementType, List{NpgsqlParameter}, bool)" />
		/// <param name="parameter">One <see cref="NpgsqlParameter">NpgsqlParameter</see>, which gets converted into a <see cref="List{T}">List&lt;NpgsqlParameter&gt;</see>.</param>
		private Dictionary<int, List<dynamic>> ExecuteQuery(StatementType command, NpgsqlParameter parameter, bool needsResult = false) => ExecuteQuery(command, new List<NpgsqlParameter> { parameter }, needsResult);

		public PostgresGuild(string host, int port, string username, string password, string databaseName, SslMode sslMode)
		{
			Connection = new($"Host={host};Port={port};Username={username};Password={password};Database={databaseName};SSL Mode={sslMode}");
			Logger.Info("Opening connection to database...");
			try
			{
				Connection.Open();
				Logger.Debug("Creating guild_config table if it doesn't exist already...");
				NpgsqlCommand createGuildConfigTable = new(File.ReadAllText(Path.Join(FileSystem.ProjectRoot, "res/sql/drivers/postgresql/guild_config_table.sql")), Connection);
				_ = createGuildConfigTable.ExecuteNonQuery();
				createGuildConfigTable.Dispose();
			}
			catch (SocketException error)
			{
				Logger.Critical($"Failed to connect to database. {error.Message}", true);
			}
			Logger.Info("Preparing SQL commands...");
			Logger.Debug($"Preparing {StatementType.InsertGuildId}...");
			NpgsqlCommand insertGuildId = new("INSERT INTO guild_config(guild_id) VALUES(@guildId)", Connection);
			_ = insertGuildId.Parameters.Add(new("guildId", NpgsqlDbType.Bigint));
			insertGuildId.Prepare();
			PreparedStatements.Add(StatementType.InsertGuildId, insertGuildId);

			Logger.Debug($"Preparing {StatementType.GuildIdExists}...");
			NpgsqlCommand guildIdExists = new("SELECT exists(select 1 FROM guild_config WHERE guild_id=@guildId)", Connection);
			_ = guildIdExists.Parameters.Add(new("guildId", NpgsqlDbType.Bigint));
			guildIdExists.Prepare();
			PreparedStatements.Add(StatementType.GuildIdExists, guildIdExists);

			Logger.Debug($"Preparing {StatementType.GetAntiInvite}...");
			NpgsqlCommand getAntiInvite = new("SELECT anti_invite FROM guild_config WHERE guild_id=@guildId", Connection);
			_ = getAntiInvite.Parameters.Add(new("guildId", NpgsqlDbType.Bigint));
			getAntiInvite.Prepare();
			PreparedStatements.Add(StatementType.GetAntiInvite, getAntiInvite);

			Logger.Debug($"Preparing {StatementType.SetAntiInvite}...");
			NpgsqlCommand setAntiInvite = new("UPDATE guild_config SET anti_invite=@isEnabled WHERE guild_id=@guildId", Connection);
			_ = setAntiInvite.Parameters.Add(new("isEnabled", NpgsqlDbType.Boolean));
			_ = setAntiInvite.Parameters.Add(new("guildId", NpgsqlDbType.Bigint));
			setAntiInvite.Prepare();
			PreparedStatements.Add(StatementType.SetAntiInvite, setAntiInvite);

			Logger.Debug($"Preparing {StatementType.GetAllowedInvites}...");
			NpgsqlCommand getAllowedInvites = new("SELECT allowed_invites FROM guild_config WHERE guild_id=@guildId", Connection);
			_ = getAllowedInvites.Parameters.Add(new("guildId", NpgsqlDbType.Bigint));
			getAllowedInvites.Prepare();
			PreparedStatements.Add(StatementType.GetAllowedInvites, getAllowedInvites);

			Logger.Debug($"Preparing {StatementType.SetAllowedInvites}...");
			NpgsqlCommand setAllowedInvites = new("UPDATE guild_config SET allowed_invites=@allowedInvites WHERE guild_id=@guildId", Connection);
			_ = setAllowedInvites.Parameters.Add(new("allowedInvites", NpgsqlDbType.Array | NpgsqlDbType.Text));
			_ = setAllowedInvites.Parameters.Add(new("guildId", NpgsqlDbType.Bigint));
			setAllowedInvites.Prepare();
			PreparedStatements.Add(StatementType.SetAllowedInvites, setAllowedInvites);

			Logger.Debug($"Preparing {StatementType.AddAllowedInvite}...");
			NpgsqlCommand addAllowedInvite = new("UPDATE guild_config SET allowed_invites=array_append(allowed_invites, @invite) WHERE guild_id=@guildId", Connection);
			_ = addAllowedInvite.Parameters.Add(new("invite", NpgsqlDbType.Text));
			_ = addAllowedInvite.Parameters.Add(new("guildId", NpgsqlDbType.Bigint));
			addAllowedInvite.Prepare();
			PreparedStatements.Add(StatementType.AddAllowedInvite, addAllowedInvite);

			Logger.Debug($"Preparing {StatementType.RemoveAllowedInvite}...");
			NpgsqlCommand removeAllowedInvite = new("UPDATE guild_config SET allowed_invites=array_remove(allowed_invites, @invite) WHERE guild_id=@guildId", Connection);
			_ = removeAllowedInvite.Parameters.Add(new("invite", NpgsqlDbType.Text));
			_ = removeAllowedInvite.Parameters.Add(new("guildId", NpgsqlDbType.Bigint));
			removeAllowedInvite.Prepare();
			PreparedStatements.Add(StatementType.RemoveAllowedInvite, removeAllowedInvite);

			Logger.Debug($"Preparing {StatementType.IsAllowedInvite}...");
			NpgsqlCommand isAllowedInvite = new("SELECT allowed_invites && @invite::text[] FROM guild_config WHERE guild_id=@guildId", Connection);
			_ = isAllowedInvite.Parameters.Add(new("invite", NpgsqlDbType.Text));
			_ = isAllowedInvite.Parameters.Add(new("guildId", NpgsqlDbType.Bigint));
			isAllowedInvite.Prepare();
			PreparedStatements.Add(StatementType.IsAllowedInvite, isAllowedInvite);

			Logger.Debug($"Preparing {StatementType.GetMaxLines}...");
			NpgsqlCommand getMaxLines = new("SELECT max_lines FROM guild_config WHERE guild_id=@guildId", Connection);
			_ = getMaxLines.Parameters.Add(new("guildId", NpgsqlDbType.Bigint));
			getMaxLines.Prepare();
			PreparedStatements.Add(StatementType.GetMaxLines, getMaxLines);

			Logger.Debug($"Preparing {StatementType.SetMaxLines}...");
			NpgsqlCommand setMaxLines = new("UPDATE guild_config SET max_lines=@maxLines WHERE guild_id=@guildId", Connection);
			_ = setMaxLines.Parameters.Add(new("maxLines", NpgsqlDbType.Integer));
			_ = setMaxLines.Parameters.Add(new("guildId", NpgsqlDbType.Bigint));
			setMaxLines.Prepare();
			PreparedStatements.Add(StatementType.SetMaxLines, setMaxLines);

			Logger.Debug($"Preparing {StatementType.GetMaxMentions}...");
			NpgsqlCommand getMaxMentions = new("SELECT max_mentions FROM guild_config WHERE guild_id=@guildId", Connection);
			_ = getMaxMentions.Parameters.Add(new("guildId", NpgsqlDbType.Bigint));
			getMaxMentions.Prepare();
			PreparedStatements.Add(StatementType.GetMaxMentions, getMaxMentions);

			Logger.Debug($"Preparing {StatementType.SetMaxMentions}...");
			NpgsqlCommand setMaxMentions = new("UPDATE guild_config SET max_mentions=@maxMentions WHERE guild_id=@guildId", Connection);
			_ = setMaxMentions.Parameters.Add(new("maxMentions", NpgsqlDbType.Integer));
			_ = setMaxMentions.Parameters.Add(new("guildId", NpgsqlDbType.Bigint));
			setMaxMentions.Prepare();
			PreparedStatements.Add(StatementType.SetMaxMentions, setMaxMentions);

			Logger.Debug($"Preparing {StatementType.GetAutoDehoist}...");
			NpgsqlCommand getAutoDehoist = new("SELECT auto_dehoist FROM guild_config WHERE guild_id=@guildId", Connection);
			_ = getAutoDehoist.Parameters.Add(new("guildId", NpgsqlDbType.Bigint));
			getAutoDehoist.Prepare();
			PreparedStatements.Add(StatementType.GetAutoDehoist, getAutoDehoist);

			Logger.Debug($"Preparing {StatementType.SetAutoDehoist}...");
			NpgsqlCommand setAutoDehoist = new("UPDATE guild_config SET auto_dehoist=@autoDehoist WHERE guild_id=@guildId", Connection);
			_ = setAutoDehoist.Parameters.Add(new("autoDehoist", NpgsqlDbType.Boolean));
			_ = setAutoDehoist.Parameters.Add(new("guildId", NpgsqlDbType.Bigint));
			setAutoDehoist.Prepare();
			PreparedStatements.Add(StatementType.SetAutoDehoist, setAutoDehoist);

			Logger.Debug($"Preparing {StatementType.GetAutoRaidMode}...");
			NpgsqlCommand getAutoRaidMode = new("SELECT auto_raidmode FROM guild_config WHERE guild_id=@guildId", Connection);
			_ = getAutoRaidMode.Parameters.Add(new("guildId", NpgsqlDbType.Bigint));
			getAutoRaidMode.Prepare();
			PreparedStatements.Add(StatementType.GetAutoRaidMode, getAutoRaidMode);

			Logger.Debug($"Preparing {StatementType.SetAutoRaidMode}...");
			NpgsqlCommand setAutoRaidMode = new("UPDATE guild_config SET auto_raidmode=@autoRaidMode WHERE guild_id=@guildId", Connection);
			_ = setAutoRaidMode.Parameters.Add(new("autoRaidMode", NpgsqlDbType.Boolean));
			_ = setAutoRaidMode.Parameters.Add(new("guildId", NpgsqlDbType.Bigint));
			setAutoRaidMode.Prepare();
			PreparedStatements.Add(StatementType.SetAutoRaidMode, setAutoRaidMode);

			Logger.Debug($"Preparing {StatementType.GetIgnoredChannels}...");
			NpgsqlCommand getIgnoredChannels = new("SELECT ignored_channels FROM guild_config WHERE guild_id=@guildId", Connection);
			_ = getIgnoredChannels.Parameters.Add(new("guildId", NpgsqlDbType.Bigint));
			getIgnoredChannels.Prepare();
			PreparedStatements.Add(StatementType.GetIgnoredChannels, getIgnoredChannels);

			Logger.Debug($"Preparing {StatementType.SetIgnoredChannels}...");
			NpgsqlCommand setIgnoredChannels = new("UPDATE guild_config SET ignored_channels=@ignoredChannels WHERE guild_id=@guildId", Connection);
			_ = setIgnoredChannels.Parameters.Add(new("ignoredChannels", NpgsqlDbType.Array | NpgsqlDbType.Bigint));
			_ = setIgnoredChannels.Parameters.Add(new("guildId", NpgsqlDbType.Bigint));
			setIgnoredChannels.Prepare();
			PreparedStatements.Add(StatementType.SetIgnoredChannels, setIgnoredChannels);

			Logger.Debug($"Preparing {StatementType.AddIgnoredChannel}...");
			NpgsqlCommand addIgnoredChannel = new("UPDATE guild_config SET ignored_channels=array_append(ignored_channels, @channelId) WHERE guild_id=@guildId", Connection);
			_ = addIgnoredChannel.Parameters.Add(new("channelId", NpgsqlDbType.Bigint));
			_ = addIgnoredChannel.Parameters.Add(new("guildId", NpgsqlDbType.Bigint));
			addIgnoredChannel.Prepare();
			PreparedStatements.Add(StatementType.AddIgnoredChannel, addIgnoredChannel);

			Logger.Debug($"Preparing {StatementType.RemoveIgnoredChannel}...");
			NpgsqlCommand removeIgnoredChannel = new("UPDATE guild_config SET ignored_channels=array_remove(ignored_channels, @channelId) WHERE guild_id=@guildId", Connection);
			_ = removeIgnoredChannel.Parameters.Add(new("channelId", NpgsqlDbType.Bigint));
			_ = removeIgnoredChannel.Parameters.Add(new("guildId", NpgsqlDbType.Bigint));
			removeIgnoredChannel.Prepare();
			PreparedStatements.Add(StatementType.RemoveIgnoredChannel, removeIgnoredChannel);

			Logger.Debug($"Preparing {StatementType.IsIgnoredChannel}...");
			NpgsqlCommand isIgnoredChannel = new("SELECT ignored_channels && ARRAY[@channelId] FROM guild_config WHERE guild_id=@guildId", Connection);
			_ = isIgnoredChannel.Parameters.Add(new("channelid", NpgsqlDbType.Bigint));
			_ = isIgnoredChannel.Parameters.Add(new("guildId", NpgsqlDbType.Bigint));
			isIgnoredChannel.Prepare();
			PreparedStatements.Add(StatementType.IsIgnoredChannel, isIgnoredChannel);

			Logger.Debug($"Preparing {StatementType.GetAdminRoles}...");
			NpgsqlCommand getAdminRoles = new("SELECT administraitive_roles FROM guild_config WHERE guild_id=@guildId", Connection);
			_ = getAdminRoles.Parameters.Add(new("guildId", NpgsqlDbType.Bigint));
			getAdminRoles.Prepare();
			PreparedStatements.Add(StatementType.GetAdminRoles, getAdminRoles);

			Logger.Debug($"Preparing {StatementType.SetAdminRoles}...");
			NpgsqlCommand setAdminRoles = new("UPDATE guild_config SET administraitive_roles=@roleIds WHERE guild_id=@guildId", Connection);
			_ = setAdminRoles.Parameters.Add(new("roleIds", NpgsqlDbType.Array | NpgsqlDbType.Bigint));
			_ = setAdminRoles.Parameters.Add(new("guildId", NpgsqlDbType.Bigint));
			setAdminRoles.Prepare();
			PreparedStatements.Add(StatementType.SetAdminRoles, setAdminRoles);

			Logger.Debug($"Preparing {StatementType.AddAdminRole}...");
			NpgsqlCommand addAdminRole = new("UPDATE guild_config SET administraitive_roles=array_append(administraitive_roles, @roleId) WHERE guild_id=@guildId", Connection);
			_ = addAdminRole.Parameters.Add(new("roleId", NpgsqlDbType.Bigint));
			_ = addAdminRole.Parameters.Add(new("guildId", NpgsqlDbType.Bigint));
			addAdminRole.Prepare();
			PreparedStatements.Add(StatementType.AddAdminRole, addAdminRole);

			Logger.Debug($"Preparing {StatementType.RemoveAdminRole}...");
			NpgsqlCommand removeAdminRole = new("UPDATE guild_config SET administraitive_roles=array_remove(administraitive_roles, @roleId) WHERE guild_id=@guildId", Connection);
			_ = removeAdminRole.Parameters.Add(new("roleId", NpgsqlDbType.Bigint));
			_ = removeAdminRole.Parameters.Add(new("guildId", NpgsqlDbType.Bigint));
			removeAdminRole.Prepare();
			PreparedStatements.Add(StatementType.RemoveAdminRole, removeAdminRole);

			Logger.Debug($"Preparing {StatementType.IsAdminRole}...");
			NpgsqlCommand isAdminRole = new("SELECT administraitive_roles && ARRAY[@roleId] FROM guild_config WHERE guild_id=@guildId", Connection);
			_ = isAdminRole.Parameters.Add(new("roleId", NpgsqlDbType.Bigint));
			_ = isAdminRole.Parameters.Add(new("guildId", NpgsqlDbType.Bigint));
			isAdminRole.Prepare();
			PreparedStatements.Add(StatementType.IsAdminRole, isAdminRole);

			Logger.Debug($"Preparing {StatementType.GetMuteRole}...");
			NpgsqlCommand getMuteRole = new("SELECT mute_role FROM guild_config WHERE guild_id=@guildId", Connection);
			_ = getMuteRole.Parameters.Add(new("guildId", NpgsqlDbType.Bigint));
			getMuteRole.Prepare();
			PreparedStatements.Add(StatementType.GetMuteRole, getMuteRole);

			Logger.Debug($"Preparing {StatementType.SetMuteRole}...");
			NpgsqlCommand setMuteRole = new("UPDATE guild_config SET mute_role=@roleId WHERE guild_id=@guildId", Connection);
			_ = setMuteRole.Parameters.Add(new("roleId", NpgsqlDbType.Bigint));
			_ = setMuteRole.Parameters.Add(new("guildId", NpgsqlDbType.Bigint));
			setMuteRole.Prepare();
			PreparedStatements.Add(StatementType.SetMuteRole, setMuteRole);

			Logger.Debug($"Preparing {StatementType.GetNoMemeRole}...");
			NpgsqlCommand getNoMemeRole = new("SELECT nomeme_role FROM guild_config WHERE guild_id=@guildId", Connection);
			_ = getNoMemeRole.Parameters.Add(new("guildId", NpgsqlDbType.Bigint));
			getNoMemeRole.Prepare();
			PreparedStatements.Add(StatementType.GetNoMemeRole, getNoMemeRole);

			Logger.Debug($"Preparing {StatementType.SetNoMemeRole}...");
			NpgsqlCommand setNoMemeRole = new("UPDATE guild_config SET nomeme_role=@roleId WHERE guild_id=@guildId", Connection);
			_ = setNoMemeRole.Parameters.Add(new("roleId", NpgsqlDbType.Bigint));
			_ = setNoMemeRole.Parameters.Add(new("guildId", NpgsqlDbType.Bigint));
			setNoMemeRole.Prepare();
			PreparedStatements.Add(StatementType.SetNoMemeRole, setNoMemeRole);

			Logger.Debug($"Preparing {StatementType.GetNoVCRole}...");
			NpgsqlCommand getNoVCRole = new("SELECT novc_role FROM guild_config WHERE guild_id=@guildId", Connection);
			_ = getNoVCRole.Parameters.Add(new("guildId", NpgsqlDbType.Bigint));
			getNoVCRole.Prepare();
			PreparedStatements.Add(StatementType.GetNoVCRole, getNoVCRole);

			Logger.Debug($"Preparing {StatementType.SetNoVCRole}...");
			NpgsqlCommand setNoVCRole = new("UPDATE guild_config SET novc_role=@roleId WHERE guild_id=@guildId", Connection);
			_ = setNoVCRole.Parameters.Add(new("roleId", NpgsqlDbType.Bigint));
			_ = setNoVCRole.Parameters.Add(new("guildId", NpgsqlDbType.Bigint));
			setNoVCRole.Prepare();
			PreparedStatements.Add(StatementType.SetNoVCRole, setNoVCRole);

			Logger.Debug($"Preparing {StatementType.GetAntiRaid}...");
			NpgsqlCommand getAntiRaid = new("SELECT antiraid FROM guild_config WHERE guild_id=@guildId", Connection);
			_ = getAntiRaid.Parameters.Add(new("guildId", NpgsqlDbType.Bigint));
			getAntiRaid.Prepare();
			PreparedStatements.Add(StatementType.GetAntiRaid, getAntiRaid);

			Logger.Debug($"Preparing {StatementType.SetAntiRaid}...");
			NpgsqlCommand setAntiRaid = new("UPDATE guild_config SET antiraid=@isEnabled WHERE guild_id=@guildId", Connection);
			_ = setAntiRaid.Parameters.Add(new("isEnabled", NpgsqlDbType.Boolean));
			_ = setAntiRaid.Parameters.Add(new("guildId", NpgsqlDbType.Bigint));
			setAntiRaid.Prepare();
			PreparedStatements.Add(StatementType.SetAntiRaid, setAntiRaid);

			Logger.Debug($"Preparing {StatementType.GetAntiRaidSetOff}...");
			NpgsqlCommand getAntiRaidSetOff = new("SELECT antiraid_setoff FROM guild_config WHERE guild_id=@guildId", Connection);
			_ = getAntiRaidSetOff.Parameters.Add(new("guildId", NpgsqlDbType.Bigint));
			getAntiRaidSetOff.Prepare();
			PreparedStatements.Add(StatementType.GetAntiRaidSetOff, getAntiRaidSetOff);

			Logger.Debug($"Preparing {StatementType.SetAntiRaidSetOff}...");
			NpgsqlCommand setAntiRaidSetOff = new("UPDATE guild_config SET antiraid_setoff=@interval WHERE guild_id=@guildId", Connection);
			_ = setAntiRaidSetOff.Parameters.Add(new("interval", NpgsqlDbType.Integer));
			_ = setAntiRaidSetOff.Parameters.Add(new("guildId", NpgsqlDbType.Bigint));
			setAntiRaidSetOff.Prepare();
			PreparedStatements.Add(StatementType.SetAntiRaidSetOff, setAntiRaidSetOff);
		}

		public void Dispose()
		{
			PreparedStatements.Clear();
			Connection.Close();
			Connection.Dispose();
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

		public ulong? NoMemeRole(ulong guildId) => (ulong?)ExecuteQuery(StatementType.GetNoMemeRole, new NpgsqlParameter("guildId", (long)guildId), true)?[0][0];
		public void NoMemeRole(ulong guildId, ulong roleId) => ExecuteQuery(StatementType.SetNoMemeRole, new List<NpgsqlParameter>() { new NpgsqlParameter("guildId", (long)guildId), new NpgsqlParameter("roleId", (long)roleId) });

		public ulong? NoVCRole(ulong guildId) => (ulong?)ExecuteQuery(StatementType.GetNoVCRole, new NpgsqlParameter("guildId", (long)guildId), true)?[0][0];
		public void NoVCRole(ulong guildId, ulong roleId) => ExecuteQuery(StatementType.SetNoVCRole, new List<NpgsqlParameter>() { new NpgsqlParameter("guildId", (long)guildId), new NpgsqlParameter("roleId", (long)roleId) });

		public bool AntiRaid(ulong guildId) => ExecuteQuery(StatementType.GetAntiRaid, new NpgsqlParameter("guildId", (long)guildId), true)?[0][0];
		public void AntiRaid(ulong guildId, bool isEnabled) => ExecuteQuery(StatementType.SetAntiRaid, new List<NpgsqlParameter>() { new NpgsqlParameter("guildId", (long)guildId), new NpgsqlParameter("isEnabled", isEnabled) });

		public int AntiRaidSetOff(ulong guildId) => ExecuteQuery(StatementType.GetAntiRaidSetOff, new NpgsqlParameter("guildId", (long)guildId), true)?[0][0];
		public void AntiRaidSetOff(ulong guildId, int interval) => ExecuteQuery(StatementType.SetAntiRaidSetOff, new List<NpgsqlParameter>() { new NpgsqlParameter("guildId", (long)guildId), new NpgsqlParameter("interval", interval) });
	}
}
