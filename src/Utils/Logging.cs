using System.Text.Json;

using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

namespace Tomoe.Utils
{
	internal class Logging
	{
		[JsonProperty("tomoe")]
		public readonly LogLevel Tomoe;

		[JsonProperty("discord")]
		public readonly LogLevel Discord;

		[JsonProperty("npgsql")]
		public readonly LogLevel Npgsql;

		[JsonProperty("show_commands_id")]
		public readonly bool ShowId;

		[JsonProperty("save_to_file")]
		public readonly bool SaveToFile;

		[JsonConstructor]
		public Logging(LogLevel tomoe = LogLevel.Information, LogLevel discord = LogLevel.Information, LogLevel npgsql = LogLevel.Information, bool showId = false, bool saveToFile = false)
		{
			Tomoe = tomoe;
			Discord = discord;
			Npgsql = npgsql;
			ShowId = showId;
			SaveToFile = saveToFile;
		}
	}
}
