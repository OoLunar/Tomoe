using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Microsoft.Data.Sqlite;

using Tomoe.Database.Interfaces;
using Tomoe.Utils;

namespace Tomoe.Database.Drivers.Sqlite
{
	public class SqliteAssignments : IAssignment
	{
		private readonly SqliteConnection _connection;
		private readonly Logger _logger = new("Database.SQLite.Assignment");
		private readonly Dictionary<StatementType, SqliteCommand> _preparedStatements = new();

		private enum StatementType
		{
			Create,
			Remove,
			SelectAssignment,
			SelectAssignmentById,
			SelectAllReminders,
			SelectAllAssignments
		}

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

		public SqliteAssignments(string password, string databaseName, SqliteOpenMode openMode, SqliteCacheMode cacheMode)
		{
			SqliteConnectionStringBuilder connectionString = new();
			connectionString.Mode = openMode;
			connectionString.Cache = cacheMode;
			connectionString.DataSource = databaseName;
			connectionString.Password = password;
			_connection = new(connectionString.ToString());
			_logger.Info("Opening connection to database...");
			_connection.Open();
			_logger.Debug("Creating assignments table if it doesn't exist...");
			SqliteCommand createStrikeTable = new(File.ReadAllText(Path.Join(FileSystem.ProjectRoot, "res/sql/drivers/sqlite/assignments_table.sql")), _connection);
			_ = createStrikeTable.ExecuteNonQuery();
			createStrikeTable.Dispose();

			_logger.Info("Preparing SQL commands...");
			_logger.Debug($"Preparing {StatementType.Create}...");
			SqliteCommand create = new("INSERT INTO assignments(assignment_type, guild_id, channel_id, message_id, user_id, set_off, set_at, content) VALUES(@assignmentType, @guildId, @channelId, @messageId, @userId, @setOff, @setAt, @content)", _connection);
			SqliteParameter createAssignmentType = new("@assignmentType", SqliteType.Integer, sizeof(AssignmentType), "assignment_type");
			SqliteParameter createGuildId = new("@guildId", SqliteType.Integer, sizeof(ulong), "guild_id");
			SqliteParameter createChannelId = new("@channelId", SqliteType.Integer, sizeof(ulong), "channel_id");
			SqliteParameter createMessageId = new("@messageId", SqliteType.Integer, sizeof(ulong), "message_id");
			SqliteParameter createUserId = new("@userId", SqliteType.Integer, sizeof(ulong), "user_id");
			SqliteParameter createSetOff = new("@setOff", SqliteType.Integer);
			SqliteParameter createSetAt = new("@setAt", SqliteType.Integer);
			SqliteParameter createContent = new("@content", SqliteType.Text);
			_ = create.Parameters.Add(createGuildId);
			_ = create.Parameters.Add(createAssignmentType);
			_ = create.Parameters.Add(createChannelId);
			_ = create.Parameters.Add(createMessageId);
			_ = create.Parameters.Add(createUserId);
			_ = create.Parameters.Add(createSetOff);
			_ = create.Parameters.Add(createSetAt);
			_ = create.Parameters.Add(createContent);
			create.Prepare();
			_preparedStatements.Add(StatementType.Create, create);

			_logger.Debug($"Preparing {StatementType.Remove}...");
			SqliteCommand remove = new("DELETE FROM assignments WHERE id=@assignmentId", _connection);
			_ = remove.Parameters.Add(new("@assignmentId", SqliteType.Blob));
			remove.Prepare();
			_preparedStatements.Add(StatementType.Remove, remove);

			_logger.Debug($"Preparing {StatementType.SelectAssignment}...");
			SqliteCommand selectAssignment = new("SELECT assignment_type, guild_id, channel_id, message_id, user_id, set_off, set_at, content, id FROM assignments WHERE user_id=@userId AND assignment_type=@assignmentType", _connection);
			_ = selectAssignment.Parameters.Add(new("@userId", SqliteType.Integer, sizeof(ulong), "user_id"));
			_ = selectAssignment.Parameters.Add(new("@assignmentType", SqliteType.Integer, sizeof(AssignmentType), "assignment_type"));
			selectAssignment.Prepare();
			_preparedStatements.Add(StatementType.SelectAssignment, selectAssignment);

			_logger.Debug($"Preparing {StatementType.SelectAssignmentById}...");
			SqliteCommand selectAssignmentById = new("SELECT assignment_type, guild_id, channel_id, message_id, user_id, set_off, set_at, content, id FROM assignments WHERE user_id=@userId AND assignment_type=@assignmentType AND id=@assignmentId", _connection);
			_ = selectAssignmentById.Parameters.Add(new("@userId", SqliteType.Integer, sizeof(ulong), "user_id"));
			_ = selectAssignmentById.Parameters.Add(new("@assignmentType", SqliteType.Integer, sizeof(AssignmentType), "assignment_type"));
			selectAssignmentById.Prepare();
			_preparedStatements.Add(StatementType.SelectAssignmentById, selectAssignmentById);

			_logger.Debug($"Preparing {StatementType.SelectAllAssignments}...");
			SqliteCommand selectAllAssignments = new("SELECT * FROM assignments", _connection);
			selectAllAssignments.Prepare();
			_preparedStatements.Add(StatementType.SelectAllAssignments, selectAllAssignments);

			_logger.Debug($"Preparing {StatementType.SelectAllReminders}...");
			SqliteCommand selectAllReminders = new("SELECT * FROM assignments WHERE user_id=@userId", _connection);
			_ = selectAllReminders.Parameters.Add(new("@userId", SqliteType.Integer, sizeof(ulong), "user_id"));
			selectAllReminders.Prepare();
			_preparedStatements.Add(StatementType.SelectAllReminders, selectAllReminders);
		}

		public void Dispose()
		{
			_preparedStatements.Clear();
			_connection.Dispose();
			GC.SuppressFinalize(this);
		}

		public void Create(AssignmentType assignmentType, ulong guildId, ulong channelId, ulong messageId, ulong userId, DateTime setOff, DateTime setAt, string content) => ExecuteQuery(StatementType.Create, new List<SqliteParameter>() { new("@assignmentType", (int)assignmentType), new("@guildId", guildId), new("@channelId", channelId), new("@messageId", messageId), new("@userId", userId), new("@setOff", setOff), new("@setAt", setAt), new("@content", content) });
		public void Remove(int assignmentId) => ExecuteQuery(StatementType.Remove, new SqliteParameter("@assignmentId", assignmentId));
		public Assignment[] Retrieve(ulong userId, AssignmentType assignmentType)
		{
			Dictionary<int, List<dynamic>> queryResults = ExecuteQuery(StatementType.SelectAssignment, new List<SqliteParameter>() { new("@userId", userId), new("@assignmentType", (int)assignmentType) }, true);
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
				assignment.SetOff = DateTime.Parse(queryResults[i][5]);
				assignment.SetAt = DateTime.Parse(queryResults[i][6]);
				assignment.Content = queryResults[i][7].ToString();
				assignment.AssignmentId = (int)queryResults[i][8];
				assignments.Add(assignment);
			}
			return assignments.ToArray();
		}

		public Assignment? Retrieve(ulong userId, AssignmentType assignmentType, int assignmentId)
		{
			Dictionary<int, List<dynamic>> queryResults = ExecuteQuery(StatementType.SelectAssignment, new List<SqliteParameter>() { new("@userId", userId), new("@assignmentType", (int)assignmentType), new("@assignmentId", assignmentId) }, true);
			if (queryResults == null) return null;
			Assignment assignment = new();
			foreach (int key in queryResults.Keys)
			{
				assignment.AssignmentType = (AssignmentType)queryResults[key][0];
				assignment.GuildId = (ulong)queryResults[key][1];
				assignment.ChannelId = (ulong)queryResults[key][2];
				assignment.MessageId = (ulong)queryResults[key][3];
				assignment.UserId = (ulong)queryResults[key][4];
				assignment.SetOff = DateTime.Parse(queryResults[key][5]);
				assignment.SetAt = DateTime.Parse(queryResults[key][6]);
				assignment.Content = queryResults[key][7].ToString();
				assignment.AssignmentId = int.Parse(queryResults[key][8]);
			}
			return assignment;
		}

		public Assignment[] SelectAllAssignments()
		{
			Dictionary<int, List<dynamic>> queryResults = ExecuteQuery(StatementType.SelectAllAssignments, new List<SqliteParameter>(), true);
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
				assignment.SetOff = DateTime.Parse(queryResults[i][5]);
				assignment.SetAt = DateTime.Parse(queryResults[i][6]);
				assignment.Content = queryResults[i][7].ToString();
				assignment.AssignmentId = (int)queryResults[i][8];
				assignments.Add(assignment);
			}
			return assignments.ToArray();
		}

		public Assignment[] SelectAllReminders(ulong userId)
		{
			Dictionary<int, List<dynamic>> queryResults = ExecuteQuery(StatementType.SelectAllReminders, new SqliteParameter("@userId", userId), true);
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
				assignment.SetOff = DateTime.Parse(queryResults[i][5]);
				assignment.SetAt = DateTime.Parse(queryResults[i][6]);
				assignment.Content = queryResults[i][7].ToString();
				assignment.AssignmentId = (int)queryResults[i][8];
				assignments.Add(assignment);
			}
			return assignments.ToArray();
		}
	}
}
