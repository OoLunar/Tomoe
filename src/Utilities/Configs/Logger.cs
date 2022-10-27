using System;
using System.Globalization;
using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;

namespace Tomoe.Utilities.Configs
{
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

        public void Load(ServiceCollection services)
        {
            // Setup Logger
            // Follow the ShowId property.
            string outputTemplate = ShowId ? "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] [{Level:u4}] [{ThreadId}] {SourceContext}: {Message:lj}{NewLine}{Exception}" : "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}] [{Level:u4}] {SourceContext}: {Message:lj}{NewLine}{Exception}";
            LoggerConfiguration loggerConfiguration = new LoggerConfiguration()
                .Enrich.WithThreadId()
                .MinimumLevel.Is(Tomoe)
                // Per library settings.
                .MinimumLevel.Override("DSharpPlus", Discord)
                .MinimumLevel.Override("Microsoft.EntityFrameworkCore", Database)
                // Use custom theme because the default one stinks
                .WriteTo.Console(theme: LoggerTheme.Lunar, outputTemplate: outputTemplate);

            if (SaveToFile)
            {
                loggerConfiguration.WriteTo.File($"logs/{DateTime.Now.ToLocalTime().ToString("yyyy'-'MM'-'dd' 'HH':'mm':'ss", CultureInfo.InvariantCulture)}.log", rollingInterval: RollingInterval.Day, outputTemplate: outputTemplate);
            }

            Log.Logger = loggerConfiguration.CreateLogger();
            services.AddLogging(loggingBuilder => loggingBuilder.AddSerilog(Log.Logger, true));
            Log.ForContext<Logger>().Information("Logger up!");
        }
    }
}
