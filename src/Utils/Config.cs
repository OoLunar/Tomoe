using System;
using System.IO;
using System.Net;
using System.Text.Json;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace Tomoe.Utils
{
	internal class Config
	{
		[JsonProperty("discord_api_token")]
		public static string Token = string.Empty;

		[JsonProperty("discord_bot_prefix")]
		public static string Prefix = ">>";

		[JsonProperty("logger_config")]
		public static LoggerConfig LoggerConfig = new();

		[JsonProperty("repository_link")]
		public static string RepositoryLink = "https://github.com/OoLunar/Tomoe.git";

		[JsonProperty("invite_link")]
		public static string InviteLink = "https://discord.com/oauth2/authorize?client_id=481314095723839508&permissions=8&scope=bot";

		[JsonProperty("auto_update")]
		public static bool AutoUpdate = true;

		private static readonly string TokenFile = File.Exists("res/config.jsonc.prod") ? "res/config.jsonc.prod" : "res/config.jsonc";

		public static async Task Init()
		{
			if (File.Exists(TokenFile))
			{
				try
				{
					_ = new Newtonsoft.Json.JsonSerializer().Deserialize<Config>(new JsonTextReader(new StreamReader(File.OpenRead(TokenFile))));
				}
				catch (JsonReaderException jsonException)
				{
					Console.WriteLine($"Invalid JSONC on \"{TokenFile}\". {jsonException.Message}");
					Environment.Exit(1);
				}
				catch (JsonSerializationException typeException)
				{
					Console.WriteLine($"Error resolving config option on \"{TokenFile}\" Make sure all the config options are valid. Error: {typeException.Message}");
					Environment.Exit(1);
				}

				if (LoggerConfig.SaveToFile && !Directory.Exists("log/"))
				{
					//TODO: Try catch this after we transfer to serilogger.
					_ = Directory.CreateDirectory("log/");
				}
			}
			else
			{
				WebClient webClient = new();
				webClient.DownloadFile("https://github.com/OoLunar/Tomoe/blob/master/res/config.jsonc", "res/config.jsonc");
				Console.WriteLine($"The config file was downloaded. Please go fill out \"res/config.jsonc\". It is recommended to use \"res/config.jsonc.prod\" if you intend on contributing to Tomoe.");
				Environment.Exit(1);
			}
		}
	}
}
