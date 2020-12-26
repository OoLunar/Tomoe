using System.Text.Json;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Tomoe.Utils
{
	internal class Logging
	{
		[JsonProperty("tomoe")]
		internal LogLevel Tomoe;

		[JsonProperty("discord")]
		internal LogLevel Discord;

		[JsonProperty("npgsql")]
		internal LogLevel Npgsql;

		[JsonProperty("show_id")]
		internal bool ShowId;

		[JsonProperty("save_to_file")]
		internal bool SaveToFile;

		[JsonConstructor]
		internal Logging(LogLevel tomoe = LogLevel.Information, LogLevel discord = LogLevel.Information, LogLevel npgsql = LogLevel.Information, bool showId = false, bool saveToFile = true)
		{
			Tomoe = tomoe;
			Discord = discord;
			Npgsql = npgsql;
			ShowId = showId;
			SaveToFile = saveToFile;
		}
	}
}
