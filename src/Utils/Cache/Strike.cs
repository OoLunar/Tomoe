using Npgsql;

namespace Tomoe.Utils.Cache {
    public class Strikes {
        public static void Add(ulong guildID, ulong userID) {
            PreparedStatements.Query addStrike = Program.PreparedStatements.Statements[PreparedStatements.IndexedCommands.AddStrike];
            addStrike.Parameters["guildID"].Value = (long) guildID;
            addStrike.Parameters["userID"].Value = (long) userID;
            addStrike.Command.ExecuteNonQuery();
        }

        public static void Remove(ulong guildID, ulong userID) {
            PreparedStatements.Query removeStrike = Program.PreparedStatements.Statements[PreparedStatements.IndexedCommands.RemoveStrike];
            removeStrike.Parameters["guildID"].Value = (long) guildID;
            removeStrike.Parameters["userID"].Value = (long) userID;
            removeStrike.Command.ExecuteNonQuery();
        }

        public static int? Get(ulong guildID, ulong userID) {
            PreparedStatements.Query getGuildID = Program.PreparedStatements.Statements[PreparedStatements.IndexedCommands.GetGuild];
            getGuildID.Parameters["guildID"].Value = (long) guildID;
            NpgsqlDataReader dataReader = getGuildID.Command.ExecuteReader();
            if (!dataReader.Read()) return null;
            int queryResult = dataReader.GetInt32(0);
            dataReader.Close();
            return queryResult;
        }
    }
}