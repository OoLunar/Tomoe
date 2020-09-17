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
            SetTask,
            GetAntiraidActivated,
            SetAntiraidActivated,
            GetAntiraidInterval,
            SetAntiraidInterval,
            GetUserRoles,
            SetUserRoles,
            SetUserMute,
            GetUserMute
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
            getGuild.Parameters.Add("guildID", getGuild.Command.Parameters.Add("guildID", NpgsqlTypes.NpgsqlDbType.Bigint));
            getGuild.Command.Prepare();
            Statements.Add(IndexedCommands.GetGuild, getGuild);

            Query setGuild = new Query();
            setGuild.Command = new NpgsqlCommand("INSERT INTO guild_config(guild_id) VALUES(@guildID);", activeConnection);
            setGuild.Parameters = new Dictionary<string, NpgsqlParameter>();
            setGuild.Parameters.Add("guildID", setGuild.Command.Parameters.Add("guildID", NpgsqlTypes.NpgsqlDbType.Bigint));
            setGuild.Command.Prepare();
            Statements.Add(IndexedCommands.SetGuild, setGuild);

            Query getMuteRole = new Query();
            getMuteRole.Command = new NpgsqlCommand("SELECT mute_role FROM guild_config WHERE guild_id=@guildID;", activeConnection);
            getMuteRole.Parameters = new Dictionary<string, NpgsqlParameter>();
            getMuteRole.Parameters.Add("guildID", getMuteRole.Command.Parameters.Add("guildID", NpgsqlTypes.NpgsqlDbType.Bigint));
            getMuteRole.Command.Prepare();
            Statements.Add(IndexedCommands.GetMuteRole, getMuteRole);

            Query setMuteRole = new Query();
            setMuteRole.Command = new NpgsqlCommand("UPDATE guild_config SET mute_role=@muteRole WHERE guild_id=@guildID;", activeConnection);
            setMuteRole.Parameters = new Dictionary<string, NpgsqlParameter>();
            setMuteRole.Parameters.Add("muteRole", setMuteRole.Command.Parameters.Add("muteRole", NpgsqlTypes.NpgsqlDbType.Hstore));
            setMuteRole.Parameters.Add("guildID", setMuteRole.Command.Parameters.Add("guildID", NpgsqlTypes.NpgsqlDbType.Bigint));
            setMuteRole.Command.Prepare();
            Statements.Add(IndexedCommands.SetMuteRole, setMuteRole);

            Query getLoggingChannel = new Query();
            getLoggingChannel.Command = new NpgsqlCommand("SELECT logging_channels -> @guildEvent FROM guild_config WHERE guild_id=@guildID", activeConnection);
            getLoggingChannel.Parameters = new Dictionary<string, NpgsqlParameter>();
            getLoggingChannel.Parameters.Add("guildID", getLoggingChannel.Command.Parameters.Add("guildID", NpgsqlTypes.NpgsqlDbType.Bigint));
            getLoggingChannel.Parameters.Add("guildEvent", getLoggingChannel.Command.Parameters.Add("guildEvent", NpgsqlTypes.NpgsqlDbType.Text));
            getLoggingChannel.Command.Prepare();
            Statements.Add(IndexedCommands.GetLoggingChannel, getLoggingChannel);

            Query setLoggingChannel = new Query();
            setLoggingChannel.Command = new NpgsqlCommand("UPDATE guild_config SET logging_channels = @loggingChannel WHERE guild_id=@guildID", activeConnection);
            setLoggingChannel.Parameters = new Dictionary<string, NpgsqlParameter>();
            setLoggingChannel.Parameters.Add("guildID", setLoggingChannel.Command.Parameters.Add("guildID", NpgsqlTypes.NpgsqlDbType.Bigint));
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
            removeTask.Command = new NpgsqlCommand("DELETE FROM tasks WHERE user_id=@userID AND set_off=@setOff AND set_at=@setAt AND type=@taskType", repeatedConnection);
            removeTask.Parameters = new Dictionary<string, NpgsqlParameter>();
            removeTask.Parameters.Add("taskType", removeTask.Command.Parameters.Add("taskType", NpgsqlTypes.NpgsqlDbType.Smallint));
            removeTask.Parameters.Add("userID", removeTask.Command.Parameters.Add("userID", NpgsqlTypes.NpgsqlDbType.Bigint));
            removeTask.Parameters.Add("setAt", removeTask.Command.Parameters.Add("setAt", NpgsqlTypes.NpgsqlDbType.Timestamp));
            removeTask.Parameters.Add("setOff", removeTask.Command.Parameters.Add("setOff", NpgsqlTypes.NpgsqlDbType.Timestamp));
            removeTask.Command.Prepare();
            Statements.Add(IndexedCommands.RemoveTask, removeTask);

            Query getAntiraidActivated = new Query();
            getAntiraidActivated.Command = new NpgsqlCommand("SELECT antiraid FROM guild_config WHERE guild_id=@guildID", activeConnection);
            getAntiraidActivated.Parameters = new Dictionary<string, NpgsqlParameter>();
            getAntiraidActivated.Parameters.Add("guildID", getAntiraidActivated.Command.Parameters.Add("guildID", NpgsqlTypes.NpgsqlDbType.Bigint));
            getAntiraidActivated.Command.Prepare();
            Statements.Add(IndexedCommands.GetAntiraidActivated, getAntiraidActivated);

            Query setAntitaidActivated = new Query();
            setAntitaidActivated.Command = new NpgsqlCommand("UPDATE guild_config SET antiraid=@activated WHERE guild_id=@guildID", activeConnection);
            setAntitaidActivated.Parameters = new Dictionary<string, NpgsqlParameter>();
            setAntitaidActivated.Parameters.Add("guildID", setAntitaidActivated.Command.Parameters.Add("guildID", NpgsqlTypes.NpgsqlDbType.Bigint));
            setAntitaidActivated.Parameters.Add("activated", setAntitaidActivated.Command.Parameters.Add("activated", NpgsqlTypes.NpgsqlDbType.Boolean));
            setAntitaidActivated.Command.Prepare();
            Statements.Add(IndexedCommands.SetAntiraidActivated, setAntitaidActivated);

            Query getAntiraidInterval = new Query();
            getAntiraidInterval.Command = new NpgsqlCommand("SELECT antiraid_setoff FROM guild_config WHERE guild_id=@guildID", activeConnection);
            getAntiraidInterval.Parameters = new Dictionary<string, NpgsqlParameter>();
            getAntiraidInterval.Parameters.Add("guildID", getAntiraidInterval.Command.Parameters.Add("guildID", NpgsqlTypes.NpgsqlDbType.Bigint));
            getAntiraidInterval.Command.Prepare();
            Statements.Add(IndexedCommands.GetAntiraidInterval, getAntiraidInterval);

            Query setAntiraidInterval = new Query();
            setAntiraidInterval.Command = new NpgsqlCommand("UPDATE guild_config SET antiraid_setoff=@interval WHERE guild_id=@guildID", activeConnection);
            setAntiraidInterval.Parameters = new Dictionary<string, NpgsqlParameter>();
            setAntiraidInterval.Parameters.Add("guildID", setAntiraidInterval.Command.Parameters.Add("guildID", NpgsqlTypes.NpgsqlDbType.Bigint));
            setAntiraidInterval.Parameters.Add("interval", setAntiraidInterval.Command.Parameters.Add("interval", NpgsqlTypes.NpgsqlDbType.Smallint));
            setAntiraidInterval.Command.Prepare();
            Statements.Add(IndexedCommands.SetAntiraidInterval, setAntiraidInterval);

            Query setUserRoles = new Query();
            setUserRoles.Command = new NpgsqlCommand("UPDATE guild_cache SET role_ids=@roles WHERE user_id=@userID AND guild_id=@guildID", activeConnection);
            setUserRoles.Parameters = new Dictionary<string, NpgsqlParameter>();
            setUserRoles.Parameters.Add("guildID", setUserRoles.Command.Parameters.Add("guildID", NpgsqlTypes.NpgsqlDbType.Bigint));
            setUserRoles.Parameters.Add("userID", setUserRoles.Command.Parameters.Add("userID", NpgsqlTypes.NpgsqlDbType.Bigint));
            setUserRoles.Parameters.Add("roles", setUserRoles.Command.Parameters.Add("roles", (NpgsqlTypes.NpgsqlDbType.Array | NpgsqlTypes.NpgsqlDbType.Bigint)));
            setUserRoles.Command.Prepare();
            Statements.Add(IndexedCommands.SetUserRoles, setUserRoles);

            Query getUserRoles = new Query();
            getUserRoles.Command = new NpgsqlCommand("SELECT role_ids FROM guild_cache WHERE user_id=@userID AND guild_id=@guildID", activeConnection);
            getUserRoles.Parameters = new Dictionary<string, NpgsqlParameter>();
            getUserRoles.Parameters.Add("guildID", getUserRoles.Command.Parameters.Add("guildID", NpgsqlTypes.NpgsqlDbType.Bigint));
            getUserRoles.Parameters.Add("userID", getUserRoles.Command.Parameters.Add("userID", NpgsqlTypes.NpgsqlDbType.Bigint));
            getUserRoles.Command.Prepare();
            Statements.Add(IndexedCommands.GetUserRoles, getUserRoles);

            Query setUserMute = new Query();
            setUserMute.Command = new NpgsqlCommand("UPDATE guild_cache SET muted=@isMuted WHERE user_id=@userID AND guild_id=@guildID", activeConnection);
            setUserMute.Parameters = new Dictionary<string, NpgsqlParameter>();
            setUserMute.Parameters.Add("guildID", setUserMute.Command.Parameters.Add("guildID", NpgsqlTypes.NpgsqlDbType.Bigint));
            setUserMute.Parameters.Add("userID", setUserMute.Command.Parameters.Add("userID", NpgsqlTypes.NpgsqlDbType.Bigint));
            setUserMute.Parameters.Add("isMuted", setUserMute.Command.Parameters.Add("isMuted", NpgsqlTypes.NpgsqlDbType.Boolean));
            setUserMute.Command.Prepare();
            Statements.Add(IndexedCommands.SetUserMute, setUserMute);

            Query getUserMute = new Query();
            getUserMute.Command = new NpgsqlCommand("SELECT muted FROM guild_cache WHERE user_id=@userID AND guild_id=@guildID", activeConnection);
            getUserMute.Parameters = new Dictionary<string, NpgsqlParameter>();
            getUserMute.Parameters.Add("guildID", getUserMute.Command.Parameters.Add("guildID", NpgsqlTypes.NpgsqlDbType.Bigint));
            getUserMute.Parameters.Add("userID", getUserMute.Command.Parameters.Add("userID", NpgsqlTypes.NpgsqlDbType.Bigint));
            getUserMute.Command.Prepare();
            Statements.Add(IndexedCommands.GetUserMute, getUserMute);

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