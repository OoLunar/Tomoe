using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Npgsql;
using NpgsqlTypes;
using Tomoe.Database.Classes;

namespace Tomoe.Database {
    public class PostgresSQL : IDatabase {

        internal static NpgsqlConnection _activeConnection = new NpgsqlConnection($"Host={Program.Config.Database.Postgresql.Host};Port={Program.Config.Database.Postgresql.Port};Username={Program.Config.Database.Postgresql.Username};Password={Program.Config.Database.Postgresql.Password};Database={Program.Config.Database.Postgresql.DatabaseName};SSL Mode={Program.Config.Database.Postgresql.SslMode}");
        internal static NpgsqlConnection _repeatedConnection = new NpgsqlConnection($"Host={Program.Config.Database.Postgresql.Host};Port={Program.Config.Database.Postgresql.Port};Username={Program.Config.Database.Postgresql.Username};Password={Program.Config.Database.Postgresql.Password};Database={Program.Config.Database.Postgresql.DatabaseName};SSL Mode={Program.Config.Database.Postgresql.SslMode}");
        private PreparedStatments PreparedStatments = new PreparedStatments();
        private static Logger Logger = new Logger("Database/PostgreSQL");

        // Test connection and create tables
        public PostgresSQL() {
            try {
                _activeConnection.Open();
            } catch (System.Exception error) when(error.Message == "Resource temporarily unavailable" && error.Source == "System.Net.NameResolution") {
                Logger.Error("Cannot connect. \"Resource temporarily unavailable.\" Try checking your internet connection?");
                Environment.Exit(1);
            }

            NpgsqlCommand create_config_table = new NpgsqlCommand(File.ReadAllText(Path.Join(Program.ProjectRoot, "src/res/sql/guild_config_table.sql")), _activeConnection);
            Task<int> task = System.Threading.Tasks.Task.Run(() => create_config_table.ExecuteNonQuery());
            if (task.Wait(TimeSpan.FromSeconds(10))) {
                create_config_table.Dispose();
                NpgsqlCommand create_cache_table = new NpgsqlCommand(File.ReadAllText(Path.Join(Program.ProjectRoot, "src/res/sql/guild_cache_table.sql")), _activeConnection);
                create_cache_table.ExecuteNonQuery();
                create_cache_table.Dispose();
                NpgsqlCommand create_tasks_table = new NpgsqlCommand(File.ReadAllText(Path.Join(Program.ProjectRoot, "src/res/sql/tasks.sql")), _activeConnection);
                create_tasks_table.ExecuteNonQuery();
                create_tasks_table.Dispose();
                _activeConnection.Close();
                Logger.Info("Connected!");
            } else {
                create_config_table.Dispose();
                _activeConnection.Close();
                Logger.Error("Timed out on executing \"src/res/sql/guild_config_table.sql\". Exiting...");
                Environment.Exit(1);
            }
        }
    }

    class PreparedStatments {
        public enum IndexedCommands {
            GetUserStrikes,
            AddUserStrike,
            RemoveUserStrike,
            SetUserStrikes,
            GetUserRoles,
            AddUserRole,
            RemoveUserRole,
            SetUserRoles,
            InsertGuildId,
            GetUserMuted,
            SetUserMuted,
            GetUserNoMemed,
            SetUserNoMemed,
            GetUserNoVoiceChat,
            SetUserNoVoiceChat,
            GetAntiInvite,
            SetAntiInvite,
            GetAntiDuplicate,
            SetAntiDuplicate,
            GetAllowedInvites,
            SetAllowedInvites,
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
            GetAdministraitiveRoles,
            SetAdministraitiveRoles,
            GetLoggingChannels,
            SetLoggingChannels,
            GetLoggingChannel,
            SetLoggingChannel,
            GetMuteRole,
            SetMuteRole,
            GetNoMemeRole,
            SetNoMemeRole,
            GetNoVoiceChatRole,
            SetNoVoiceChatRole,
            GetAntiRaidActive,
            SetAntiRaidActive,
            GetAntiRaidSetOff,
            SetAntiRaidSetOff,
            GetUserTasks,
            GetAllTasks,
            AddTask,
            RemoveTask,
        }

        public struct Query {
            public NpgsqlCommand Command;
            public Dictionary<string, NpgsqlParameter> Parameters;
        }

        public Dictionary<IndexedCommands, Query> Statements = new Dictionary<IndexedCommands, Query>();

        private void createPreparedStatement(string query, Dictionary<string, NpgsqlDbType> parameters, NpgsqlConnection connection, IndexedCommands Action) {
            Query statement = new Query();
            statement.Command = new NpgsqlCommand(query, connection);
            // parameters can be null, in which case there would be no params.
            if (parameters != null) {
                statement.Parameters = new Dictionary<string, NpgsqlParameter>();
                // Param can also be seen as a key
                foreach (string param in parameters.Keys) statement.Parameters.Add(param, statement.Command.Parameters.Add(param, parameters[param]));
            }
            statement.Command.Prepare();
            Statements.Add(Action, statement);
        }

        public PreparedStatments() {
            PostgresSQL._activeConnection.Open();
            PostgresSQL._repeatedConnection.Open();

            Dictionary<string, NpgsqlDbType> guildId = new Dictionary<string, NpgsqlDbType>() { { "guildId", NpgsqlDbType.Bigint } };
            Dictionary<string, NpgsqlDbType> userId = new Dictionary<string, NpgsqlDbType>() { { "userId", NpgsqlDbType.Bigint } };
            Dictionary<string, NpgsqlDbType> userAndGuildId = new Dictionary<string, NpgsqlDbType>() { { "guildId", NpgsqlDbType.Bigint }, { "userId", NpgsqlDbType.Bigint } };

            Dictionary<string, NpgsqlDbType> setStrikeCount = userAndGuildId.ToDictionary(entry => entry.Key, entry => entry.Value);
            setStrikeCount.Add("strikeCount", NpgsqlDbType.Smallint);

            Dictionary<string, NpgsqlDbType> muteRole = userAndGuildId.ToDictionary(entry => entry.Key, entry => entry.Value);
            muteRole.Add("muteRole", NpgsqlDbType.Hstore);

            Dictionary<string, NpgsqlDbType> getLoggingChannel = userAndGuildId.ToDictionary(entry => entry.Key, entry => entry.Value);
            getLoggingChannel.Add("guildEvent", NpgsqlDbType.Text);

            Dictionary<string, NpgsqlDbType> setloggingChannel = userAndGuildId.ToDictionary(entry => entry.Key, entry => entry.Value);
            setloggingChannel.Add("loggingChannel", NpgsqlDbType.Hstore);

            Dictionary<string, NpgsqlDbType> addTask = userAndGuildId.ToDictionary(entry => entry.Key, entry => entry.Value);
            addTask.Add("taskType", NpgsqlDbType.Smallint);
            addTask.Add("channelID", NpgsqlDbType.Bigint);
            addTask.Add("setOff", NpgsqlDbType.Timestamp);
            addTask.Add("setAt", NpgsqlDbType.Timestamp);
            addTask.Add("content", NpgsqlDbType.Text);

            Dictionary<string, NpgsqlDbType> removeTask = userId.ToDictionary(entry => entry.Key, entry => entry.Value);
            removeTask.Add("taskType", NpgsqlDbType.Smallint);
            removeTask.Add("setAt", NpgsqlDbType.Timestamp);
            removeTask.Add("setOff", NpgsqlDbType.Timestamp);

            Dictionary<string, NpgsqlDbType> antiraid = guildId.ToDictionary(entry => entry.Key, entry => entry.Value);
            antiraid.Add("activated", NpgsqlDbType.Boolean);

            Dictionary<string, NpgsqlDbType> antiraidSetOff = guildId.ToDictionary(entry => entry.Key, entry => entry.Value);
            antiraid.Add("interval", NpgsqlDbType.Smallint);

            Dictionary<string, NpgsqlDbType> setUserRoles = userAndGuildId.ToDictionary(entry => entry.Key, entry => entry.Value);
            setUserRoles.Add("roles", (NpgsqlDbType.Array | NpgsqlDbType.Bigint));

            Dictionary<string, NpgsqlDbType> userRole = userAndGuildId.ToDictionary(entry => entry.Key, entry => entry.Value);
            userRole.Add("roleId", NpgsqlDbType.Bigint);

            Dictionary<string, NpgsqlDbType> setUserMute = userAndGuildId.ToDictionary(entry => entry.Key, entry => entry.Value);
            setUserRoles.Add("isMuted", NpgsqlDbType.Boolean);

            // Strike system
            createPreparedStatement("SELECT strikes FROM guild_cache WHERE user_id=@userId AND guild_id=@guildId", userAndGuildId, PostgresSQL._activeConnection, IndexedCommands.GetUserStrikes);
            createPreparedStatement("UPDATE guild_cache SET strikes = strikes + 1 WHERE user_id=@userId AND guild_id=@guildId", userAndGuildId, PostgresSQL._activeConnection, IndexedCommands.AddUserStrike);
            createPreparedStatement("UPDATE guild_cache SET strikes = strikes - 1 WHERE user_id=@userId AND guild_id=@guildId", userAndGuildId, PostgresSQL._activeConnection, IndexedCommands.RemoveUserStrike);
            createPreparedStatement("UPDATE guild_cache SET strikes = @strikeCount WHERE user_id=@userId AND guild_id=@guildId", setStrikeCount, PostgresSQL._activeConnection, IndexedCommands.SetUserStrikes);

            createPreparedStatement("INSERT INTO guild_config(guild_id) VALUES(@guildId)", guildId, PostgresSQL._activeConnection, IndexedCommands.InsertGuildId);

            // Static roles system
            createPreparedStatement("SELECT role_ids FROM guild_cache WHERE user_id=@userId AND guild_id=@guildId", userAndGuildId, PostgresSQL._activeConnection, IndexedCommands.GetUserRoles);
            createPreparedStatement("UPDATE guild_cache SET role_ids=@roles WHERE user_id=@userId AND guild_id=@guildId", setUserRoles, PostgresSQL._activeConnection, IndexedCommands.SetUserRoles);
            createPreparedStatement("UPDATE guild_cache SET role_ids=array_append(role_ids, @roleId) WHERE user_id=@userId AND guild_id=@guildId", userRole, PostgresSQL._activeConnection, IndexedCommands.AddUserRole);
            createPreparedStatement("UPDATE guild_cache SET role_ids=array_remove(role_ids, @roleId) WHERE user_id=@userId AND guild_id=@guildId", userRole, PostgresSQL._activeConnection, IndexedCommands.RemoveUserRole);

            // Mute system
            createPreparedStatement("SELECT mute_role FROM guild_config WHERE guild_id=@guildId", guildId, PostgresSQL._activeConnection, IndexedCommands.GetMuteRole);
            createPreparedStatement("UPDATE guild_config SET mute_role=@muteRole WHERE guild_id=@guildId", muteRole, PostgresSQL._activeConnection, IndexedCommands.SetMuteRole);

            // NoMeme system
            createPreparedStatement("SELECT logging_channels -> @guildEvent FROM guild_config WHERE guild_id=@guildId", getLoggingChannel, PostgresSQL._activeConnection, IndexedCommands.GetLoggingChannels);
            createPreparedStatement("UPDATE guild_config SET logging_channels = @loggingChannel WHERE guild_id=@guildId", setloggingChannel, PostgresSQL._activeConnection, IndexedCommands.SetLoggingChannel);
            createPreparedStatement("SELECT * FROM tasks WHERE user_id = @userId AND type = 0", userId, PostgresSQL._activeConnection, IndexedCommands.GetUserTasks);
            createPreparedStatement("SELECT * FROM tasks", null, PostgresSQL._repeatedConnection, IndexedCommands.GetAllTasks);
            createPreparedStatement("INSERT INTO tasks VALUES(@taskType, @guildId, @channelID, @userId, @setOff, @setAt, @content)", addTask, PostgresSQL._activeConnection, IndexedCommands.AddTask);
            createPreparedStatement("DELETE FROM tasks WHERE user_id=@userId AND set_off=@setOff AND set_at=@setAt AND type=@taskType", removeTask, PostgresSQL._repeatedConnection, IndexedCommands.RemoveTask);
            createPreparedStatement("SELECT antiraid FROM guild_config WHERE guild_id=@guildId", guildId, PostgresSQL._activeConnection, IndexedCommands.GetAntiRaidActive);
            createPreparedStatement("UPDATE guild_config SET antiraid=@activated WHERE guild_id=@guildId", antiraid, PostgresSQL._activeConnection, IndexedCommands.SetAntiRaidActive);
            createPreparedStatement("SELECT antiraid_setoff FROM guild_config WHERE guild_id=@guildId", guildId, PostgresSQL._activeConnection, IndexedCommands.GetAntiRaidSetOff);
            createPreparedStatement("UPDATE guild_config SET antiraid_setoff=@interval WHERE guild_id=@guildId", antiraidSetOff, PostgresSQL._activeConnection, IndexedCommands.SetAntiRaidSetOff);
            createPreparedStatement("UPDATE guild_cache SET muted=@isMuted WHERE user_id=@userId AND guild_id=@guildId", setUserMute, PostgresSQL._activeConnection, IndexedCommands.SetUserMuted);
            createPreparedStatement("SELECT muted FROM guild_cache WHERE user_id=@userId AND guild_id=@guildId", userAndGuildId, PostgresSQL._activeConnection, IndexedCommands.GetUserMuted);
        }
    }
}