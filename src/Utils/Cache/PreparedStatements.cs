using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml;
using Npgsql;

namespace Tomoe.Utils.Cache {
    public class PreparedStatements {

        public enum IndexedCommands {
            GetMuteRole,
            SetMuteRole,
            GetGuild,
            SetGuild,
            GetLoggingChannel,
            SetLoggingChannel,
            GetTask,
            GetAllTasks,
            RemoveTask,
            SetTask
        }

        public struct Query {
            public NpgsqlCommand Command;
            public Dictionary<string, NpgsqlParameter> Parameters;
        }

        private static XmlNode postgresSettings = Program.Tokens.DocumentElement.SelectSingleNode("postgres");
        public static NpgsqlConnection activeConnection = new NpgsqlConnection($"Host={postgresSettings.Attributes["host"].Value};Port={postgresSettings.Attributes["port"].Value};Username={postgresSettings.Attributes["username"].Value};Password={postgresSettings.Attributes["password"].Value};Database={postgresSettings.Attributes["database"].Value};SSL Mode={postgresSettings.Attributes["ssl_mode"].Value}");
        public static NpgsqlConnection repeatedConnection = new NpgsqlConnection($"Host={postgresSettings.Attributes["host"].Value};Port={postgresSettings.Attributes["port"].Value};Username={postgresSettings.Attributes["username"].Value};Password={postgresSettings.Attributes["password"].Value};Database={postgresSettings.Attributes["database"].Value};SSL Mode={postgresSettings.Attributes["ssl_mode"].Value}");
        public Dictionary<IndexedCommands, Query> Statements = new Dictionary<IndexedCommands, Query>();

        public PreparedStatements() {
            activeConnection.Open();
            repeatedConnection.Open();

            Query getGuild = new Query();
            getGuild.Command = new NpgsqlCommand("SELECT guild_id FROM guild_config WHERE guild_id=@guildID;", activeConnection);
            getGuild.Parameters = new Dictionary<string, NpgsqlParameter>();
            getGuild.Parameters.Add("guildID", getGuild.Command.Parameters.Add("guildID", NpgsqlTypes.NpgsqlDbType.Text));
            getGuild.Command.Prepare();
            Statements.Add(IndexedCommands.GetGuild, getGuild);

            Query setGuild = new Query();
            setGuild.Command = new NpgsqlCommand("INSERT INTO guild_config(guild_id) VALUES(@guildID);", activeConnection);
            setGuild.Parameters = new Dictionary<string, NpgsqlParameter>();
            setGuild.Parameters.Add("guildID", setGuild.Command.Parameters.Add("guildID", NpgsqlTypes.NpgsqlDbType.Text));
            setGuild.Command.Prepare();
            Statements.Add(IndexedCommands.SetGuild, setGuild);

            Query getMuteRole = new Query();
            getMuteRole.Command = new NpgsqlCommand("SELECT mute_role FROM guild_config WHERE guild_id=@guildID;", activeConnection);
            getMuteRole.Parameters = new Dictionary<string, NpgsqlParameter>();
            getMuteRole.Parameters.Add("guildID", getMuteRole.Command.Parameters.Add("guildID", NpgsqlTypes.NpgsqlDbType.Text));
            getMuteRole.Command.Prepare();
            Statements.Add(IndexedCommands.GetMuteRole, getMuteRole);

            Query setMuteRole = new Query();
            setMuteRole.Command = new NpgsqlCommand("UPDATE guild_config SET mute_role=@muteInfo WHERE guild_id=@guildID;", activeConnection);
            setMuteRole.Parameters = new Dictionary<string, NpgsqlParameter>();
            setMuteRole.Parameters.Add("muteInfo", setMuteRole.Command.Parameters.Add("muteInfo", NpgsqlTypes.NpgsqlDbType.Text));
            setMuteRole.Parameters.Add("guildID", setMuteRole.Command.Parameters.Add("guildID", NpgsqlTypes.NpgsqlDbType.Text));
            setMuteRole.Command.Prepare();
            Statements.Add(IndexedCommands.SetMuteRole, setMuteRole);

            Query getLoggingChannel = new Query();
            getLoggingChannel.Command = new NpgsqlCommand("SELECT logging_channels -> @guildEvent FROM guild_config WHERE guild_id=@guildID", activeConnection);
            getLoggingChannel.Parameters = new Dictionary<string, NpgsqlParameter>();
            getLoggingChannel.Parameters.Add("guildID", getLoggingChannel.Command.Parameters.Add("guildID", NpgsqlTypes.NpgsqlDbType.Text));
            getLoggingChannel.Parameters.Add("guildEvent", getLoggingChannel.Command.Parameters.Add("guildEvent", NpgsqlTypes.NpgsqlDbType.Text));
            getLoggingChannel.Command.Prepare();
            Statements.Add(IndexedCommands.GetLoggingChannel, getLoggingChannel);

            Query setLoggingChannel = new Query();
            setLoggingChannel.Command = new NpgsqlCommand("UPDATE guild_config SET logging_channels = @loggingChannel WHERE guild_id=@guildID", activeConnection);
            setLoggingChannel.Parameters = new Dictionary<string, NpgsqlParameter>();
            setLoggingChannel.Parameters.Add("guildID", setLoggingChannel.Command.Parameters.Add("guildID", NpgsqlTypes.NpgsqlDbType.Text));
            setLoggingChannel.Parameters.Add("loggingChannel", setLoggingChannel.Command.Parameters.Add("loggingChannel", NpgsqlTypes.NpgsqlDbType.Hstore));
            setLoggingChannel.Command.Prepare();
            Statements.Add(IndexedCommands.SetLoggingChannel, setLoggingChannel);

            Query getTask = new Query();
            getTask.Command = new NpgsqlCommand("SELECT * FROM tasks WHERE user_id = @userID AND type = @taskType", activeConnection);
            getTask.Parameters = new Dictionary<string, NpgsqlParameter>();
            getTask.Parameters.Add("taskType", getTask.Command.Parameters.Add("taskType", NpgsqlTypes.NpgsqlDbType.Smallint));
            getTask.Parameters.Add("userID", getTask.Command.Parameters.Add("userID", NpgsqlTypes.NpgsqlDbType.Bigint));
            getTask.Command.Prepare();
            Statements.Add(IndexedCommands.GetTask, getTask);

            Query getAllTasks = new Query();
            getAllTasks.Command = new NpgsqlCommand("SELECT * FROM tasks", repeatedConnection);
            getAllTasks.Command.Prepare();
            Statements.Add(IndexedCommands.GetAllTasks, getAllTasks);

            Query setTask = new Query();
            setTask.Command = new NpgsqlCommand("INSERT INTO tasks VALUES(@taskType, @guildID, @channelID, @userID, @setOff, @setAt, @content)", activeConnection);
            setTask.Parameters = new Dictionary<string, NpgsqlParameter>();
            setTask.Parameters.Add("taskType", setTask.Command.Parameters.Add("taskType", NpgsqlTypes.NpgsqlDbType.Smallint));
            setTask.Parameters.Add("guildID", setTask.Command.Parameters.Add("guildID", NpgsqlTypes.NpgsqlDbType.Bigint));
            setTask.Parameters.Add("channelID", setTask.Command.Parameters.Add("channelID", NpgsqlTypes.NpgsqlDbType.Bigint));
            setTask.Parameters.Add("userID", setTask.Command.Parameters.Add("userID", NpgsqlTypes.NpgsqlDbType.Bigint));
            setTask.Parameters.Add("setOff", setTask.Command.Parameters.Add("setOff", NpgsqlTypes.NpgsqlDbType.Timestamp));
            setTask.Parameters.Add("setAt", setTask.Command.Parameters.Add("setAt", NpgsqlTypes.NpgsqlDbType.Timestamp));
            setTask.Parameters.Add("content", setTask.Command.Parameters.Add("content", NpgsqlTypes.NpgsqlDbType.Text));
            setTask.Command.Prepare();
            Statements.Add(IndexedCommands.SetTask, setTask);

            Query removeTask = new Query();
            removeTask.Command = new NpgsqlCommand("delete from tasks WHERE user_id=@userID AND set_off=@setOff AND set_at=@setAt AND type=@taskType", repeatedConnection);
            removeTask.Parameters = new Dictionary<string, NpgsqlParameter>();
            removeTask.Parameters.Add("taskType", removeTask.Command.Parameters.Add("taskType", NpgsqlTypes.NpgsqlDbType.Smallint));
            removeTask.Parameters.Add("userID", removeTask.Command.Parameters.Add("userID", NpgsqlTypes.NpgsqlDbType.Bigint));
            removeTask.Parameters.Add("setAt", removeTask.Command.Parameters.Add("setAt", NpgsqlTypes.NpgsqlDbType.Timestamp));
            removeTask.Parameters.Add("setOff", removeTask.Command.Parameters.Add("setOff", NpgsqlTypes.NpgsqlDbType.Timestamp));
            removeTask.Command.Prepare();
            Statements.Add(IndexedCommands.RemoveTask, removeTask);
        }

        public static void TestConnection() {
            activeConnection.Open();
            NpgsqlCommand create_config_table = new NpgsqlCommand(System.IO.File.ReadAllText("src/SQL/guild_config_table.sql"), activeConnection);
            var task = Task.Run(() => create_config_table.ExecuteNonQuery());
            if (task.Wait(TimeSpan.FromSeconds(10))) {
                create_config_table.Dispose();
                NpgsqlCommand create_cache_table = new NpgsqlCommand(System.IO.File.ReadAllText("src/SQL/guild_cache_table.sql"), activeConnection);
                create_cache_table.ExecuteNonQuery();
                create_cache_table.Dispose();
                NpgsqlCommand create_tasks_table = new NpgsqlCommand(System.IO.File.ReadAllText("src/SQL/tasks.sql"), activeConnection);
                create_tasks_table.ExecuteNonQuery();
                create_tasks_table.Dispose();
                activeConnection.Close();
                Console.WriteLine("[Database] Connected!");
            } else {
                create_config_table.Dispose();
                activeConnection.Close();
                Console.WriteLine("[Database] Can't connect. Exiting...");
                Environment.Exit(1);
            }
        }
    }
}