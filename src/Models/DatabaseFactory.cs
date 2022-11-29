using System.Globalization;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Npgsql;
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
			NpgsqlConnectionStringBuilder connectionBuilder = new()
			{
				ApplicationName = configuration.GetValue("database:applicationName", "Tomoe Discord Bot"),
				Database = configuration.GetValue("database:databaseName", "tomoe"),
				Host = configuration.GetValue("database:host", "localhost"),
				Username = configuration.GetValue("database:username", "tomoe"),
				Port = configuration.GetValue("database:port", 5432),
				Password = configuration.GetValue<string>("database:password")
			};
			optionsBuilder.UseNpgsql(connectionBuilder.ToString(), options => options.EnableRetryOnFailure(5));
			optionsBuilder.UseSnakeCaseNamingConvention(CultureInfo.InvariantCulture);

			DatabaseContext databaseContext = new(optionsBuilder.Options);
			return databaseContext;
		}
	}
}
