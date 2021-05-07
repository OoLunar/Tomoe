
namespace Tomoe.Utils.Configs
{
    using Serilog.Events;
    using System.Text.Json.Serialization;

    public class Logger
    {
        [JsonPropertyName("tomoe"), JsonConverter(typeof(JsonStringEnumConverter))]
        public LogEventLevel Tomoe { get; set; }

        [JsonPropertyName("discord"), JsonConverter(typeof(JsonStringEnumConverter))]
        public LogEventLevel Discord { get; set; }

        [JsonPropertyName("database"), JsonConverter(typeof(JsonStringEnumConverter))]
        public LogEventLevel Database { get; set; }

        [JsonPropertyName("show_id")]
        public bool ShowId { get; set; }

        [JsonPropertyName("save_to_file")]
        public bool SaveToFile { get; set; }
    }
}
