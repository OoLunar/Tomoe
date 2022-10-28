using System;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Tomoe.Utilities.Converters;

namespace Tomoe.Utilities.Configs
{
    public class Config
    {
        [JsonPropertyName("discord_api_token")]
        public string DiscordApiToken { get; set; }

        [JsonPropertyName("discord_bot_prefix")]
        public string DiscordBotPrefix { get; set; }

        [JsonPropertyName("discord_debug_guild_id")]
        public ulong DiscordDebugGuildId { get; set; }

        [JsonPropertyName("repository_link")]
        public string RepositoryLink { get; set; }

        [JsonPropertyName("reaction_timeout"), JsonConverter(typeof(JsonTimeSpanConverter))]
        public TimeSpan ReactionTimeout { get; set; }

        [JsonPropertyName("logger")]
        public Logger Logger { get; set; }

        [JsonPropertyName("database")]
        public Database Database { get; set; }

        public static async Task<Config> LoadAsync()
        {
            // Setup Config
            // Look for Environment variables for Docker. If the variable is set, but doesn't exist, assume it was improper configuration and exit.
            string tokenFile = Environment.GetEnvironmentVariable("CONFIG_FILE");
            if (string.IsNullOrWhiteSpace(tokenFile))
            {
                tokenFile = File.Exists("res/config.jsonc.prod")
                    ? "res/config.jsonc.prod"
                    : "res/config.jsonc";
            }

            if (!File.Exists(tokenFile))
            {
                // No config file could be found. Download it for them and inform them of the issue.
                HttpClient httpClient = new();
                httpClient.DefaultRequestHeaders.Add("UserAgent", "Tomoe/2.2.0");

                FileStream file = File.Open("res/config.jsonc", FileMode.OpenOrCreate, FileAccess.Write);
                await (await httpClient.GetStreamAsync("https://raw.githubusercontent.com/OoLunar/Tomoe/master/res/config.jsonc")).CopyToAsync(file);
                file.Close();
                Console.WriteLine("The config file was downloaded. Please go fill out \"res/config.jsonc\". It is recommended to use \"res/config.jsonc.prod\" if you intend on contributing to Tomoe.");
                Environment.Exit(1);
            }

            // Prefer JsonSerializer.DeserializeAsync over JsonSerializer.Deserialize due to being able to send the stream directly.
            return await JsonSerializer.DeserializeAsync<Config>(File.OpenRead(tokenFile), new JsonSerializerOptions() { IncludeFields = true, AllowTrailingCommas = true, ReadCommentHandling = JsonCommentHandling.Skip, PropertyNameCaseInsensitive = true });
        }
    }
}
