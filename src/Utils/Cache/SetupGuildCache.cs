using System.Xml;
using Npgsql;

namespace Tomoe.Utils.Cache {
    public class SetupGuildCache {
        private static XmlNode PostgresSettings = Program.Tokens.DocumentElement.SelectSingleNode("Postgres");
        //Convert to SSL mode.
        private static NpgsqlConnection Connection = new NpgsqlConnection($"Host={PostgresSettings.Attributes["host"].Value};Port={PostgresSettings.Attributes["port"].Value};Username={PostgresSettings.Attributes["username"].Value};Password={PostgresSettings.Attributes["password"].Value};Database={PostgresSettings.Attributes["database"].Value}");
        public ulong GuildID;
        public ulong RoleID;
        public ulong SetByUserID;

        public static void Store(ulong guildID) {
            Connection.Open();
            //TODO: Make single INSERT or UPDATE query. This is inefficient.
            //TODO: Learn if this is really the correct way to use prepared statements.
            NpgsqlCommand updateQuery = new NpgsqlCommand($"INSERT INTO guild_configs(guild_id) VALUES('{guildID}');", Connection);
            updateQuery.ExecuteNonQuery();
            updateQuery.Dispose();
            Connection.Close();
        }

        public static ulong? Get(ulong guildID) {
            Connection.Open();
            NpgsqlDataReader isMutedRolePresent = new NpgsqlCommand($"SELECT guild_id FROM guild_configs WHERE guild_id='{guildID}';", Connection).ExecuteReader();
            isMutedRolePresent.Read();
            System.Console.WriteLine("Here 1");
            string queryResult = " ";
            if (isMutedRolePresent.HasRows) queryResult = isMutedRolePresent[0].ToString().Trim();
            System.Console.WriteLine("Here 2");
            isMutedRolePresent.Close();
            Connection.Close();
            if (!string.IsNullOrWhiteSpace(queryResult)) {
                return ulong.Parse(queryResult);
            } else return null;
        }
    }
}