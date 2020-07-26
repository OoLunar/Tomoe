using System.Xml;
using Npgsql;

namespace Tomoe.Utils.Cache {
    public class MutedRole {
        private static XmlNode PostgresSettings = Program.Tokens.DocumentElement.SelectSingleNode("Postgres");
        private static NpgsqlConnection Connection = new NpgsqlConnection($"Host={PostgresSettings.Attributes["host"].Value};Port={PostgresSettings.Attributes["port"].Value};Username={PostgresSettings.Attributes["username"].Value};Password={PostgresSettings.Attributes["password"].Value};Database={PostgresSettings.Attributes["database"].Value};SSL Mode={PostgresSettings.Attributes["ssl_mode"].Value}");
        public ulong GuildID;
        public ulong RoleID;
        public ulong SetByUserID;

        public MutedRole(ulong roleID, ulong guildID, ulong setByUserID) {
            RoleID = roleID;
            GuildID = guildID;
            SetByUserID = setByUserID;
        }

        public static void Store(ulong guildID, ulong roleID, ulong userID) {
            Connection.Open();
            //TODO: Make single INSERT or UPDATE query. This is inefficient.
            //TODO: Learn if this is really the correct way to use prepared statements.
            NpgsqlCommand updateQuery = new NpgsqlCommand($"UPDATE guild_configs SET mute_role='{roleID},{userID}' WHERE guild_id='{guildID}';", Connection);
            updateQuery.ExecuteNonQuery();
            updateQuery.Dispose();
            Connection.Close();
        }

        public static MutedRole Get(ulong guildID) {
            Connection.Open();
            NpgsqlDataReader isMutedRolePresent = new NpgsqlCommand($"SELECT mute_role FROM guild_configs WHERE guild_id='{guildID}';", Connection).ExecuteReader();
            isMutedRolePresent.Read();
            System.Console.WriteLine("Here 1");
            string queryResult = " ";
            if (isMutedRolePresent.HasRows) queryResult = isMutedRolePresent[0].ToString().Trim();
            System.Console.WriteLine("Here 2");
            isMutedRolePresent.Close();
            Connection.Close();
            if (!string.IsNullOrWhiteSpace(queryResult)) {
                string[] mutedRoleInfo = queryResult.ToString().Split(',');
                ulong roleID = ulong.Parse(mutedRoleInfo[0]);
                ulong userID = ulong.Parse(mutedRoleInfo[1]);
                return new MutedRole(roleID, userID, guildID);
            } else return null;
        }
    }
}