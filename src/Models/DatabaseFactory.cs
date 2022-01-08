using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System.Globalization;
using System.IO;
using Tomoe.Utils;

namespace Tomoe.Models
{
    public class DatabaseFactory : IDesignTimeDbContextFactory<DatabaseContext>
    {
        public DatabaseContext CreateDbContext(string[] args)
        {
            FileUtils.CreateDefaultConfig();

            ConfigurationBuilder configurationBuilder = new();
            configurationBuilder.Sources.Clear();
            configurationBuilder.AddJsonFile(Path.Join(FileUtils.GetConfigPath(), "config.json"), true, true);
            configurationBuilder.AddJsonFile(Path.Join(FileUtils.GetConfigPath(), "config.json.prod"), true, true);
            configurationBuilder.AddEnvironmentVariables("TOMOE_");
            configurationBuilder.AddCommandLine(args);
            IConfigurationRoot configuration = configurationBuilder.Build();

            DbContextOptionsBuilder<DatabaseContext> optionsBuilder = new();
            NpgsqlConnectionStringBuilder connectionBuilder = new();
            connectionBuilder.ApplicationName = configuration.GetValue("database:applicationName", "Tomoe Discord Bot");
            connectionBuilder.Database = configuration.GetValue("database:databaseName", "tomoe");
            connectionBuilder.Host = configuration.GetValue("database:host", "localhost");
            connectionBuilder.Username = configuration.GetValue("database:username", "tomoe");
            connectionBuilder.Port = configuration.GetValue("database:port", 5432);
            connectionBuilder.Password = configuration.GetValue<string>("database:password");
            optionsBuilder.UseNpgsql(connectionBuilder.ToString(), options => options.EnableRetryOnFailure(5));
            optionsBuilder.UseSnakeCaseNamingConvention(CultureInfo.InvariantCulture);

            DatabaseContext databaseContext = new(optionsBuilder.Options);
            databaseContext.Database.EnsureCreated();

            return databaseContext;
        }
    }
}