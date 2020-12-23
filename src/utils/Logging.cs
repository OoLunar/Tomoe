using System.Text.Json;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Tomoe.Utils {
    public class Logging {
        [JsonProperty("tomoe")]
        public LogLevel Tomoe;

        [JsonProperty("discord")]
        public LogLevel Discord;

        [JsonProperty("npgsql")]
        public LogLevel Npgsql;

        [JsonProperty("show_id")]
        public bool ShowId;

        [JsonProperty("save_to_file")]
        public bool SaveToFile;

        [JsonConstructor]
        public Logging(LogLevel tomoe = LogLevel.Information, LogLevel discord = LogLevel.Information, LogLevel npgsql = LogLevel.Information, bool showId = false, bool saveToFile = true) {
            Tomoe = tomoe;
            Discord = discord;
            Npgsql = npgsql;
            ShowId = showId;
            SaveToFile = saveToFile;
        }
    }
}