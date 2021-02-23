using System.Text.Json;

using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

namespace Tomoe.Utils
{
	internal class LoggerConfig
	{
		[JsonProperty("tomoe")]
		public readonly LogLevel Tomoe;

		[JsonProperty("discord")]
		public readonly LogLevel Discord;

		[JsonProperty("show_commands_id")]
		public readonly bool ShowId;

		[JsonProperty("save_to_file")]
		public readonly bool SaveToFile;

		[JsonConstructor]
		public LoggerConfig(LogLevel tomoe = LogLevel.Information, LogLevel discord = LogLevel.Information, bool showId = false, bool saveToFile = false)
		{
			Tomoe = tomoe;
			Discord = discord;
			ShowId = showId;
			SaveToFile = saveToFile;
		}
	}
}
