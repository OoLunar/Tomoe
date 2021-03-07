using Serilog.Events;

namespace Tomoe.Utils
{
	public class LoggerConfig
	{
		public LogEventLevel Tomoe = LogEventLevel.Debug;
		public LogEventLevel Discord = LogEventLevel.Information;
		public LogEventLevel Database = LogEventLevel.Warning;
		public bool ShowId = true;
		public bool SaveToFile = true;
	}
}
