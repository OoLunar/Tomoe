using System;
using Microsoft.Data.Sqlite;
using Tomoe.Database.Classes;

namespace Tomoe.Database {
    public class SQLite : IDatabase {
        private SqliteConnection GuildCacheConnection;
        private SqliteConnection GuildConfigConnection;
        private Logger Logger = new Logger("Database/SQLite");

        public SQLite() {
            if (Tomoe.Utils.FileSystem.CreateDirectory(Program.Config.Database.Sqlite.DatabasePath) == false) {
                Logger.Error("Database directory is needed for creating SQLite database files. Exiting...");
                Environment.Exit(1);
            }

            GuildCacheConnection = new SqliteConnection($"Data Source={Program.Config.Database.Sqlite.DatabasePath}/guild_cache.db");
            GuildConfigConnection = new SqliteConnection($"Data Source={Program.Config.Database.Sqlite.DatabasePath}/guild_config.db");
        }
    }
}