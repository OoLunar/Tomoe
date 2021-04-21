using System.Text.Json.Serialization;
using Serilog.Events;

namespace Tomoe.Utils.Configs
{
	public class Logger
	{
		[JsonPropertyName("tomoe"), JsonConverter(typeof(JsonStringEnumConverter))]
		public LogEventLevel Tomoe { get; internal set; }

		[JsonPropertyName("discord"), JsonConverter(typeof(JsonStringEnumConverter))]
		public LogEventLevel Discord { get; internal set; }

		[JsonPropertyName("database"), JsonConverter(typeof(JsonStringEnumConverter))]
		public LogEventLevel Database { get; internal set; }

		[JsonPropertyName("show_id")]
		public bool ShowId { get; internal set; }

		[JsonPropertyName("save_to_file")]
		public bool SaveToFile { get; internal set; }
	}
}
