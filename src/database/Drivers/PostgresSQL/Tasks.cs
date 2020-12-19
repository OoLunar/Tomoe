using System;
using System.Collections.Generic;
using System.IO;
using Npgsql;
using NpgsqlTypes;
using Tomoe.Database.Interfaces;
using Tomoe.Utils;

namespace Tomoe.Database.Drivers.PostgresSQL {
    public class PostgresTasks : ITasks {
        private static Logger _logger = new Logger("Database/PostgresSQL/User");
        private enum statementType {
            CreateTask,
            RemoveTask,
            SelectTask,
            SelectAllReminders,
            SelectAllTasks
        }

        private Dictionary<statementType, NpgsqlCommand> _preparedStatements = new Dictionary<statementType, NpgsqlCommand>();

        /// <summary>
        /// Executes an SQL query from <see cref="Tomoe.Database.Drivers.PostgresSQL.PostgresTasks._preparedStatements">_preparedStatements</see>, using <seealso cref="Tomoe.Database.Drivers.PostgresSQL.PostgresTasks.statementType">statementType</seealso> as a key.
        /// 
        /// Returns a list of results if <paramref name="needsResult">needsResult</paramref> is true, otherwise returns null.
        /// </summary>
        /// <param name="command">Which SQL command to execute, using <see cref="Tomoe.Database.Drivers.PostgresSQL.PostgresTasks.statementType">statementType</see> as an index.</param>
        /// <param name="parameters">A list of <see cref="Npgsql.NpgsqlParameter">NpgsqlParameter's</see>.</param>
        /// <param name="needsResult">Returns a list of results if true, otherwise returns null.</param>
        /// <returns><see cref="System.Collections.Generic.List{T}">List&lt;dynamic&gt;</see> if <paramref name="needsResult">needsResult</paramref> is true, otherwise returns null.</returns>
        private Dictionary<int, List<dynamic>> executeQuery(statementType command, List<NpgsqlParameter> parameters, bool needsResult = false) {
            _logger.Trace($"Executing prepared statement \"{command.ToString()}\"");
            NpgsqlCommand statement = _preparedStatements[command];
            Dictionary<string, NpgsqlParameter> sortedParameters = new Dictionary<string, NpgsqlParameter>();
            foreach (NpgsqlParameter param in parameters) sortedParameters.Add(param.ParameterName, param);
            foreach (NpgsqlParameter temp in statement.Parameters) temp.Value = sortedParameters[temp.ParameterName].Value;
            if (needsResult) {
                NpgsqlDataReader reader = statement.ExecuteReader();
                Dictionary<int, List<dynamic>> values = new Dictionary<int, List<dynamic>>();
                while (reader.Read())
                    for (int i = 0; i < reader.FieldCount; i++) {
                        List<dynamic> list = new List<dynamic>();
                        if (reader[i].GetType() == typeof(System.DBNull))
                            list.Add(null);
                        else
                            list.Add(reader[i]);
                        values.Add(i, list);
                    }
                reader.DisposeAsync().ConfigureAwait(false).GetAwaiter();
                if (values.Count == 0) values = null;
                return values;
            } else {
                statement.ExecuteNonQuery();
                return null;
            }
        }

        /// <inheritdoc cref="Tomoe.Database.Drivers.PostgresSQL.PostgresTasks.executeQuery(statementType, List{NpgsqlParameter}, bool)" />
        /// <param name="parameter">One <see cref="Npgsql.NpgsqlParameter">NpgsqlParameter</see>, which gets converted into a <see cref="System.Collections.Generic.List{T}">List&lt;NpgsqlParameter&gt;</see>.</param>
        private Dictionary<int, List<dynamic>> executeQuery(statementType command, NpgsqlParameter parameter, bool needsResult = false) => executeQuery(command, new List<NpgsqlParameter> { parameter }, needsResult);

        private NpgsqlConnection _connection;

        public PostgresTasks(string host, int port, string username, string password, string database_name, SslMode sslMode) {
            _connection = new NpgsqlConnection($"Host={host};Port={port};Username={username};Password={password};Database={database_name};SSL Mode={sslMode}");
            _logger.Info("Opening connection to database...");
            try {
                _connection.Open();
                NpgsqlCommand createTagsTable = new NpgsqlCommand(File.ReadAllText(Path.Join(FileSystem.ProjectRoot, "res/sql/tasks.sql")), _connection);
                createTagsTable.ExecuteNonQuery();
                createTagsTable.Dispose();
            } catch (System.Net.Sockets.SocketException error) {
                _logger.Critical($"Failed to connect to database. {error.Message}", true);
            }
            _logger.Trace($"Preparing {statementType.CreateTask}...");
            _logger.Info("Preparing SQL commands...");
            NpgsqlCommand createTask = new NpgsqlCommand("INSERT INTO tasks VALUES(@taskType, @guildId, @channelId, @userId, @setOff, @setAt, @content)", _connection);
            createTask.Parameters.Add(new NpgsqlParameter("taskType", NpgsqlDbType.Smallint));
            createTask.Parameters.Add(new NpgsqlParameter("guildId", NpgsqlDbType.Bigint));
            createTask.Parameters.Add(new NpgsqlParameter("channelId", NpgsqlDbType.Bigint));
            createTask.Parameters.Add(new NpgsqlParameter("userId", NpgsqlDbType.Bigint));
            createTask.Parameters.Add(new NpgsqlParameter("setOff", NpgsqlDbType.Timestamp));
            createTask.Parameters.Add(new NpgsqlParameter("setAt", NpgsqlDbType.Timestamp));
            createTask.Parameters.Add(new NpgsqlParameter("content", NpgsqlDbType.Varchar));
            createTask.Prepare();
            _preparedStatements.Add(statementType.CreateTask, createTask);

            _logger.Trace($"Preparing {statementType.RemoveTask}...");
            NpgsqlCommand removeTask = new NpgsqlCommand("DELETE FROM tasks WHERE user_id=@userId AND set_off=@setOff AND set_at=@setAt AND type=@taskType", _connection);
            removeTask.Parameters.Add(new NpgsqlParameter("userId", NpgsqlDbType.Bigint));
            removeTask.Parameters.Add(new NpgsqlParameter("setOff", NpgsqlDbType.Timestamp));
            removeTask.Parameters.Add(new NpgsqlParameter("setAt", NpgsqlDbType.Timestamp));
            removeTask.Parameters.Add(new NpgsqlParameter("taskType", NpgsqlDbType.Smallint));
            removeTask.Prepare();
            _preparedStatements.Add(statementType.RemoveTask, removeTask);

            _logger.Trace($"Preparing {statementType.SelectTask}...");
            NpgsqlCommand selectTask = new NpgsqlCommand("SELECT tasks(task_type, guild_id, channel_id, user_id, set_off, set_at, content) FROM tasks WHERE user_id=@userId AND type=@taskType", _connection);
            selectTask.Parameters.Add(new NpgsqlParameter("userId", NpgsqlDbType.Bigint));
            selectTask.Parameters.Add(new NpgsqlParameter("taskType", NpgsqlDbType.Smallint));
            selectTask.Prepare();
            _preparedStatements.Add(statementType.SelectTask, selectTask);

            _logger.Trace($"Preparing {statementType.SelectAllTasks}...");
            NpgsqlCommand selectAllTasks = new NpgsqlCommand("SELECT * FROM tasks", _connection);
            selectAllTasks.Prepare();
            _preparedStatements.Add(statementType.SelectAllTasks, selectAllTasks);

            _logger.Trace($"Preparing {statementType.SelectAllReminders}...");
            NpgsqlCommand selectAllReminders = new NpgsqlCommand("SELECT * FROM tasks WHERE user_id=@userId", _connection);
            selectAllReminders.Parameters.Add(new NpgsqlParameter("userId", NpgsqlDbType.Bigint));
            selectAllReminders.Prepare();
            _preparedStatements.Add(statementType.SelectAllReminders, selectAllReminders);
        }
        public void Create(TaskType taskType, ulong guildId, ulong channelId, ulong userId, DateTime setOff, DateTime setAt, string content) => executeQuery(statementType.CreateTask, new List<NpgsqlParameter>() { new NpgsqlParameter("taskType", (int) taskType), new NpgsqlParameter("guildId", (long) guildId), new NpgsqlParameter("channelId", (long) channelId), new NpgsqlParameter("userId", (long) userId), new NpgsqlParameter("setOff", setOff), new NpgsqlParameter("setAt", setAt), new NpgsqlParameter("content", content) });
        public void Remove(ulong userId, DateTime setAt, DateTime setOff, TaskType taskType) => executeQuery(statementType.RemoveTask, new List<NpgsqlParameter>() { new NpgsqlParameter("userId", (long) userId), new NpgsqlParameter("setAt", setAt), new NpgsqlParameter("setOff", setOff), new NpgsqlParameter("taskType", (int) taskType) });
        public Task[] Select(ulong userId, TaskType taskType) {
            Dictionary<int, List<dynamic>> queryResults = executeQuery(statementType.SelectTask, new List<NpgsqlParameter>() { new NpgsqlParameter("userId", (long) userId), new NpgsqlParameter("taskType", (int) taskType) }, true);
            List<Task> tasks = new List<Task>();
            foreach (int i in queryResults.Keys) {
                foreach (dynamic result in queryResults.Values) { // Order can be determined from the SQL query and how it was selected.
                    Task task = new Task();
                    task.TaskType = (TaskType) queryResults[i][0];
                    task.GuildId = ulong.Parse(queryResults[i][1]);
                    task.ChannelId = ulong.Parse(queryResults[i][2]);
                    task.UserId = ulong.Parse(queryResults[i][3]);
                    task.SetOff = DateTime.Parse(queryResults[i][4]);
                    task.SetAt = DateTime.Parse(queryResults[i][5]);
                    task.Content = queryResults[i][6].ToString();
                    tasks.Add(task);
                }
            }
            return tasks.ToArray();
        }
        public Task[] SelectAllTasks() {
            Dictionary<int, List<dynamic>> queryResults = executeQuery(statementType.SelectAllTasks, new List<NpgsqlParameter>(), true);
            List<Task> tasks = new List<Task>();
            foreach (int i in queryResults.Keys) {
                foreach (dynamic result in queryResults.Values) { // Order can be determined from the SQL query and how it was selected.
                    Task task = new Task();
                    task.TaskType = (TaskType) queryResults[i][0];
                    task.GuildId = ulong.Parse(queryResults[i][1]);
                    task.ChannelId = ulong.Parse(queryResults[i][2]);
                    task.UserId = ulong.Parse(queryResults[i][3]);
                    task.SetOff = DateTime.Parse(queryResults[i][4]);
                    task.SetAt = DateTime.Parse(queryResults[i][5]);
                    task.Content = queryResults[i][6].ToString();
                    tasks.Add(task);
                }
            }
            return tasks.ToArray();
        }

        public Task[] SelectAllReminders(ulong userId) {
            Dictionary<int, List<dynamic>> queryResults = executeQuery(statementType.SelectAllReminders, new NpgsqlParameter("userId", (long) userId), true);
            List<Task> tasks = new List<Task>();
            foreach (int i in queryResults.Keys) {
                foreach (dynamic result in queryResults.Values) { // Order can be determined from the SQL query and how it was selected.
                    Task task = new Task();
                    task.TaskType = (TaskType) queryResults[i][0];
                    task.GuildId = ulong.Parse(queryResults[i][1]);
                    task.ChannelId = ulong.Parse(queryResults[i][2]);
                    task.UserId = ulong.Parse(queryResults[i][3]);
                    task.SetOff = DateTime.Parse(queryResults[i][4]);
                    task.SetAt = DateTime.Parse(queryResults[i][5]);
                    task.Content = queryResults[i][6].ToString();
                    tasks.Add(task);
                }
            }
            return tasks.ToArray();
        }
    }
}