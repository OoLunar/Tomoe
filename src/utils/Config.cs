using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Tomoe.Database;
using Tomoe.Utils;

namespace Tomoe {
    public class Config {
        [JsonProperty("discord_api_token")]
        public static string Token;

        [JsonProperty("discord_bot_prefix")]
        public static string Prefix;

        [JsonProperty("tomoe_log_level")]
        public static LogLevel LogLevel;

        [JsonProperty("discord_log_level")]
        public static LogLevel DiscordLogLevel;

        [JsonProperty("npgsql_log_level")]
        public static LogLevel NpgsqlLogLevel;

        [JsonProperty("database")]
        public static Driver Database;

        [JsonProperty("repository_link")]
        public static string RepositoryLink;

        public static string ProjectRoot = Path.GetFullPath("../../../../", System.AppDomain.CurrentDomain.BaseDirectory).Replace('\\', '/');
        private static string tokenFile = Path.Combine(ProjectRoot, "res/config.jsonc");
        private static string dialogueFile = Path.Combine(ProjectRoot, "res/dialog.jsonc");
        private static Logger _logger = new Logger("Config", LogLevel.Information);

        public static async Task Init() {
            _logger.Info("Starting...");
            if (!File.Exists(Path.Join(ProjectRoot, "res/config.jsonc"))) {
                Utils.FileSystem.CreateFile(tokenFile);
                File.WriteAllText(tokenFile, "{\r\n\t/*Token to have your bot login to Discord.*/\r\n\t\"discord_api_token\": \"\",\r\n\t/*Log to file instead of console.*/\r\n\t\"log_to_file\": false,\r\n\t/*The default prefix when the bot joins.*/\r\n\t\"default_prefix\": \">>\",\r\n\t/* Options are:\r\n\t * - None\r\n\t * - Trace\r\n\t * - Debug\r\n\t * - Information\r\n\t * - Warn\r\n\t * - Error\r\n\t * - Critical\r\n\t */\r\n\t\"log_level\": \"Information\",\r\n\t/*PostgresSQL database. More support will be added in the future.*/\r\n\t\"database_type\": \"postgres\",\r\n\t/*The IP or hostname that the database is listening on.*/\r\n\t\"host\": \"example.com\",\r\n\t/*The port that the database is listening on.*/\r\n\t\"port\": 5432,\r\n\t/*The username used to login to the database.*/\r\n\t\"username\": \"tomoe\",\r\n\t/*An Postgres compliant password used to login. Must be Postgres compliant.*/\r\n\t\"password\": \"someAlphanumericPassword\",\r\n\t/*Database name.*/\r\n\t\"database_name\": \"tomoe\",\r\n\t/* Choose security levels. Options are:\r\n\t\t* - Require\r\n\t\t* - Prefer\r\n\t\t* - Disable\r\n\t\t* Defaults to Require.\r\n\t*/\r\n\t\"ssl_mode\": \"Require\",\r\n\t/*For those who choose SQLite instead of Postgres*/\r\n\t\"database_filepath\": \"db/\"\r\n}");
                System.Console.WriteLine($"[Credentials] '{tokenFile}' was created, which means that the Discord Bot Token and the PostgreSQL information will need to be filled out. All PostgreSQL information should be alphanumeric.");
                System.Environment.Exit(1);
            }

            try {
                new Newtonsoft.Json.JsonSerializer().Deserialize<Config>(new JsonTextReader(new StreamReader(System.IO.File.OpenRead(tokenFile))));
            } catch (Newtonsoft.Json.JsonReaderException jsonException) {
                _logger.Critical($"Invalid JSONC on '{tokenFile}'. {jsonException.Message}");
            } catch (Newtonsoft.Json.JsonSerializationException typeException) {
                _logger.Critical($"Error resolving config option on '{tokenFile}' Make sure all the config options are valid. Error: {typeException.Message}");
            }

            Thread.Sleep(50);
        }
    }
}