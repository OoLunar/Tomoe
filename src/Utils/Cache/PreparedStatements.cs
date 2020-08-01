using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml;
using Npgsql;

namespace Tomoe.Utils.Cache {
    public class PreparedStatements {

        public enum IndexedCommands {
            GetMuteRole,
            SetupMuteRole,
            GetGuild,
            SetupGuild
        }

        public struct Query {
            public NpgsqlCommand Command;
            public Dictionary<string, NpgsqlParameter> Parameters;
        }

        private static XmlNode postgresSettings = Program.Tokens.DocumentElement.SelectSingleNode("postgres");
        public static NpgsqlConnection connection = new NpgsqlConnection($"Host={postgresSettings.Attributes["host"].Value};Port={postgresSettings.Attributes["port"].Value};Username={postgresSettings.Attributes["username"].Value};Password={postgresSettings.Attributes["password"].Value};Database={postgresSettings.Attributes["database"].Value};SSL Mode={postgresSettings.Attributes["ssl_mode"].Value}");
        public Dictionary<IndexedCommands, Query> Statements = new Dictionary<IndexedCommands, Query>();

        public PreparedStatements() {
            connection.Open();

            Query getGuild = new Query();
            getGuild.Command = new NpgsqlCommand("SELECT guild_id FROM guild_configs WHERE guild_id=@guildID;", connection);
            getGuild.Parameters = new Dictionary<string, NpgsqlParameter>();
            getGuild.Parameters.Add("guildID", getGuild.Command.Parameters.Add("guildID", NpgsqlTypes.NpgsqlDbType.Text));
            getGuild.Command.Prepare();
            Statements.Add(IndexedCommands.GetGuild, getGuild);

            Query setupGuild = new Query();
            setupGuild.Command = new NpgsqlCommand("INSERT INTO guild_configs(guild_id) VALUES(@guildID);", connection);
            setupGuild.Parameters = new Dictionary<string, NpgsqlParameter>();
            setupGuild.Parameters.Add("guildID", setupGuild.Command.Parameters.Add("guildID", NpgsqlTypes.NpgsqlDbType.Text));
            setupGuild.Command.Prepare();
            Statements.Add(IndexedCommands.SetupGuild, setupGuild);

            Query getMuteRole = new Query();
            getMuteRole.Command = new NpgsqlCommand("SELECT mute_role FROM guild_configs WHERE guild_id=@guildID;", connection);
            getMuteRole.Parameters = new Dictionary<string, NpgsqlParameter>();
            getMuteRole.Parameters.Add("guildID", getMuteRole.Command.Parameters.Add("guildID", NpgsqlTypes.NpgsqlDbType.Text));
            getMuteRole.Command.Prepare();
            Statements.Add(IndexedCommands.GetMuteRole, getMuteRole);

            Query setupMuteRole = new Query();
            setupMuteRole.Command = new NpgsqlCommand("UPDATE guild_configs SET mute_role=@muteInfo WHERE guild_id=@guildID;", connection);
            setupMuteRole.Parameters = new Dictionary<string, NpgsqlParameter>();
            setupMuteRole.Parameters.Add("muteInfo", setupMuteRole.Command.Parameters.Add("muteInfo", NpgsqlTypes.NpgsqlDbType.Text));
            setupMuteRole.Parameters.Add("guildID", setupMuteRole.Command.Parameters.Add("guildID", NpgsqlTypes.NpgsqlDbType.Text));
            setupMuteRole.Command.Prepare();
            Statements.Add(IndexedCommands.SetupMuteRole, setupMuteRole);
        }

        public static void TestConnection() {
            connection.Open();
            NpgsqlCommand testConn = new NpgsqlCommand("SELECT guild_id FROM guild_configs", connection);
            var task = Task.Run(() => testConn.ExecuteNonQuery());
            if (task.Wait(TimeSpan.FromSeconds(10))) {
                testConn.Dispose();
                connection.Close();
                Console.WriteLine("[Database] Connected!");
            } else {
                testConn.Dispose();
                connection.Close();
                Console.WriteLine("[Database] Can't connect. Exiting...");
                Environment.Exit(1);
            }
        }
    }
}