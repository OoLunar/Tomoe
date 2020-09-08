using System.Collections.Generic;
using Npgsql;

namespace Tomoe.Utils.Cache {
    public class Guild {
        public ulong GuildID;
        public ulong RoleID;
        public ulong SetByUserID;

        public static void Store(ulong guildID) {
            PreparedStatements.Query muteRole = Program.PreparedStatements.Statements[PreparedStatements.IndexedCommands.SetGuild];
            muteRole.Parameters["guildID"].Value = guildID.ToString();
            muteRole.Command.ExecuteNonQuery();
        }

        public static ulong? Get(ulong guildID) {
            PreparedStatements.Query getGuildID = Program.PreparedStatements.Statements[PreparedStatements.IndexedCommands.GetGuild];
            getGuildID.Parameters["guildID"].Value = guildID.ToString();
            NpgsqlDataReader dataReader = getGuildID.Command.ExecuteReader();
            dataReader.Read();
            string queryResult = null;
            if (dataReader.HasRows) queryResult = dataReader[0].ToString().Trim();
            dataReader.Close();
            if (!string.IsNullOrWhiteSpace(queryResult)) {
                return ulong.Parse(queryResult);
            } else return null;
        }

        public static ulong? GetLoggingChannel(ulong guildID, Event eventUpdated) {
            PreparedStatements.Query getLoggingChannel = Program.PreparedStatements.Statements[PreparedStatements.IndexedCommands.GetLoggingChannel];
            getLoggingChannel.Parameters["guildID"].Value = guildID.ToString();
            getLoggingChannel.Parameters["guildEvent"].Value = eventUpdated.ToString();
            NpgsqlDataReader dataReader = getLoggingChannel.Command.ExecuteReader();
            dataReader.Read();
            string queryResult = null;
            if (dataReader.HasRows) queryResult = dataReader[0].ToString().Trim();
            dataReader.Close();
            if (!string.IsNullOrWhiteSpace(queryResult)) {
                return ulong.Parse(queryResult);
            } else return null;
        }

        public static void AddLoggingChannel(ulong guildID, Event eventUpdated, ulong channel) {
            PreparedStatements.Query addLoggingChannel = Program.PreparedStatements.Statements[PreparedStatements.IndexedCommands.SetLoggingChannel];
            addLoggingChannel.Parameters["guildID"].Value = guildID.ToString();
            addLoggingChannel.Parameters["loggingChannel"].Value = new Dictionary<string, string>() { { eventUpdated.ToString(), channel.ToString() } };
            addLoggingChannel.Command.ExecuteNonQuery();
        }
    }
}