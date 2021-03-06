using System.Text.Json;

using Microsoft.Extensions.Logging;

using Newtonsoft.Json;
using Serilog.Events;

namespace Tomoe.Utils
{
	internal class LoggerConfig
	{
		[JsonProperty("tomoe")]
		public readonly LogEventLevel Tomoe;

		[JsonProperty("discord")]
		public readonly LogEventLevel Discord;

		[JsonProperty("database")]
		public readonly LogEventLevel Database;

		[JsonProperty("show_commands_id")]
		public readonly bool ShowId;

		[JsonProperty("save_to_file")]
		public readonly bool SaveToFile;

		[JsonConstructor]
		public LoggerConfig(LogEventLevel tomoe = LogEventLevel.Information, LogEventLevel discord = LogEventLevel.Information, LogEventLevel database = LogEventLevel.Error, bool showId = false, bool saveToFile = false)
		{
			Tomoe = tomoe;
			Discord = discord;
			Database = database;
			ShowId = showId;
			SaveToFile = saveToFile;
		}
	}
}
