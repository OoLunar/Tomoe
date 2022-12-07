using System;
using System.Globalization;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Npgsql;
using OoLunar.Tomoe.Database.Models;

namespace OoLunar.Tomoe.Database
{
    public sealed class DatabaseContext : DbContext, IDesignTimeDbContextFactory<DatabaseContext>
    {
        public DbSet<GuildModel> Guilds { get; init; } = null!;
        public DbSet<GuildMemberModel> Members { get; init; } = null!;
        public DbSet<RoleMenuModel> RoleMenus { get; init; } = null!;
        public DbSet<PollModel> Polls { get; init; } = null!;

        // Standard EFCore convention.
        public DatabaseContext() { }
        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options) { }

        public DatabaseContext CreateDbContext(string[] args)
        {
            ConfigurationBuilder configurationBuilder = new();
            configurationBuilder.Sources.Clear();
#if DEBUG
            configurationBuilder.AddJsonFile(Path.Join(Environment.CurrentDirectory, "../res", "config.json"), true, true);
            configurationBuilder.AddJsonFile(Path.Join(Environment.CurrentDirectory, "../res", "config.json.prod"), true, true);
#else
            configurationBuilder.AddJsonFile(Path.Join(Environment.CurrentDirectory, "res", "config.json"), true, true);
            configurationBuilder.AddJsonFile(Path.Join(Environment.CurrentDirectory, "res", "config.json.prod"), true, true);
#endif
            configurationBuilder.AddEnvironmentVariables("TOMOE_");
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
            optionsBuilder.UseNpgsql(connectionBuilder.ToString(), options => options.EnableRetryOnFailure(5));
            optionsBuilder.UseSnakeCaseNamingConvention(CultureInfo.InvariantCulture);
        }
    }
}
