using System.Text.Json;

using Newtonsoft.Json;
using Serilog.Events;

namespace Tomoe.Utils
{
	internal class DatabaseConfig
	{
		[JsonProperty("application_name")]
		public readonly string ApplicationName;

		[JsonProperty("database_name")]
		public readonly string DatabaseName;

		[JsonProperty("host")]
		public readonly string Host;

		[JsonProperty("password")]
		public readonly string Password;

		[JsonProperty("username")]
		public readonly string Username;

		[JsonProperty("port")]
		public readonly int Port;

		[JsonConstructor]
		public DatabaseConfig(string applicationName, string databaseName, string host, string password, string username, int port)
		{
			ApplicationName = applicationName;
			DatabaseName = databaseName;
			Host = host;
			Password = password;
			Username = username;
			Port = port;
		}
	}
}
