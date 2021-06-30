namespace Tomoe.Db
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Design;
    using Npgsql;
    using System;
    using System.Globalization;
    using System.IO;
    using System.Net;
    using Tomoe.Utilities.Configs;

    public class BloggingContextFactory : IDesignTimeDbContextFactory<Database>
    {
        public Database CreateDbContext(string[] args)
        {
            string tokenFile = Environment.GetEnvironmentVariable("CONFIG_FILE");
            if (tokenFile != null && !File.Exists(tokenFile))
            {
                Console.WriteLine($"The config file \"{tokenFile}\" does not exist. Consider removed the $CONFIG_FILE environment variable or making sure the file exists.");
                Environment.Exit(1);
            }
            else if (File.Exists("res/config.jsonc.prod"))
            {
                // Look for production file first. Contributers are expected not to fill out res/config.jsonc, but res/config.jsonc.prod instead.
                tokenFile = "res/config.jsonc.prod";
            }
            else if (File.Exists("res/config.jsonc"))
            {
                tokenFile = "res/config.jsonc";
            }
            else
            {
                // No config file could be found. Download it for them and inform them of the issue.
                WebClient webClient = new();
                webClient.DownloadFile("https://raw.githubusercontent.com/OoLunar/Tomoe/master/res/config.jsonc", "res/config.jsonc");
                Console.WriteLine("The config file was downloaded. Please go fill out \"res/config.jsonc\". It is recommended to use \"res/config.jsonc.prod\" if you intend on contributing to Tomoe.");
                Environment.Exit(1);
            }

            Config config = Config.Load().GetAwaiter().GetResult();
            DbContextOptionsBuilder<Database> options = new();
            NpgsqlConnectionStringBuilder connectionBuilder = new();
            connectionBuilder.ApplicationName = config.Database.ApplicationName;
            connectionBuilder.Database = config.Database.DatabaseName;
            connectionBuilder.Host = config.Database.Host;
            connectionBuilder.Password = config.Database.Password;
            connectionBuilder.Username = config.Database.Username;
            connectionBuilder.Port = config.Database.Port;
            options.UseNpgsql(connectionBuilder.ToString(), npgsqlOptions => npgsqlOptions.EnableRetryOnFailure());
            options.UseSnakeCaseNamingConvention(CultureInfo.InvariantCulture);
            options.EnableSensitiveDataLogging();
            options.EnableDetailedErrors();

            return new Database(options.Options);
        }
    }
}