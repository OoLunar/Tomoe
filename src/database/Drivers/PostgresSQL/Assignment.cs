using System;
using System.Collections.Generic;
using System.IO;
using Npgsql;
using NpgsqlTypes;
using Tomoe.Database.Interfaces;
using Tomoe.Utils;

namespace Tomoe.Database.Drivers.PostgresSQL {
    public class PostgresAssignment : IAssignment {
        private static Logger _logger = new Logger("Database.PostgresSQL.Assignment");
        private NpgsqlConnection _connection;
        private Dictionary<statementType, NpgsqlCommand> _preparedStatements = new Dictionary<statementType, NpgsqlCommand>();
        private enum statementType {
            Create,
            Remove,
            SelectTask,
            SelectTaskById,
            SelectAllReminders,
            SelectAllAssignments
        }

        /// <summary>
        /// Executes an SQL query from <see cref="Tomoe.Database.Drivers.PostgresSQL.PostgresAssignment._preparedStatements">_preparedStatements</see>, using <seealso cref="Tomoe.Database.Drivers.PostgresSQL.PostgresAssignment.statementType">statementType</seealso> as a key.
        /// 
        /// Returns a list of results if <paramref name="needsResult">needsResult</paramref> is true, otherwise returns null.
        /// </summary>
        /// <param name="command">Which SQL command to execute, using <see cref="Tomoe.Database.Drivers.PostgresSQL.PostgresAssignment.statementType">statementType</see> as an index.</param>
        /// <param name="parameters">A list of <see cref="Npgsql.NpgsqlParameter">NpgsqlParameter's</see>.</param>
        /// <param name="needsResult">Returns a list of results if true, otherwise returns null.</param>
        /// <returns><see cref="System.Collections.Generic.List{T}">List&lt;dynamic&gt;</see> if <paramref name="needsResult">needsResult</paramref> is true, otherwise returns null.</returns>
        private Dictionary<int, List<dynamic>> executeQuery(statementType command, List<NpgsqlParameter> parameters, bool needsResult = false) {
            List<string> keyValue = new List<string>();
            foreach (NpgsqlParameter param in parameters) keyValue.Add($"\"{param.ParameterName}: {param.Value}\"");
            _logger.Trace($"Executing prepared statement \"{command.ToString()}\" with parameters: {string.Join(", ", keyValue.ToArray())}");

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

        /// <inheritdoc cref="Tomoe.Database.Drivers.PostgresSQL.PostgresAssignment.executeQuery(statementType, List{NpgsqlParameter}, bool)" />
        /// <param name="parameter">One <see cref="Npgsql.NpgsqlParameter">NpgsqlParameter</see>, which gets converted into a <see cref="System.Collections.Generic.List{T}">List&lt;NpgsqlParameter&gt;</see>.</param>
        private Dictionary<int, List<dynamic>> executeQuery(statementType command, NpgsqlParameter parameter, bool needsResult = false) => executeQuery(command, new List<NpgsqlParameter> { parameter }, needsResult);

        public PostgresAssignment(string host, int port, string username, string password, string database_name, SslMode sslMode) {
            _connection = new NpgsqlConnection($"Host={host};Port={port};Username={username};Password={password};Database={database_name};SSL Mode={sslMode}");
            NpgsqlConnection selectRoutineConnection = new NpgsqlConnection($"Host={host};Port={port};Username={username};Password={password};Database={database_name};SSL Mode={sslMode}");
            _logger.Info("Opening connection to database...");
            try {
                _connection.Open();
                selectRoutineConnection.Open();
                NpgsqlCommand createTagsTable = new NpgsqlCommand(File.ReadAllText(Path.Join(FileSystem.ProjectRoot, "res/sql/assignments_table.sql")), _connection);
                createTagsTable.ExecuteNonQuery();
                createTagsTable.Dispose();
            } catch (System.Net.Sockets.SocketException error) {
                _logger.Critical($"Failed to connect to database. {error.Message}", true);
            }
            _logger.Info("Preparing SQL commands...");
            _logger.Debug($"Preparing {statementType.Create}...");
            NpgsqlCommand createTask = new NpgsqlCommand("INSERT INTO assignments(task_type, guild_id, channel_id, message_id, user_id, set_off, set_at, content) VALUES(@taskType, @guildId, @channelId, @messageId, @userId, @setOff, @setAt, @content)", _connection);
            createTask.Parameters.Add(new NpgsqlParameter("taskType", NpgsqlDbType.Smallint));
            createTask.Parameters.Add(new NpgsqlParameter("guildId", NpgsqlDbType.Bigint));
            createTask.Parameters.Add(new NpgsqlParameter("channelId", NpgsqlDbType.Bigint));
            createTask.Parameters.Add(new NpgsqlParameter("messageId", NpgsqlDbType.Bigint));
            createTask.Parameters.Add(new NpgsqlParameter("userId", NpgsqlDbType.Bigint));
            createTask.Parameters.Add(new NpgsqlParameter("setOff", NpgsqlDbType.Timestamp));
            createTask.Parameters.Add(new NpgsqlParameter("setAt", NpgsqlDbType.Timestamp));
            createTask.Parameters.Add(new NpgsqlParameter("content", NpgsqlDbType.Varchar));
            createTask.Prepare();
            _preparedStatements.Add(statementType.Create, createTask);

            _logger.Debug($"Preparing {statementType.Remove}...");
            NpgsqlCommand removeTask = new NpgsqlCommand("DELETE FROM assignments WHERE id=@taskId", _connection);
            removeTask.Parameters.Add(new NpgsqlParameter("taskId", NpgsqlDbType.Integer));
            removeTask.Prepare();
            _preparedStatements.Add(statementType.Remove, removeTask);

            _logger.Debug($"Preparing {statementType.SelectTask}...");
            NpgsqlCommand selectTask = new NpgsqlCommand("SELECT task_type, guild_id, channel_id, message_id, user_id, set_off, set_at, content, id FROM assignments WHERE user_id=@userId AND task_type=@taskType", _connection);
            selectTask.Parameters.Add(new NpgsqlParameter("userId", NpgsqlDbType.Bigint));
            selectTask.Parameters.Add(new NpgsqlParameter("taskType", NpgsqlDbType.Smallint));
            selectTask.Prepare();
            _preparedStatements.Add(statementType.SelectTask, selectTask);

            _logger.Debug($"Preparing {statementType.SelectTaskById}...");
            NpgsqlCommand selectTaskById = new NpgsqlCommand("SELECT task_type, guild_id, channel_id, message_id, user_id, set_off, set_at, content, id FROM assignments WHERE user_id=@userId AND task_type=@taskType AND id=@taskId", _connection);
            selectTaskById.Parameters.Add(new NpgsqlParameter("userId", NpgsqlDbType.Bigint));
            selectTaskById.Parameters.Add(new NpgsqlParameter("taskType", NpgsqlDbType.Smallint));
            selectTaskById.Parameters.Add(new NpgsqlParameter("taskId", NpgsqlDbType.Integer));
            selectTaskById.Prepare();
            _preparedStatements.Add(statementType.SelectTaskById, selectTaskById);

            _logger.Debug($"Preparing {statementType.SelectAllAssignments}...");
            NpgsqlCommand selectAllAssignments = new NpgsqlCommand("SELECT * FROM assignments", selectRoutineConnection);
            selectAllAssignments.Prepare();
            _preparedStatements.Add(statementType.SelectAllAssignments, selectAllAssignments);

            _logger.Debug($"Preparing {statementType.SelectAllReminders}...");
            NpgsqlCommand selectAllReminders = new NpgsqlCommand("SELECT * FROM assignments WHERE user_id=@userId", _connection);
            selectAllReminders.Parameters.Add(new NpgsqlParameter("userId", NpgsqlDbType.Bigint));
            selectAllReminders.Prepare();
            _preparedStatements.Add(statementType.SelectAllReminders, selectAllReminders);
        }
        public void Create(AssignmentType taskType, ulong guildId, ulong channelId, ulong messageId, ulong userId, DateTime setOff, DateTime setAt, string content) => executeQuery(statementType.Create, new List<NpgsqlParameter>() { new NpgsqlParameter("taskType", (int) taskType), new NpgsqlParameter("guildId", (long) guildId), new NpgsqlParameter("channelId", (long) channelId), new NpgsqlParameter("messageId", (long) messageId), new NpgsqlParameter("userId", (long) userId), new NpgsqlParameter("setOff", setOff), new NpgsqlParameter("setAt", setAt), new NpgsqlParameter("content", content) });
        public void Remove(int taskId) => executeQuery(statementType.Remove, new NpgsqlParameter("taskId", taskId));
        public Assignment[] Select(ulong userId, AssignmentType taskType) {
            Dictionary<int, List<dynamic>> queryResults = executeQuery(statementType.SelectTask, new List<NpgsqlParameter>() { new NpgsqlParameter("userId", (long) userId), new NpgsqlParameter("taskType", (int) taskType) }, true);
            if (queryResults == null) return null;
            List<Assignment> assignments = new List<Assignment>();
            foreach (int i in queryResults.Keys) { // Order can be determined from the SQL query and how it was selected.
                Assignment assignment = new Assignment();
                assignment.TaskType = (AssignmentType) queryResults[i][0];
                assignment.GuildId = (ulong) queryResults[i][1];
                assignment.ChannelId = (ulong) queryResults[i][2];
                assignment.MessageId = (ulong) queryResults[i][3];
                assignment.UserId = (ulong) queryResults[i][4];
                assignment.SetOff = (DateTime) queryResults[i][5];
                assignment.SetAt = (DateTime) queryResults[i][6];
                assignment.Content = queryResults[i][7].ToString();
                assignment.TaskId = (int) queryResults[i][8];
                assignments.Add(assignment);
            }
            return assignments.ToArray();
        }

        public Assignment? Select(ulong userId, AssignmentType taskType, int taskId) {
            Dictionary<int, List<dynamic>> queryResults = executeQuery(statementType.SelectTask, new List<NpgsqlParameter>() { new NpgsqlParameter("userId", (long) userId), new NpgsqlParameter("taskType", (int) taskType), new NpgsqlParameter("taskId", taskId) }, true);
            if (queryResults == null) return null;
            Assignment assignment = new Assignment();
            foreach (int key in queryResults.Keys) {
                assignment.TaskType = (AssignmentType) queryResults[key][0];
                assignment.GuildId = (ulong) queryResults[key][1];
                assignment.ChannelId = (ulong) queryResults[key][2];
                assignment.MessageId = (ulong) queryResults[key][3];
                assignment.UserId = (ulong) queryResults[key][4];
                assignment.SetOff = (DateTime) queryResults[key][5];
                assignment.SetAt = (DateTime) queryResults[key][6];
                assignment.Content = queryResults[key][7].ToString();
                assignment.TaskId = (int) queryResults[key][8];
            }
            return assignment;
        }

        public Assignment[] SelectAllAssignments() {
            Dictionary<int, List<dynamic>> queryResults = executeQuery(statementType.SelectAllAssignments, new List<NpgsqlParameter>(), true);
            if (queryResults == null) return null;
            List<Assignment> assignments = new List<Assignment>();
            foreach (int i in queryResults.Keys) { // Order can be determined from the SQL query and how it was selected.
                Assignment assignment = new Assignment();
                assignment.TaskType = (AssignmentType) queryResults[i][0];
                assignment.GuildId = (ulong) queryResults[i][1];
                assignment.ChannelId = (ulong) queryResults[i][2];
                assignment.MessageId = (ulong) queryResults[i][3];
                assignment.UserId = (ulong) queryResults[i][4];
                assignment.SetOff = (DateTime) queryResults[i][5];
                assignment.SetAt = (DateTime) queryResults[i][6];
                assignment.Content = queryResults[i][7].ToString();
                assignment.TaskId = (int) queryResults[i][8];
                assignments.Add(assignment);
            }
            return assignments.ToArray();
        }

        public Assignment[] SelectAllReminders(ulong userId) {
            Dictionary<int, List<dynamic>> queryResults = executeQuery(statementType.SelectAllReminders, new NpgsqlParameter("userId", (long) userId), true);
            if (queryResults == null) return null;
            List<Assignment> assignments = new List<Assignment>();
            foreach (int i in queryResults.Keys) { // Order can be determined from the SQL query and how it was selected.
                Assignment assignment = new Assignment();
                assignment.TaskType = (AssignmentType) queryResults[i][0];
                assignment.GuildId = (ulong) queryResults[i][1];
                assignment.ChannelId = (ulong) queryResults[i][2];
                assignment.MessageId = (ulong) queryResults[i][3];
                assignment.UserId = (ulong) queryResults[i][4];
                assignment.SetOff = (DateTime) queryResults[i][5];
                assignment.SetAt = (DateTime) queryResults[i][6];
                assignment.Content = queryResults[i][7].ToString();
                assignment.TaskId = (int) queryResults[i][8];
                assignments.Add(assignment);
            }
            return assignments.ToArray();
        }
    }
}