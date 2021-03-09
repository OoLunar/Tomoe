using System.Text.Json.Serialization;
using Serilog.Events;

namespace Tomoe.Utils.Configs
{
	public class Logger
	{
		[JsonPropertyName("tomoe"), JsonConverter(typeof(JsonStringEnumConverter))]
		public LogEventLevel Tomoe;

		[JsonPropertyName("discord"), JsonConverter(typeof(JsonStringEnumConverter))]
		public LogEventLevel Discord;

		[JsonPropertyName("database"), JsonConverter(typeof(JsonStringEnumConverter))]
		public LogEventLevel Database;

		[JsonPropertyName("show_id")]
		public bool ShowId;

		[JsonPropertyName("save_to_file")]
		public bool SaveToFile;
	}
}
