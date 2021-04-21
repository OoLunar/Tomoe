using System;
using System.Text.Json.Serialization;
using Tomoe.Utils.Converters;

namespace Tomoe.Utils.Configs
{
	public class Config
	{
		[JsonPropertyName("discord_api_token")]
		public string DiscordApiToken { get; internal set; }

		[JsonPropertyName("discord_bot_prefix")]
		public string DiscordBotPrefix { get; internal set; }

		[JsonPropertyName("repository_link")]
		public string RepositoryLink { get; internal set; }

		[JsonPropertyName("reaction_timeout"), JsonConverter(typeof(JsonTimeSpanConverter))]
		public TimeSpan ReactionTimeout { get; internal set; }

		[JsonPropertyName("logger")]
		public Logger Logger { get; internal set; }

		[JsonPropertyName("database")]
		public Database Database { get; internal set; }
	}
}
