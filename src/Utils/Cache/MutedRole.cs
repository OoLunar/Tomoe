using System.Collections.Generic;
using System.Linq;
using Npgsql;

namespace Tomoe.Utils.Cache {
    public static class MutedRoleExtension {
        public static bool HasValue(this MutedRole muteRole) => muteRole != null;
    }

    public class MutedRole {
        public ulong GuildID;
        public ulong RoleID;
        public ulong UserID;

        public MutedRole(ulong guildID, ulong roleID, ulong setByUserID) {
            GuildID = guildID;
            RoleID = roleID;
            UserID = setByUserID;
        }

        public static void Set(ulong guildID, ulong roleID, ulong userID) {
            PreparedStatements.Query muteRole = Program.PreparedStatements.Statements[PreparedStatements.IndexedCommands.SetMuteRole];
            muteRole.Parameters["muteRole"].Value = new System.Collections.Generic.Dictionary<string, string>() { { roleID.ToString(), userID.ToString() } };
            muteRole.Parameters["guildID"].Value = long.Parse(guildID.ToString());
            muteRole.Command.ExecuteNonQuery();
        }

        public static MutedRole? Get(ulong guildID) {
            PreparedStatements.Query muteRole = Program.PreparedStatements.Statements[PreparedStatements.IndexedCommands.GetMuteRole];
            muteRole.Parameters["guildID"].Value = long.Parse(guildID.ToString());
            NpgsqlDataReader isMutedRolePresent = muteRole.Command.ExecuteReader();
            isMutedRolePresent.Read();
            Dictionary<string, string> queryResult = null;
            if (isMutedRolePresent.HasRows && isMutedRolePresent[0].GetType() != typeof(System.DBNull)) queryResult = (Dictionary<string, string>) isMutedRolePresent[0];
            isMutedRolePresent.Close();
            if (queryResult != null) {
                ulong roleID = ulong.Parse(queryResult.First().Key);
                ulong userID = ulong.Parse(queryResult.First().Value);
                return new MutedRole(guildID, roleID, userID);
            } else return null;
        }

        public static void SetMute(ulong userID, ulong guildID, bool isMuted) {
            PreparedStatements.Query toggleMute = Program.PreparedStatements.Statements[PreparedStatements.IndexedCommands.SetUserMute];
            toggleMute.Parameters["userID"].Value = (long) userID;
            toggleMute.Parameters["guildID"].Value = (long) guildID;
            toggleMute.Parameters["isMuted"].Value = isMuted;
            toggleMute.Command.ExecuteNonQuery();
        }

        public static bool GetMute(ulong userID, ulong guildID) {
            PreparedStatements.Query getMute = Program.PreparedStatements.Statements[PreparedStatements.IndexedCommands.GetMuteRole];
            getMute.Parameters["guildID"].Value = (long) guildID;
            getMute.Parameters["userID"].Value = (long) userID;
            NpgsqlDataReader databaseReader = getMute.Command.ExecuteReader();
            databaseReader.Read();
            bool queryResult = bool.Parse(databaseReader[0].ToString());
            databaseReader.Close();
            return queryResult;
        }
    }
}