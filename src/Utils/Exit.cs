using System;
using Microsoft.Extensions.DependencyInjection;

using Tomoe.Db;

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
			Commands.Public.Assignments.Timer.Dispose();
			_logger.Info("Closing Discord...");
			await Program.Client.StopAsync();
			_logger.Info("Closing database...");
			_ = await Program.ServiceProvider.GetService<Database>().SaveChangesAsync();
			_logger.Info("Goodbyte!");
			Environment.Exit(0);
		}
	}
}
