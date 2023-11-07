using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Npgsql;
using OoLunar.Tomoe.Database.Models;
using OoLunar.Tomoe.Database.Models.Reminders;

namespace OoLunar.Tomoe.Database
{
    public sealed class DatabaseContext : DbContext, IDesignTimeDbContextFactory<DatabaseContext>
    {
        public DbSet<GuildModel> Guilds { get; init; } = null!;
        public DbSet<GuildMemberModel> Members { get; init; } = null!;
        public DbSet<PollModel> Polls { get; init; } = null!;
        public DbSet<SingleReminderModel> Reminders { get; init; } = null!;
        public DbSet<RepeatingReminderModel> RepeatingReminders { get; init; } = null!;
        public DbSet<TodoReminderModel> TodoReminders { get; init; } = null!;

        // Standard EFCore convention.
        public DatabaseContext() { }
        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options) { }

        public DatabaseContext CreateDbContext(string[] args)
        {
            ConfigurationBuilder configurationBuilder = new();
            configurationBuilder.Sources.Clear();
            configurationBuilder.AddJsonFile("config.json", true, true);
#if DEBUG
            configurationBuilder.AddJsonFile("config.debug.json", true, true);
#endif
            configurationBuilder.AddEnvironmentVariables("Tomoe__");
            configurationBuilder.AddCommandLine(args);

            IConfigurationRoot configuration = configurationBuilder.Build();
            DbContextOptionsBuilder<DatabaseContext> optionsBuilder = new();
            ConfigureOptions(optionsBuilder, configuration);
            return new(optionsBuilder.Options);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder) => modelBuilder.HasPostgresExtension("hstore");

        internal static void ConfigureOptions(DbContextOptionsBuilder optionsBuilder, IConfiguration configuration)
        {
            NpgsqlConnectionStringBuilder connectionBuilder = new()
            {
                ApplicationName = configuration.GetValue("database:application_name", "Tomoe Discord Bot"),
                Database = configuration.GetValue("database:database_name", "tomoe"),
                Host = configuration.GetValue("database:host", "localhost"),
                Username = configuration.GetValue("database:username", "tomoe"),
                Port = configuration.GetValue("database:port", 5432),
                Password = configuration.GetValue<string>("database:password")
            };

            optionsBuilder.UseNpgsql(connectionBuilder.ToString(), options => options.EnableRetryOnFailure(5).CommandTimeout(5));
        }
    }
}
