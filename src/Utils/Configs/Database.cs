using System.Text.Json.Serialization;

namespace Tomoe.Utils.Configs
{
	public class Database
	{
		[JsonPropertyName("application_name")]
		public string ApplicationName;

		[JsonPropertyName("database_name")]
		public string DatabaseName;

		[JsonPropertyName("host")]
		public string Host;

		[JsonPropertyName("password")]
		public string Password;

		[JsonPropertyName("username")]
		public string Username;

		[JsonPropertyName("port")]
		public int Port;
	}
}
