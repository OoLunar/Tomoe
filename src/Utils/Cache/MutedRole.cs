using Npgsql;

namespace Tomoe.Utils.Cache {
    public class MutedRole {
        public ulong GuildID;
        public ulong RoleID;
        public ulong SetByUserID;

        public MutedRole(ulong roleID, ulong guildID, ulong setByUserID) {
            RoleID = roleID;
            GuildID = guildID;
            SetByUserID = setByUserID;
        }

        public static void Store(ulong guildID, ulong roleID, ulong userID) {
            PreparedStatements.Query muteRole = Program.PreparedStatements.Statements[PreparedStatements.IndexedCommands.SetupMuteRole];
            muteRole.Parameters["guildID"].Value = guildID.ToString();
            muteRole.Parameters["roleID"].Value = roleID.ToString();
            muteRole.Parameters["userID"].Value = userID.ToString();
            muteRole.Command.ExecuteNonQuery();
        }

        public static MutedRole Get(ulong guildID) {
            PreparedStatements.Query muteRole = Program.PreparedStatements.Statements[PreparedStatements.IndexedCommands.GetMuteRole];
            muteRole.Parameters["guildID"].Value = guildID;
            foreach (NpgsqlStatement stmt in muteRole.Command.Statements) System.Console.WriteLine(stmt.InputParameters[0].Value);
            NpgsqlDataReader isMutedRolePresent = muteRole.Command.ExecuteReader();
            isMutedRolePresent.Read();
            string queryResult = null;
            if (isMutedRolePresent.HasRows) queryResult = isMutedRolePresent[0].ToString().Trim();
            if (!string.IsNullOrWhiteSpace(queryResult)) {
                string[] mutedRoleInfo = queryResult.ToString().Split(',');
                ulong roleID = ulong.Parse(mutedRoleInfo[0]);
                ulong userID = ulong.Parse(mutedRoleInfo[1]);
                return new MutedRole(roleID, userID, guildID);
            } else return null;
        }
    }
}