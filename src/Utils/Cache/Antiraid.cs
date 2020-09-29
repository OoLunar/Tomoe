using System.Collections.Generic;
using Npgsql;

namespace Tomoe.Utils.Cache {
    public class Antiraid {
        public bool Activated;
        public int Interval;

        public static bool? IsActivated(ulong guildID) {
            PreparedStatements.Query isAntiraidActivated = Program.PreparedStatements.Statements[PreparedStatements.IndexedCommands.GetAntiraidActivated];
            isAntiraidActivated.Parameters["guildID"].Value = (long) guildID;
            NpgsqlDataReader dataReader = isAntiraidActivated.Command.ExecuteReader();
            if (!dataReader.Read()) return null;
            bool queryResult = dataReader.GetBoolean(0);
            dataReader.Close();
            return queryResult;
        }

        public static int? GetInterval(ulong guildID) {
            PreparedStatements.Query getAntiraidInterval = Program.PreparedStatements.Statements[PreparedStatements.IndexedCommands.GetAntiraidInterval];
            getAntiraidInterval.Parameters["guildID"].Value = (long) guildID;
            NpgsqlDataReader dataReader = getAntiraidInterval.Command.ExecuteReader();
            if (!dataReader.Read()) return null;
            int queryResult = dataReader.GetInt32(0);
            dataReader.Close();
            return queryResult;
        }

        public static void SetActivated(ulong guildID, bool isActivated) {
            PreparedStatements.Query setActivated = Program.PreparedStatements.Statements[PreparedStatements.IndexedCommands.SetAntiraidActivated];
            setActivated.Parameters["guildID"].Value = (long) guildID;
            setActivated.Parameters["activated"].Value = isActivated;
            setActivated.Command.ExecuteNonQuery();
        }

        public static void SetInterval(ulong guildID, int interval) {
            PreparedStatements.Query setInterval = Program.PreparedStatements.Statements[PreparedStatements.IndexedCommands.SetAntiraidInterval];
            setInterval.Parameters["guildID"].Value = (long) guildID;
            setInterval.Parameters["interval"].Value = interval;
            setInterval.Command.ExecuteNonQuery();
        }
    }
}