using System;

namespace Tomoe.Utils
{
	public class Quit
	{
		private static readonly Logger _logger = new("Exit");
		public static async void ConsoleShutdown(object sender, ConsoleCancelEventArgs args)
		{
			Console.Write("\b\b");
			_logger.Info("Shutting down...");
			_logger.Info("Closing routines...");
			Commands.Public.Reminders.Timer.Dispose();
			_logger.Info("Closing database...");
			Program.Database.Dispose();
			_logger.Info("Closing Discord...");
			await Program.Client.StopAsync();
			_logger.Info("Goodbyte!");
			Environment.Exit(0);
		}
	}
}
