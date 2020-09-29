using Npgsql;

namespace Tomoe.Utils.Cache {
    public class UserRoles {
        public static void Store(ulong guildID, ulong userID, long[] roles) {
            PreparedStatements.Query storeRoles = Program.PreparedStatements.Statements[PreparedStatements.IndexedCommands.SetUserRoles];
            storeRoles.Parameters["guildID"].Value = (long) guildID;
            storeRoles.Parameters["userID"].Value = (long) userID;
            storeRoles.Parameters["roles"].Value = roles;
            storeRoles.Command.ExecuteNonQuery();
        }

        public static ulong[] ? Get(ulong guildID, ulong userID) {
            PreparedStatements.Query getRoles = Program.PreparedStatements.Statements[PreparedStatements.IndexedCommands.GetUserRoles];
            getRoles.Parameters["guildID"].Value = (long) guildID;
            getRoles.Parameters["userID"].Value = (long) userID;
            NpgsqlDataReader dataReader = getRoles.Command.ExecuteReader();
            if (!dataReader.Read()) return null;
            foreach (ulong[] channelID in dataReader) {
                dataReader.Close();
                return channelID;
            }
            return null;
        }
    }
}