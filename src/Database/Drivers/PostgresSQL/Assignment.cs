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
	public class PostgresAssignments : IAssignment
	{
		private static readonly Logger _logger = new("Database.PostgresSQL.Assignment");
		private readonly NpgsqlConnection _connection;
		private readonly Dictionary<StatementType, NpgsqlCommand> _preparedStatements = new();
		private int retryCount;
		private enum StatementType
		{
			Create,
			Remove,
			SelectAssignment,
			SelectAssignmentById,
			SelectAllReminders,
			SelectAllAssignments
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

		public PostgresAssignments(string host, int port, string username, string password, string databaseName, SslMode sslMode)
		{
			_connection = new NpgsqlConnection($"Host={host};Port={port};Username={username};Password={password};Database={databaseName};SSL Mode={sslMode}");
			NpgsqlConnection selectRoutineConnection = new($"Host={host};Port={port};Username={username};Password={password};Database={databaseName};SSL Mode={sslMode}");
			_logger.Info("Opening connection to database...");
			try
			{
				_connection.Open();
				selectRoutineConnection.Open();
				_logger.Debug("Creating assignments table if it doesn't exist...");
				NpgsqlCommand createTagsTable = new(File.ReadAllText(Path.Join(FileSystem.ProjectRoot, "res/sql/drivers/postgresql/assignments_table.sql")), _connection);
				_ = createTagsTable.ExecuteNonQuery();
				createTagsTable.Dispose();
			}
			catch (SocketException error)
			{
				_logger.Critical($"Failed to connect to database. {error.Message}");
			}
			catch (PostgresException error) when (error.SqlState == "28P01")
			{
				_logger.Critical($"Failed to connect to database. Check your password.");
			}
			_logger.Info("Preparing SQL commands...");
			_logger.Debug($"Preparing {StatementType.Create}...");
			NpgsqlCommand createAssignment = new("INSERT INTO assignments(task_type, guild_id, channel_id, message_id, user_id, set_off, set_at, content) VALUES(@taskType, @guildId, @channelId, @messageId, @userId, @setOff, @setAt, @content)", _connection);
			_ = createAssignment.Parameters.Add(new("taskType", NpgsqlDbType.Smallint));
			_ = createAssignment.Parameters.Add(new("guildId", NpgsqlDbType.Bigint));
			_ = createAssignment.Parameters.Add(new("channelId", NpgsqlDbType.Bigint));
			_ = createAssignment.Parameters.Add(new("messageId", NpgsqlDbType.Bigint));
			_ = createAssignment.Parameters.Add(new("userId", NpgsqlDbType.Bigint));
			_ = createAssignment.Parameters.Add(new("setOff", NpgsqlDbType.Timestamp));
			_ = createAssignment.Parameters.Add(new("setAt", NpgsqlDbType.Timestamp));
			_ = createAssignment.Parameters.Add(new("content", NpgsqlDbType.Varchar));
			createAssignment.Prepare();
			_preparedStatements.Add(StatementType.Create, createAssignment);

			_logger.Debug($"Preparing {StatementType.Remove}...");
			NpgsqlCommand removeAssignment = new("DELETE FROM assignments WHERE id=@taskId", _connection);
			_ = removeAssignment.Parameters.Add(new("taskId", NpgsqlDbType.Integer));
			removeAssignment.Prepare();
			_preparedStatements.Add(StatementType.Remove, removeAssignment);

			_logger.Debug($"Preparing {StatementType.SelectAssignment}...");
			NpgsqlCommand selectAssignment = new("SELECT task_type, guild_id, channel_id, message_id, user_id, set_off, set_at, content, id FROM assignments WHERE user_id=@userId AND task_type=@taskType", _connection);
			_ = selectAssignment.Parameters.Add(new("userId", NpgsqlDbType.Bigint));
			_ = selectAssignment.Parameters.Add(new("taskType", NpgsqlDbType.Smallint));
			selectAssignment.Prepare();
			_preparedStatements.Add(StatementType.SelectAssignment, selectAssignment);

			_logger.Debug($"Preparing {StatementType.SelectAssignmentById}...");
			NpgsqlCommand selectAssignmentById = new("SELECT task_type, guild_id, channel_id, message_id, user_id, set_off, set_at, content, id FROM assignments WHERE user_id=@userId AND task_type=@taskType AND id=@taskId", _connection);
			_ = selectAssignmentById.Parameters.Add(new("userId", NpgsqlDbType.Bigint));
			_ = selectAssignmentById.Parameters.Add(new("taskType", NpgsqlDbType.Smallint));
			_ = selectAssignmentById.Parameters.Add(new("taskId", NpgsqlDbType.Integer));
			selectAssignmentById.Prepare();
			_preparedStatements.Add(StatementType.SelectAssignmentById, selectAssignmentById);

			_logger.Debug($"Preparing {StatementType.SelectAllAssignments}...");
			NpgsqlCommand selectAllAssignments = new("SELECT * FROM assignments", selectRoutineConnection);
			selectAllAssignments.Prepare();
			_preparedStatements.Add(StatementType.SelectAllAssignments, selectAllAssignments);

			_logger.Debug($"Preparing {StatementType.SelectAllReminders}...");
			NpgsqlCommand selectAllReminders = new("SELECT * FROM assignments WHERE user_id=@userId", _connection);
			_ = selectAllReminders.Parameters.Add(new("userId", NpgsqlDbType.Bigint));
			selectAllReminders.Prepare();
			_preparedStatements.Add(StatementType.SelectAllReminders, selectAllReminders);
		}
		public void Create(AssignmentType taskType, ulong guildId, ulong channelId, ulong messageId, ulong userId, DateTime setOff, DateTime setAt, string content) => ExecuteQuery(StatementType.Create, new List<NpgsqlParameter>() { new("taskType", (int)taskType), new("guildId", (long)guildId), new("channelId", (long)channelId), new("messageId", (long)messageId), new("userId", (long)userId), new("setOff", setOff), new("setAt", setAt), new("content", content) });
		public void Remove(int taskId) => ExecuteQuery(StatementType.Remove, new NpgsqlParameter("taskId", taskId));
		public Assignment[] Retrieve(ulong userId, AssignmentType taskType)
		{
			Dictionary<int, List<dynamic>> queryResults = ExecuteQuery(StatementType.SelectAssignment, new List<NpgsqlParameter>() { new("userId", (long)userId), new("taskType", (int)taskType) }, true);
			if (queryResults == null) return null;
			List<Assignment> assignments = new();
			foreach (int i in queryResults.Keys)
			{ // Order can be determined from the SQL query and how it was selected.
				Assignment assignment = new();
				assignment.AssignmentType = (AssignmentType)queryResults[i][0];
				assignment.GuildId = (ulong)queryResults[i][1];
				assignment.ChannelId = (ulong)queryResults[i][2];
				assignment.MessageId = (ulong)queryResults[i][3];
				assignment.UserId = (ulong)queryResults[i][4];
				assignment.SetOff = (DateTime)queryResults[i][5];
				assignment.SetAt = (DateTime)queryResults[i][6];
				assignment.Content = queryResults[i][7].ToString();
				assignment.AssignmentId = (int)queryResults[i][8];
				assignments.Add(assignment);
			}
			return assignments.ToArray();
		}

		public Assignment? Retrieve(ulong userId, AssignmentType taskType, int taskId)
		{
			Dictionary<int, List<dynamic>> queryResults = ExecuteQuery(StatementType.SelectAssignment, new List<NpgsqlParameter>() { new("userId", (long)userId), new("taskType", (int)taskType), new("taskId", taskId) }, true);
			if (queryResults == null) return null;
			Assignment assignment = new();
			foreach (int key in queryResults.Keys)
			{
				assignment.AssignmentType = (AssignmentType)queryResults[key][0];
				assignment.GuildId = (ulong)queryResults[key][1];
				assignment.ChannelId = (ulong)queryResults[key][2];
				assignment.MessageId = (ulong)queryResults[key][3];
				assignment.UserId = (ulong)queryResults[key][4];
				assignment.SetOff = (DateTime)queryResults[key][5];
				assignment.SetAt = (DateTime)queryResults[key][6];
				assignment.Content = queryResults[key][7].ToString();
				assignment.AssignmentId = (int)queryResults[key][8];
			}
			return assignment;
		}

		public Assignment[] SelectAllAssignments()
		{
			Dictionary<int, List<dynamic>> queryResults = ExecuteQuery(StatementType.SelectAllAssignments, new List<NpgsqlParameter>(), true);
			if (queryResults == null) return null;
			List<Assignment> assignments = new();
			foreach (int i in queryResults.Keys)
			{ // Order can be determined from the SQL query and how it was selected.
				Assignment assignment = new();
				assignment.AssignmentType = (AssignmentType)queryResults[i][0];
				assignment.GuildId = (ulong)queryResults[i][1];
				assignment.ChannelId = (ulong)queryResults[i][2];
				assignment.MessageId = (ulong)queryResults[i][3];
				assignment.UserId = (ulong)queryResults[i][4];
				assignment.SetOff = (DateTime)queryResults[i][5];
				assignment.SetAt = (DateTime)queryResults[i][6];
				assignment.Content = queryResults[i][7].ToString();
				assignment.AssignmentId = (int)queryResults[i][8];
				assignments.Add(assignment);
			}
			return assignments.ToArray();
		}

		public Assignment[] SelectAllReminders(ulong userId)
		{
			Dictionary<int, List<dynamic>> queryResults = ExecuteQuery(StatementType.SelectAllReminders, new NpgsqlParameter("userId", (long)userId), true);
			if (queryResults == null) return null;
			List<Assignment> assignments = new();
			foreach (int i in queryResults.Keys)
			{ // Order can be determined from the SQL query and how it was selected.
				Assignment assignment = new();
				assignment.AssignmentType = (AssignmentType)queryResults[i][0];
				assignment.GuildId = (ulong)queryResults[i][1];
				assignment.ChannelId = (ulong)queryResults[i][2];
				assignment.MessageId = (ulong)queryResults[i][3];
				assignment.UserId = (ulong)queryResults[i][4];
				assignment.SetOff = (DateTime)queryResults[i][5];
				assignment.SetAt = (DateTime)queryResults[i][6];
				assignment.Content = queryResults[i][7].ToString();
				assignment.AssignmentId = (int)queryResults[i][8];
				assignments.Add(assignment);
			}
			return assignments.ToArray();
		}

		public void Dispose()
		{
			_preparedStatements.Clear();
			_connection.Close();
			_connection.Dispose();
			GC.SuppressFinalize(this);
		}
	}
}
