using System.Text.Json.Serialization;

namespace Tomoe.Utils.Configs
{
	public class Database
	{
		[JsonPropertyName("application_name")]
		public string ApplicationName { get; internal set; }

		[JsonPropertyName("database_name")]
		public string DatabaseName { get; internal set; }

		[JsonPropertyName("host")]
		public string Host { get; internal set; }

		[JsonPropertyName("password")]
		public string Password { get; internal set; }

		[JsonPropertyName("username")]
		public string Username { get; internal set; }

		[JsonPropertyName("port")]
		public int Port { get; internal set; }
	}
}
