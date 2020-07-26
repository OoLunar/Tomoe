using System.Xml;
using Npgsql;

namespace Tomoe.Utils.Cache {
    public class MutedRole {
        public ulong GuildID;
        public ulong RoleID;
        public ulong SetByUserID;

        private static XmlNode postgresSettings = Program.Tokens.DocumentElement.SelectSingleNode("postgres");
        private static NpgsqlConnection connection = new NpgsqlConnection($"Host={postgresSettings.Attributes["host"].Value};Port={postgresSettings.Attributes["port"].Value};Username={postgresSettings.Attributes["username"].Value};Password={postgresSettings.Attributes["password"].Value};Database={postgresSettings.Attributes["database"].Value};SSL Mode={postgresSettings.Attributes["ssl_mode"].Value}");

        public MutedRole(ulong roleID, ulong guildID, ulong setByUserID) {
            RoleID = roleID;
            GuildID = guildID;
            SetByUserID = setByUserID;
        }

        public static void Store(ulong guildID, ulong roleID, ulong userID) {
            //TODO: Prepared statements. Learn them. Even if it kills you.
            connection.Open();
            NpgsqlCommand updateQuery = new NpgsqlCommand($"UPDATE guild_configs SET mute_role='{roleID},{userID}' WHERE guild_id='{guildID}';", connection);
            updateQuery.ExecuteNonQuery();
            updateQuery.Dispose();
            connection.Close();
        }

        public static MutedRole Get(ulong guildID) {
            connection.Open();
            NpgsqlDataReader isMutedRolePresent = new NpgsqlCommand($"SELECT mute_role FROM guild_configs WHERE guild_id='{guildID}';", connection).ExecuteReader();
            isMutedRolePresent.Read();
            System.Console.WriteLine("Here 1");
            string queryResult = " ";
            if (isMutedRolePresent.HasRows) queryResult = isMutedRolePresent[0].ToString().Trim();
            System.Console.WriteLine("Here 2");
            isMutedRolePresent.Close();
            connection.Close();
            if (!string.IsNullOrWhiteSpace(queryResult)) {
                string[] mutedRoleInfo = queryResult.ToString().Split(',');
                ulong roleID = ulong.Parse(mutedRoleInfo[0]);
                ulong userID = ulong.Parse(mutedRoleInfo[1]);
                return new MutedRole(roleID, userID, guildID);
            } else return null;
        }
    }
}