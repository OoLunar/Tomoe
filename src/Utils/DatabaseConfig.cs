using System.Collections.Generic;
using System.Text.Json;

using Newtonsoft.Json;

namespace Tomoe.Utils
{
	public class DatabaseConfig
	{
		[JsonProperty("driver")]
		internal string Driver = "sqlite";

		[JsonProperty("password")]
		internal string Password = "password";

		[JsonProperty("database_name")]
		internal string DatabaseName = "res/sql/Tomoe.db";

		[JsonProperty("parameters")]
		internal Dictionary<string, string> Parameters = new() { { "open_mode", "ReadWriteCreate" }, { "cache_mode", "Shared" } };

		[JsonProperty("max_retry_count")]
		internal int MaxRetryCount = 5;
	}
}
