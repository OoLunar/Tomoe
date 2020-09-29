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
            PreparedStatements.Query setMuteRole = Program.PreparedStatements.Statements[PreparedStatements.IndexedCommands.SetMuteRole];
            setMuteRole.Parameters["muteRole"].Value = new System.Collections.Generic.Dictionary<string, string>() { { roleID.ToString(), userID.ToString() } };
            setMuteRole.Parameters["guildID"].Value = long.Parse(guildID.ToString());
            setMuteRole.Command.ExecuteNonQuery();
        }

        public static MutedRole? Get(ulong guildID) {
            PreparedStatements.Query getMuteRole = Program.PreparedStatements.Statements[PreparedStatements.IndexedCommands.GetMuteRole];
            getMuteRole.Parameters["guildID"].Value = long.Parse(guildID.ToString());
            NpgsqlDataReader dataReader = getMuteRole.Command.ExecuteReader();
            if (!dataReader.Read()) return null;
            Dictionary<string, string> queryResult = (Dictionary<string, string>) dataReader[0];
            dataReader.Close();
            ulong roleID = ulong.Parse(queryResult.First().Key);
            ulong userID = ulong.Parse(queryResult.First().Value);
            return new MutedRole(guildID, roleID, userID);
        }

        public static void SetMute(ulong userID, ulong guildID, bool isMuted) {
            PreparedStatements.Query toggleUserMute = Program.PreparedStatements.Statements[PreparedStatements.IndexedCommands.SetUserMute];
            toggleUserMute.Parameters["userID"].Value = (long) userID;
            toggleUserMute.Parameters["guildID"].Value = (long) guildID;
            toggleUserMute.Parameters["isMuted"].Value = isMuted;
            toggleUserMute.Command.ExecuteNonQuery();
        }

        public static bool? GetMute(ulong userID, ulong guildID) {
            PreparedStatements.Query getUserMute = Program.PreparedStatements.Statements[PreparedStatements.IndexedCommands.GetUserMute];
            getUserMute.Parameters["guildID"].Value = (long) guildID;
            getUserMute.Parameters["userID"].Value = (long) userID;
            NpgsqlDataReader dataReader = getUserMute.Command.ExecuteReader();
            if (!dataReader.Read()) return null;
            bool queryResult = dataReader.GetBoolean(0);
            dataReader.Close();
            return queryResult;
        }
    }
}