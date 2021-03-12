using System;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Tomoe.Db;

namespace Tomoe.Utils
{
	public class Quit
	{
		private static readonly ILogger _logger = Log.ForContext<Quit>();
		public static async void ConsoleShutdown(object sender, ConsoleCancelEventArgs args)
		{
			Console.Write("\b\b");
			_logger.Information("Shutting down...");
			_logger.Information("Closing routines...");
			await Commands.Public.Assignments.Dispose();
			_logger.Information("Closing Discord...");
			await Program.Client.StopAsync();
			_logger.Information("Closing database...");
			_ = await Program.ServiceProvider.GetService<Database>().SaveChangesAsync();
			_logger.Information("Goodbyte!");
			Environment.Exit(0);
		}
	}
}
