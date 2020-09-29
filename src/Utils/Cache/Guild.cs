using System.Collections.Generic;
using Npgsql;

namespace Tomoe.Utils.Cache {
    public class Guild {
        public ulong GuildID;
        public ulong RoleID;
        public ulong SetByUserID;

        public static void Store(ulong guildID) {
            PreparedStatements.Query muteRole = Program.PreparedStatements.Statements[PreparedStatements.IndexedCommands.SetGuild];
            muteRole.Parameters["guildID"].Value = (long) guildID;
            muteRole.Command.ExecuteNonQuery();
        }

        public static ulong? Get(ulong guildID) {
            PreparedStatements.Query getGuildID = Program.PreparedStatements.Statements[PreparedStatements.IndexedCommands.GetGuild];
            getGuildID.Parameters["guildID"].Value = (long) guildID;
            NpgsqlDataReader dataReader = getGuildID.Command.ExecuteReader();
            if (!dataReader.Read()) return null;
            long queryResult = dataReader.GetInt64(0);
            dataReader.Close();
            return ulong.Parse(queryResult.ToString());
        }

        public static ulong? GetLoggingChannel(ulong guildID, Event eventUpdated) {
            PreparedStatements.Query getLoggingChannel = Program.PreparedStatements.Statements[PreparedStatements.IndexedCommands.GetLoggingChannel];
            getLoggingChannel.Parameters["guildID"].Value = (long) guildID;
            getLoggingChannel.Parameters["guildEvent"].Value = eventUpdated.ToString();
            NpgsqlDataReader dataReader = getLoggingChannel.Command.ExecuteReaderAsync().GetAwaiter().GetResult();
            if (!dataReader.Read()) return null;
            long queryResult = dataReader.GetInt64(0);
            dataReader.Close();
            return ulong.Parse(queryResult.ToString());
        }

        public static void AddLoggingChannel(ulong guildID, Event eventUpdated, ulong channel) {
            PreparedStatements.Query addLoggingChannel = Program.PreparedStatements.Statements[PreparedStatements.IndexedCommands.SetLoggingChannel];
            addLoggingChannel.Parameters["guildID"].Value = (long) guildID;
            addLoggingChannel.Parameters["loggingChannel"].Value = new Dictionary<string, string>() { { eventUpdated.ToString(), channel.ToString() } };
            addLoggingChannel.Command.ExecuteNonQuery();
        }
    }
}