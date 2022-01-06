using System;
using System.Text.Json.Serialization;
using Tomoe.Utils.Converters;

namespace Tomoe.Utils.Configs
{
    public class Config
    {
        [JsonPropertyName("discord_api_token")]
        public string DiscordApiToken { get; set; }

        [JsonPropertyName("discord_bot_prefix")]
        public string DiscordBotPrefix { get; set; }

        [JsonPropertyName("repository_link")]
        public string RepositoryLink { get; set; }

        [JsonPropertyName("reaction_timeout"), JsonConverter(typeof(JsonTimeSpanConverter))]
        public TimeSpan ReactionTimeout { get; set; }

        [JsonPropertyName("logger")]
        public Logger Logger { get; set; }

        [JsonPropertyName("database")]
        public Database Database { get; set; }

        [JsonPropertyName("update")]
        public Update Update { get; set; }
    }
}