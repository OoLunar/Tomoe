using System.Collections.Generic;
using Npgsql;

namespace Tomoe.Utils.Cache {
    public class Antiraid {
        public bool Activated;
        public int Interval;

        public static bool? IsActivated(ulong guildID) {
            PreparedStatements.Query getLoggingChannel = Program.PreparedStatements.Statements[PreparedStatements.IndexedCommands.GetAntiraidActivated];
            getLoggingChannel.Parameters["guildID"].Value = (long) guildID;
            NpgsqlDataReader dataReader = getLoggingChannel.Command.ExecuteReader();
            dataReader.Read();
            string queryResult = null;
            if (dataReader.HasRows) queryResult = dataReader[0].ToString().Trim();
            dataReader.Close();
            if (!string.IsNullOrWhiteSpace(queryResult)) {
                return bool.Parse(queryResult);
            } else return null;
        }

        public static int? GetInterval(ulong guildID) {
            PreparedStatements.Query getLoggingChannel = Program.PreparedStatements.Statements[PreparedStatements.IndexedCommands.GetAntiraidInterval];
            getLoggingChannel.Parameters["guildID"].Value = (long) guildID;
            NpgsqlDataReader dataReader = getLoggingChannel.Command.ExecuteReader();
            dataReader.Read();
            string queryResult = null;
            if (dataReader.HasRows) queryResult = dataReader[0].ToString().Trim();
            dataReader.Close();
            if (!string.IsNullOrWhiteSpace(queryResult)) {
                return int.Parse(queryResult);
            } else return null;
        }

        public static void SetActivated(ulong guildID, bool isActivated) {
            PreparedStatements.Query addLoggingChannel = Program.PreparedStatements.Statements[PreparedStatements.IndexedCommands.SetAntiraidActivated];
            addLoggingChannel.Parameters["guildID"].Value = (long) guildID;
            addLoggingChannel.Parameters["activated"].Value = isActivated;
            addLoggingChannel.Command.ExecuteNonQuery();
        }
    }
}