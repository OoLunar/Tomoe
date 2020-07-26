using System.Xml;
using Npgsql;

namespace Tomoe.Utils.Cache {
    public class SetupGuildCache {
        public ulong GuildID;
        public ulong RoleID;
        public ulong SetByUserID;

        private static XmlNode postgresSettings = Program.Tokens.DocumentElement.SelectSingleNode("postgres");
        private static NpgsqlConnection connection = new NpgsqlConnection($"Host={postgresSettings.Attributes["host"].Value};Port={postgresSettings.Attributes["port"].Value};Username={postgresSettings.Attributes["username"].Value};Password={postgresSettings.Attributes["password"].Value};Database={postgresSettings.Attributes["database"].Value};SSL Mode={postgresSettings.Attributes["ssl_mode"].Value}");

        public static void Store(ulong guildID) {
            //TODO: Prepared statements. Learn them. Even if it kills you.
            connection.Open();
            NpgsqlCommand updateQuery = new NpgsqlCommand($"INSERT INTO guild_configs(guild_id) VALUES('{guildID}');", connection);
            updateQuery.ExecuteNonQuery();
            updateQuery.Dispose();
            connection.Close();
        }

        public static ulong? Get(ulong guildID) {
            connection.Open();
            NpgsqlDataReader isMutedRolePresent = new NpgsqlCommand($"SELECT guild_id FROM guild_configs WHERE guild_id='{guildID}';", connection).ExecuteReader();
            isMutedRolePresent.Read();
            System.Console.WriteLine("Here 1");
            string queryResult = " ";
            if (isMutedRolePresent.HasRows) queryResult = isMutedRolePresent[0].ToString().Trim();
            System.Console.WriteLine("Here 2");
            isMutedRolePresent.Close();
            connection.Close();
            if (!string.IsNullOrWhiteSpace(queryResult)) {
                return ulong.Parse(queryResult);
            } else return null;
        }
    }
}