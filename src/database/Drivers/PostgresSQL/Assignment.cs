using System;
using System.Collections.Generic;
using System.IO;
using Npgsql;
using NpgsqlTypes;
using Tomoe.Database.Interfaces;
using Tomoe.Utils;
using System.Net.Sockets;

namespace Tomoe.Database.Drivers.PostgresSQL
{
	public class PostgresAssignments : IAssignment
	{
		private static readonly Logger Logger = new Logger("Database.PostgresSQL.Assignment");
		private readonly NpgsqlConnection Connection;
		private readonly Dictionary<StatementType, NpgsqlCommand> PreparedStatements = new Dictionary<StatementType, NpgsqlCommand>();
		private enum StatementType
		{
			Create,
			Remove,
			SelectTask,
			SelectTaskById,
			SelectAllReminders,
			SelectAllAssignments
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
			List<string> keyValue = new List<string>();
			foreach (NpgsqlParameter param in parameters)
			{
				keyValue.Add($"\"{param.ParameterName}: {param.Value}\"");
			}
			Logger.Trace($"Executing prepared statement \"{command}\" with parameters: {string.Join(", ", keyValue.ToArray())}");

			NpgsqlCommand statement = PreparedStatements[command];
			Dictionary<string, NpgsqlParameter> sortedParameters = new Dictionary<string, NpgsqlParameter>();
			foreach (NpgsqlParameter param in parameters)
			{
				sortedParameters.Add(param.ParameterName, param);
			}

			foreach (NpgsqlParameter temp in statement.Parameters)
			{
				temp.Value = sortedParameters[temp.ParameterName].Value;
			}

			if (needsResult)
			{
				NpgsqlDataReader reader = statement.ExecuteReaderAsync().ConfigureAwait(false).GetAwaiter().GetResult();
				Dictionary<int, List<dynamic>> values = new Dictionary<int, List<dynamic>>();
				int indexCount = 0;
				while (reader.Read())
				{
					List<dynamic> list = new List<dynamic>();
					for (int i = 0; i < reader.FieldCount; i++)
					{
						if (reader[i].GetType() == typeof(DBNull))
						{
							list.Add(null);
						}
						else
						{
							list.Add(reader[i]);
						}
						Logger.Trace($"Recieved values: {reader[i] ?? "null"} on iteration {i}");
					}
					if (list.Count == 1 && list[0] == null)
					{
						values.Add(indexCount, null);
					}
					else
					{
						values.Add(indexCount, list);
					}
					indexCount++;
				}
				reader.DisposeAsync().ConfigureAwait(false).GetAwaiter();
				if (values.Count == 0)
				{
					values = null;
				}
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

		public PostgresAssignments(string host, int port, string username, string password, string databaseName, SslMode sslMode)
		{
			Connection = new NpgsqlConnection($"Host={host};Port={port};Username={username};Password={password};Database={databaseName};SSL Mode={sslMode}");
			NpgsqlConnection selectRoutineConnection = new NpgsqlConnection($"Host={host};Port={port};Username={username};Password={password};Database={databaseName};SSL Mode={sslMode}");
			Logger.Info("Opening connection to database...");
			try
			{
				Connection.Open();
				selectRoutineConnection.Open();
				NpgsqlCommand createTagsTable = new NpgsqlCommand(File.ReadAllText(Path.Join(FileSystem.ProjectRoot, "res/sql/assignments_table.sql")), Connection);
				_ = createTagsTable.ExecuteNonQuery();
				createTagsTable.Dispose();
			}
			catch (SocketException error)
			{
				Logger.Critical($"Failed to connect to database. {error.Message}", true);
			}
			Logger.Info("Preparing SQL commands...");
			Logger.Debug($"Preparing {StatementType.Create}...");
			NpgsqlCommand createTask = new NpgsqlCommand("INSERT INTO assignments(task_type, guild_id, channel_id, message_id, user_id, set_off, set_at, content) VALUES(@taskType, @guildId, @channelId, @messageId, @userId, @setOff, @setAt, @content)", Connection);
			_ = createTask.Parameters.Add(new NpgsqlParameter("taskType", NpgsqlDbType.Smallint));
			_ = createTask.Parameters.Add(new NpgsqlParameter("guildId", NpgsqlDbType.Bigint));
			_ = createTask.Parameters.Add(new NpgsqlParameter("channelId", NpgsqlDbType.Bigint));
			_ = createTask.Parameters.Add(new NpgsqlParameter("messageId", NpgsqlDbType.Bigint));
			_ = createTask.Parameters.Add(new NpgsqlParameter("userId", NpgsqlDbType.Bigint));
			_ = createTask.Parameters.Add(new NpgsqlParameter("setOff", NpgsqlDbType.Timestamp));
			_ = createTask.Parameters.Add(new NpgsqlParameter("setAt", NpgsqlDbType.Timestamp));
			_ = createTask.Parameters.Add(new NpgsqlParameter("content", NpgsqlDbType.Varchar));
			createTask.Prepare();
			PreparedStatements.Add(StatementType.Create, createTask);

			Logger.Debug($"Preparing {StatementType.Remove}...");
			NpgsqlCommand removeTask = new NpgsqlCommand("DELETE FROM assignments WHERE id=@taskId", Connection);
			_ = removeTask.Parameters.Add(new NpgsqlParameter("taskId", NpgsqlDbType.Integer));
			removeTask.Prepare();
			PreparedStatements.Add(StatementType.Remove, removeTask);

			Logger.Debug($"Preparing {StatementType.SelectTask}...");
			NpgsqlCommand selectTask = new NpgsqlCommand("SELECT task_type, guild_id, channel_id, message_id, user_id, set_off, set_at, content, id FROM assignments WHERE user_id=@userId AND task_type=@taskType", Connection);
			_ = selectTask.Parameters.Add(new NpgsqlParameter("userId", NpgsqlDbType.Bigint));
			_ = selectTask.Parameters.Add(new NpgsqlParameter("taskType", NpgsqlDbType.Smallint));
			selectTask.Prepare();
			PreparedStatements.Add(StatementType.SelectTask, selectTask);

			Logger.Debug($"Preparing {StatementType.SelectTaskById}...");
			NpgsqlCommand selectTaskById = new NpgsqlCommand("SELECT task_type, guild_id, channel_id, message_id, user_id, set_off, set_at, content, id FROM assignments WHERE user_id=@userId AND task_type=@taskType AND id=@taskId", Connection);
			_ = selectTaskById.Parameters.Add(new NpgsqlParameter("userId", NpgsqlDbType.Bigint));
			_ = selectTaskById.Parameters.Add(new NpgsqlParameter("taskType", NpgsqlDbType.Smallint));
			_ = selectTaskById.Parameters.Add(new NpgsqlParameter("taskId", NpgsqlDbType.Integer));
			selectTaskById.Prepare();
			PreparedStatements.Add(StatementType.SelectTaskById, selectTaskById);

			Logger.Debug($"Preparing {StatementType.SelectAllAssignments}...");
			NpgsqlCommand selectAllAssignments = new NpgsqlCommand("SELECT * FROM assignments", selectRoutineConnection);
			selectAllAssignments.Prepare();
			PreparedStatements.Add(StatementType.SelectAllAssignments, selectAllAssignments);

			Logger.Debug($"Preparing {StatementType.SelectAllReminders}...");
			NpgsqlCommand selectAllReminders = new NpgsqlCommand("SELECT * FROM assignments WHERE user_id=@userId", Connection);
			_ = selectAllReminders.Parameters.Add(new NpgsqlParameter("userId", NpgsqlDbType.Bigint));
			selectAllReminders.Prepare();
			PreparedStatements.Add(StatementType.SelectAllReminders, selectAllReminders);
		}
		public void Create(AssignmentType taskType, ulong guildId, ulong channelId, ulong messageId, ulong userId, DateTime setOff, DateTime setAt, string content) => ExecuteQuery(StatementType.Create, new List<NpgsqlParameter>() { new NpgsqlParameter("taskType", (int)taskType), new NpgsqlParameter("guildId", (long)guildId), new NpgsqlParameter("channelId", (long)channelId), new NpgsqlParameter("messageId", (long)messageId), new NpgsqlParameter("userId", (long)userId), new NpgsqlParameter("setOff", setOff), new NpgsqlParameter("setAt", setAt), new NpgsqlParameter("content", content) });
		public void Remove(int taskId) => ExecuteQuery(StatementType.Remove, new NpgsqlParameter("taskId", taskId));
		public Assignment[] Select(ulong userId, AssignmentType taskType)
		{
			Dictionary<int, List<dynamic>> queryResults = ExecuteQuery(StatementType.SelectTask, new List<NpgsqlParameter>() { new NpgsqlParameter("userId", (long)userId), new NpgsqlParameter("taskType", (int)taskType) }, true);
			if (queryResults == null)
			{
				return null;
			}
			List<Assignment> assignments = new List<Assignment>();
			foreach (int i in queryResults.Keys)
			{ // Order can be determined from the SQL query and how it was selected.
				Assignment assignment = new();
				assignment.TaskType = (AssignmentType)queryResults[i][0];
				assignment.GuildId = (ulong)queryResults[i][1];
				assignment.ChannelId = (ulong)queryResults[i][2];
				assignment.MessageId = (ulong)queryResults[i][3];
				assignment.UserId = (ulong)queryResults[i][4];
				assignment.SetOff = (DateTime)queryResults[i][5];
				assignment.SetAt = (DateTime)queryResults[i][6];
				assignment.Content = queryResults[i][7].ToString();
				assignment.TaskId = (int)queryResults[i][8];
				assignments.Add(assignment);
			}
			return assignments.ToArray();
		}

		public Assignment? Select(ulong userId, AssignmentType taskType, int taskId)
		{
			Dictionary<int, List<dynamic>> queryResults = ExecuteQuery(StatementType.SelectTask, new List<NpgsqlParameter>() { new NpgsqlParameter("userId", (long)userId), new NpgsqlParameter("taskType", (int)taskType), new NpgsqlParameter("taskId", taskId) }, true);
			if (queryResults == null)
			{
				return null;
			}
			Assignment assignment = new Assignment();
			foreach (int key in queryResults.Keys)
			{
				assignment.TaskType = (AssignmentType)queryResults[key][0];
				assignment.GuildId = (ulong)queryResults[key][1];
				assignment.ChannelId = (ulong)queryResults[key][2];
				assignment.MessageId = (ulong)queryResults[key][3];
				assignment.UserId = (ulong)queryResults[key][4];
				assignment.SetOff = (DateTime)queryResults[key][5];
				assignment.SetAt = (DateTime)queryResults[key][6];
				assignment.Content = queryResults[key][7].ToString();
				assignment.TaskId = (int)queryResults[key][8];
			}
			return assignment;
		}

		public Assignment[] SelectAllAssignments()
		{
			Dictionary<int, List<dynamic>> queryResults = ExecuteQuery(StatementType.SelectAllAssignments, new List<NpgsqlParameter>(), true);
			if (queryResults == null)
			{
				return null;
			}
			List<Assignment> assignments = new List<Assignment>();
			foreach (int i in queryResults.Keys)
			{ // Order can be determined from the SQL query and how it was selected.
				Assignment assignment = new();
				assignment.TaskType = (AssignmentType)queryResults[i][0];
				assignment.GuildId = (ulong)queryResults[i][1];
				assignment.ChannelId = (ulong)queryResults[i][2];
				assignment.MessageId = (ulong)queryResults[i][3];
				assignment.UserId = (ulong)queryResults[i][4];
				assignment.SetOff = (DateTime)queryResults[i][5];
				assignment.SetAt = (DateTime)queryResults[i][6];
				assignment.Content = queryResults[i][7].ToString();
				assignment.TaskId = (int)queryResults[i][8];
				assignments.Add(assignment);
			}
			return assignments.ToArray();
		}

		public Assignment[] SelectAllReminders(ulong userId)
		{
			Dictionary<int, List<dynamic>> queryResults = ExecuteQuery(StatementType.SelectAllReminders, new NpgsqlParameter("userId", (long)userId), true);
			if (queryResults == null)
			{
				return null;
			}
			List<Assignment> assignments = new List<Assignment>();
			foreach (int i in queryResults.Keys)
			{ // Order can be determined from the SQL query and how it was selected.
				Assignment assignment = new();
				assignment.TaskType = (AssignmentType)queryResults[i][0];
				assignment.GuildId = (ulong)queryResults[i][1];
				assignment.ChannelId = (ulong)queryResults[i][2];
				assignment.MessageId = (ulong)queryResults[i][3];
				assignment.UserId = (ulong)queryResults[i][4];
				assignment.SetOff = (DateTime)queryResults[i][5];
				assignment.SetAt = (DateTime)queryResults[i][6];
				assignment.Content = queryResults[i][7].ToString();
				assignment.TaskId = (int)queryResults[i][8];
				assignments.Add(assignment);
			}
			return assignments.ToArray();
		}

		public void Dispose()
		{
			PreparedStatements.Clear();
			Connection.Close();
			Connection.Dispose();
			GC.SuppressFinalize(this);
		}
	}
}
