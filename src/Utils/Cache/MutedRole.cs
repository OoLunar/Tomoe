using Npgsql;

namespace Tomoe.Utils.Cache {
    public class MutedRole {
        public ulong GuildID;
        public ulong RoleID;
        public ulong UserID;

        public MutedRole(ulong guildID, ulong roleID, ulong setByUserID) {
            GuildID = guildID;
            RoleID = roleID;
            UserID = setByUserID;
        }

        public static void Store(ulong guildID, ulong roleID, ulong userID) {
            PreparedStatements.Query muteRole = Program.PreparedStatements.Statements[PreparedStatements.IndexedCommands.SetupMuteRole];
            muteRole.Parameters["muteInfo"].Value = $"{roleID.ToString()},{userID.ToString()}";
            muteRole.Parameters["guildID"].Value = guildID.ToString();
            muteRole.Command.ExecuteNonQuery();
        }

        public static MutedRole Get(ulong guildID) {
            PreparedStatements.Query muteRole = Program.PreparedStatements.Statements[PreparedStatements.IndexedCommands.GetMuteRole];
            muteRole.Parameters["guildID"].Value = guildID.ToString();
            NpgsqlDataReader isMutedRolePresent = muteRole.Command.ExecuteReader();
            isMutedRolePresent.Read();
            string queryResult = null;
            if (isMutedRolePresent.HasRows) queryResult = isMutedRolePresent[0].ToString().Trim();
            isMutedRolePresent.Close();
            if (!string.IsNullOrWhiteSpace(queryResult)) {
                string[] mutedRoleInfo = queryResult.ToString().Split(',');
                ulong roleID = ulong.Parse(mutedRoleInfo[0]);
                ulong userID = ulong.Parse(mutedRoleInfo[1]);
                return new MutedRole(guildID, roleID, userID);
            } else return null;
        }
    }
}