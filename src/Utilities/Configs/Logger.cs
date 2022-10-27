using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.SystemConsole.Themes;

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
                .WriteTo.Console(theme: new AnsiConsoleTheme(new Dictionary<ConsoleThemeStyle, string>
                {
                    [ConsoleThemeStyle.Text] = "\x1b[0m",
                    [ConsoleThemeStyle.SecondaryText] = "\x1b[90m",
                    [ConsoleThemeStyle.TertiaryText] = "\x1b[90m",
                    [ConsoleThemeStyle.Invalid] = "\x1b[31m",
                    [ConsoleThemeStyle.Null] = "\x1b[95m",
                    [ConsoleThemeStyle.Name] = "\x1b[93m",
                    [ConsoleThemeStyle.String] = "\x1b[96m",
                    [ConsoleThemeStyle.Number] = "\x1b[95m",
                    [ConsoleThemeStyle.Boolean] = "\x1b[95m",
                    [ConsoleThemeStyle.Scalar] = "\x1b[95m",
                    [ConsoleThemeStyle.LevelVerbose] = "\x1b[34m",
                    [ConsoleThemeStyle.LevelDebug] = "\x1b[90m",
                    [ConsoleThemeStyle.LevelInformation] = "\x1b[36m",
                    [ConsoleThemeStyle.LevelWarning] = "\x1b[33m",
                    [ConsoleThemeStyle.LevelError] = "\x1b[31m",
                    [ConsoleThemeStyle.LevelFatal] = "\x1b[97;91m",
                }), outputTemplate: outputTemplate);

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
