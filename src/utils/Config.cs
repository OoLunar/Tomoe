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

        private static string tokenFile = Path.Combine(FileSystem.ProjectRoot, "res/config.jsonc");
        private static Logger _logger = new Logger("Config", LogLevel.Information);

        public static async Task Init() {
            _logger.Info("Starting...");
            if (!File.Exists(Path.Join(FileSystem.ProjectRoot, "res/config.jsonc"))) {
                Utils.FileSystem.CreateFile(tokenFile);
                File.WriteAllText(tokenFile, "{\r\n\t/*Token to have your bot login to Discord.*/\r\n\t\"discord_api_token\": \"<insert Discord bot token>\",\r\n\t/*The default prefix when the bot joins.*/\r\n\t\"discord_bot_prefix\": \">>\",\r\n\t/* Options are:\r\n\t * - None\r\n\t * - Trace\r\n\t * - Debug\r\n\t * - Information\r\n\t * - Warn\r\n\t * - Error\r\n\t * - Critical\r\n\t */\r\n\t\"tomoe_log_level\": \"Information\",\r\n\t\"discord_log_level\": \"Information\",\r\n\t/*Ignored when database.driver is not set to PostgresSQL*/\r\n\t\"npgsql_log_level\": \"Information\",\r\n\t/*An HTTPS link to the git repository.*/\r\n\t\"repository_link\": \"https://github.com/OoLunar/Tomoe.git\",\r\n\t/*Database details*/\r\n\t\"database\": {\r\n\t\t/*PostgresSQL database. More support will be added in the future.*/\r\n\t\t\"driver\": \"PostgresSQL\",\r\n\t\t/*The IP or hostname that the database is listening on.*/\r\n\t\t\"host\": \"<insert database host here>\",\r\n\t\t/*The port that the database is listening on.*/\r\n\t\t\"port\": 5432,\r\n\t\t/*The username used to login to the database.*/\r\n\t\t\"username\": \"tomoe_discord_bot\",\r\n\t\t/*An Postgres compliant password used to login. Must be Postgres compliant.*/\r\n\t\t\"password\": \"<insert postgres password here>\",\r\n\t\t/*Database name.*/\r\n\t\t\"database_name\": \"tomoe\",\r\n\t\t/* Choose security levels. Options are:\r\n\t\t* - Require\r\n\t\t* - Prefer\r\n\t\t* - Disable\r\n\t\t* Defaults to Require.\r\n\t\t*/\r\n\t\t\"ssl_mode\": \"Require\"\r\n\t}\r\n}");
                _logger.Critical($"'{tokenFile}' was created, which means that the Discord Bot Token and the PostgreSQL information will need to be filled out. All PostgreSQL information should be alphanumeric.");
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