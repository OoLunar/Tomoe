using System.Text.Json.Serialization;

namespace Tomoe.Utils.Configs
{
	public class Config
	{
		[JsonPropertyName("discord_api_token")]
		public string DiscordApiToken;

		[JsonPropertyName("discord_bot_prefix")]
		public string DiscordBotPrefix;

		[JsonPropertyName("repository_link")]
		public string RepositoryLink;

		[JsonPropertyName("invite_link")]
		public string InviteLink;

		[JsonPropertyName("logger")]
		public Logger Logger;

		[JsonPropertyName("database")]
		public Database Database;
	}
}