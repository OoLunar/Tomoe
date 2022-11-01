using System.Globalization;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Npgsql;
using Serilog;

namespace Tomoe.Utilities.Configs
{
    public class Database
    {
        [JsonPropertyName("application_name")]
        public string ApplicationName { get; set; }

        [JsonPropertyName("database_name")]
        public string DatabaseName { get; set; }

        [JsonPropertyName("host")]
        public string Host { get; set; }

        [JsonPropertyName("password")]
        public string Password { get; set; }

        [JsonPropertyName("username")]
        public string Username { get; set; }

        [JsonPropertyName("port")]
        public int Port { get; set; }

        public Task LoadAsync(ServiceCollection services)
        {
            Serilog.ILogger logger = Log.ForContext<Database>();
            services.AddDbContext<Models.Database>(options =>
            {
                NpgsqlConnectionStringBuilder connectionBuilder = new()
                {
                    ApplicationName = ApplicationName,
                    Database = DatabaseName,
                    Host = Host,
                    Username = Username,
                    Port = Port,
                    Password = Password
                };
                options.UseNpgsql(connectionBuilder.ToString(), options => options.EnableRetryOnFailure());
                options.UseLoggerFactory(services.BuildServiceProvider().GetService<ILoggerFactory>());
                options.UseSnakeCaseNamingConvention(CultureInfo.InvariantCulture);
#if DEBUG
                options.EnableSensitiveDataLogging();
                options.EnableDetailedErrors();
#endif
            }, ServiceLifetime.Transient);

            using IServiceScope scope = services.BuildServiceProvider().CreateScope();
            Models.Database database = scope.ServiceProvider.GetService<Models.Database>();

            logger.Information("Connecting to the database...");
            //await database.Database.EnsureCreatedAsync();
            logger.Information("Database up!");

            return Task.CompletedTask;
        }
    }
}