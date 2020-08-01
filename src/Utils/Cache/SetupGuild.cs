using Npgsql;

namespace Tomoe.Utils.Cache {
    public class SetupGuild {
        public ulong GuildID;
        public ulong RoleID;
        public ulong SetByUserID;

        public static void Store(ulong guildID) {
            PreparedStatements.Query muteRole = Program.PreparedStatements.Statements[PreparedStatements.IndexedCommands.SetupGuild];
            muteRole.Parameters["guildID"].Value = guildID.ToString();
            muteRole.Command.ExecuteNonQuery();
        }

        public static ulong? Get(ulong guildID) {
            PreparedStatements.Query getGuildID = Program.PreparedStatements.Statements[PreparedStatements.IndexedCommands.GetGuild];
            getGuildID.Parameters["guildID"].Value = guildID.ToString();
            NpgsqlDataReader isGuildIDPresent = getGuildID.Command.ExecuteReader();
            isGuildIDPresent.Read();
            string queryResult = null;
            if (isGuildIDPresent.HasRows) queryResult = isGuildIDPresent[0].ToString().Trim();
            isGuildIDPresent.Close();
            if (!string.IsNullOrWhiteSpace(queryResult)) {
                return ulong.Parse(queryResult);
            } else return null;
        }
    }
}