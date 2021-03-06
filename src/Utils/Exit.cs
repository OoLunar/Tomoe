namespace Tomoe.Utils
{
    using DSharpPlus.Entities;
    using Microsoft.Extensions.DependencyInjection;
    using Serilog;
    using System;
    using Tomoe.Db;

    public class Quit
    {
        private static readonly ILogger _logger = Log.ForContext<Quit>();
        public static async void ConsoleShutdown(object sender, ConsoleCancelEventArgs args)
        {
            Console.Write("\b\b");
            _logger.Information("Shutting down...");
            _logger.Information("Closing Discord...");
            await Program.Client.UpdateStatusAsync(null, UserStatus.Offline);
            await Program.Client.DisconnectAsync();
            _logger.Information("Closing database...");
            await Program.ServiceProvider.GetService<Database>().SaveChangesAsync();
            _logger.Information("Goodbyte!");
            Environment.Exit(0);
        }
    }
}
