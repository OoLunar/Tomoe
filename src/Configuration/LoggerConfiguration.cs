using System.Collections.Generic;
using Serilog;
using Serilog.Events;

namespace OoLunar.Tomoe.Configuration
{
    public sealed record LoggerConfiguration
    {
        public string Format { get; init; } = "[{Timestamp:O}] [{Level:u4}] {SourceContext}: {Message:lj}{NewLine}{Exception}";
        public string Path { get; init; } = "logs";
        public string FileName { get; init; } = "yyyy'-'MM'-'dd' 'HH'.'mm'.'ss";
        public LogEventLevel LogLevel { get; init; } = LogEventLevel.Information;
        public RollingInterval RollingInterval { get; init; } = RollingInterval.Day;
        public IReadOnlyDictionary<string, LogEventLevel> Overrides { get; init; } = new Dictionary<string, LogEventLevel>();
    }
}
