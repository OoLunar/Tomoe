using System;
using System.IO;
using System.Text.Json;
using Newtonsoft.Json;

namespace Tomoe {
    public class Config {
        [JsonProperty("database")]
        public Database.Database Database;

        [JsonProperty("discord_api_token")]
        public string DiscordApiToken;

        [JsonProperty("log_to_file")]
        public bool LogToFile;

        public static string ProjectRoot = Path.GetFullPath("../../../../", System.AppDomain.CurrentDomain.BaseDirectory).Replace('\\', '/');
        public string LogPath;
        private static string tokenFile = Path.Combine(ProjectRoot, "res/config.jsonc");
        private static string dialogueFile = Path.Combine(ProjectRoot, "res/dialog.jsonc");

        public static Config Init() {
            if (!File.Exists(Path.Join(ProjectRoot, "res/config.jsonc"))) {
                Utils.FileSystem.CreateFile(tokenFile);
                File.WriteAllText(tokenFile, "{\n\t/*Token to have your bot login to Discord.*/\n\t\"discord_api_token\": \"\",\n\t/*Log to file instead of console.*/\n\t\"log_to_file\": false,\n\t/*PostgresSQL database. More support will be added in the future.*/\n\t\"database\": {\n\t\t\"type\": \"sqlite\",\n\t\t\"sqlite\": {\n\t\t\t/*Relative to root directory.*/\n\t\t\t\"database_path\": \"db/\"\n\t\t},\n\t\t\"postgresql\": {\n\t\t\t/*The IP or hostname that the database is listening on.*/\n\t\t\t\"host\": \"localhost\",\n\t\t\t/*The port that the database is listening on.*/\n\t\t\t\"port\": 5432,\n\t\t\t/*The username used to login to the database.*/\n\t\t\t\"username\": \"\",\n\t\t\t/*An Postgres compliant password used to login. Must be Postgres compliant.*/\n\t\t\t\"password\": \"\",\n\t\t\t/*Database name.*/\n\t\t\t\"database_name\": \"\",\n\t\t\t/* Choose security levels. Options are:\n\t\t\t * - Require\n\t\t\t * - Prefer\n\t\t\t * - Disable\n\t\t\t * Defaults to Require.\n\t\t\t */\n\t\t\t\"ssl_mode\": \"Require\"\n\t\t}\n\t}\n}");
                System.Console.WriteLine($"[Credentials] '{tokenFile}' was created, which means that the Discord Bot Token and the PostgreSQL information will need to be filled out. All PostgreSQL information should be alphanumeric.");
                System.Environment.Exit(1);
            }
            Config config = null;

            try {
                config = new Newtonsoft.Json.JsonSerializer().Deserialize<Tomoe.Config>(new JsonTextReader(new StreamReader(System.IO.File.OpenRead(tokenFile))));
            } catch (Newtonsoft.Json.JsonReaderException jsonException) {
                System.Console.WriteLine($"[Init] Invalid JSONC on '{tokenFile}'. {jsonException.Message} Exiting.");
                System.Environment.Exit(1);
            }

            return config;
        }
    }
}