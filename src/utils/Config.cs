using System;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Tomoe.Database;

namespace Tomoe.Utils
{
	internal class Config
	{
		[JsonProperty("discord_api_token")]
		public static string Token = string.Empty;

		[JsonProperty("discord_bot_prefix")]
		public static string Prefix = ">>";

		[JsonProperty("logging")]
		public static Logging Logging = new Logging();

		[JsonProperty("database")]
		public static DatabaseLoader Database;

		[JsonProperty("repository_link")]
		public static string RepositoryLink = "https://github.com/OoLunar/Tomoe.git";

		[JsonProperty("auto_update")]
		public static bool AutoUpdate = true;

		private static readonly string TokenFile = Path.Join(FileSystem.ProjectRoot, "res/config.jsonc");
		private static readonly Logger Logger = new Logger("Config", LogLevel.Information);

		public static async Task Init()
		{
			Logger.Info("Starting...");
			if (File.Exists(TokenFile))
			{
				try
				{
					_ = new Newtonsoft.Json.JsonSerializer().Deserialize<Config>(new JsonTextReader(new StreamReader(File.OpenRead(TokenFile))));
				}
				catch (JsonReaderException jsonException)
				{
					Logger.Critical($"Invalid JSONC on \"{TokenFile}\". {jsonException.Message}");
				}
				catch (JsonSerializationException typeException)
				{
					Logger.Critical($"Error resolving config option on \"{TokenFile}\" Make sure all the config options are valid. Error: {typeException.Message}");
				}
				Thread.Sleep(50);
				if (Logging.SaveToFile && !Directory.Exists(Path.Join(FileSystem.ProjectRoot, "log/")))
				{
					FileSystem.CreateDirectory(Path.Join(FileSystem.ProjectRoot, "log/"));
				}
			}
			else
			{
				_ = FileSystem.CreateFile(TokenFile);
				File.WriteAllText(TokenFile, "{\r\n    /*Token to have your bot login to Discord.*/\r\n    \"discord_api_token\": \"<insert Discord Token here>\",\r\n    /*The default prefix when the bot joins.*/\r\n    \"discord_bot_prefix\": \">>\",\r\n    /*An HTTPS link to the git repository.*/\r\n    \"repository_link\": \"https://github.com/OoLunar/Tomoe.git\",\r\n    /*Whether to auto-update using the git library*/\r\n    \"auto_update\": true,\r\n    \"logging\": {\r\n        /* Options are:\r\n\t     * - None\r\n\t     * - Trace\r\n\t     * - Debug\r\n\t     * - Information\r\n\t     * - Warn\r\n\t     * - Error\r\n\t     * - Critical\r\n        */\r\n        \"tomoe\": \"Information\",\r\n        \"discord\": \"Information\",\r\n        /*Ignored when database.driver is not set to PostgresSQL*/\r\n        \"npgsql\": \"Information\",\r\n        \"show_commands_id\": true,\r\n        \"save_to_file\": true\r\n    },\r\n    /*Database details*/\r\n    \"database\": {\r\n        /*PostgresSQL database. More support will be added in the future.*/\r\n        \"driver\": \"PostgresSQL\",\r\n        /*The IP or hostname that the database is listening on.*/\r\n        \"host\": \"<insert hostname>\",\r\n        /*The port that the database is listening on.*/\r\n        \"port\": 5432,\r\n        /*The username used to login to the database.*/\r\n        \"username\": \"tomoe_discord_bot\",\r\n        /*An Postgres compliant password used to login. Must be Postgres compliant.*/\r\n        \"password\": \"<insert password>\",\r\n        /*Database name.*/\r\n        \"database_name\": \"tomoe\",\r\n        /* Choose security levels. Options are:\r\n\t\t* - Require\r\n\t\t* - Prefer\r\n\t\t* - Disable\r\n\t\t* Defaults to Require.\r\n\t\t*/\r\n        \"ssl_mode\": \"Require\"\r\n    }\r\n}");
				Logger.Critical($"\"{TokenFile}\" was created, which means that the Discord Bot Token and the database driver information will need to be filled out.");
				Environment.Exit(1);
			}
		}
	}
}
