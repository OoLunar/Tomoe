using System;
using System.Linq;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Npgsql;

namespace Tomoe.Db
{
	public class Database : DbContext
	{
		public DbSet<Guild> Guilds { get; set; }
		public DbSet<Strike> Strikes { get; set; }
		public DbSet<Assignment> Assignments { get; set; }

		protected override void OnConfiguring(DbContextOptionsBuilder options)
		{
			NpgsqlConnectionStringBuilder connectionBuilder = new();
			connectionBuilder.ApplicationName = "Tomoe";
			connectionBuilder.Database = "tomoe";
			connectionBuilder.Host = "mail.forsaken-borders.net";
			connectionBuilder.Password = "253ef055d412bf500ae9ce2f2de29ea96f71696734b91c49eae279d56489e299f6759095591e225b";
			connectionBuilder.Username = "tomoe_discord_bot";
			_ = options.UseNpgsql(connectionBuilder.ToString());
			_ = options.EnableSensitiveDataLogging(true);
			_ = options.UseLoggerFactory(Program.ServiceProvider.GetService<ILoggerFactory>());
		}
		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			_ = modelBuilder.Entity<Guild>()
			.Property(guild => guild.AllowedInvites)
			.HasConversion(
				allowedInvites => string.Join('\v', allowedInvites),
				allowedInvites => allowedInvites.Split('\v', StringSplitOptions.RemoveEmptyEntries).ToList()
			);

			_ = modelBuilder.Entity<Guild>()
			.Property(guild => guild.IgnoredChannels)
			.HasConversion(
				ignoredChannels => string.Join(';', ignoredChannels),
				ignoredChannels => ignoredChannels.Split(';', StringSplitOptions.RemoveEmptyEntries).Cast<ulong>().ToList()
			);

			_ = modelBuilder.Entity<Guild>()
			.Property(guild => guild.AdminRoles)
			.HasConversion(
				adminRoles => string.Join(';', adminRoles),
				adminRoles => adminRoles.Split(';', StringSplitOptions.RemoveEmptyEntries).Cast<ulong>().ToList()
			);

			_ = modelBuilder.Entity<GuildUser>()
			.Property(guildUser => guildUser.Roles)
			.HasConversion(
				roles => string.Join(';', roles),
				roles => roles.Split(';', StringSplitOptions.RemoveEmptyEntries).Cast<ulong>().ToList()
			);

			_ = modelBuilder.Entity<Strike>()
			.Property(strike => strike.Reasons)
			.HasConversion(
				reason => string.Join('\v', reason),
				reason => reason.Split('\v', StringSplitOptions.RemoveEmptyEntries).ToList()
			);

			_ = modelBuilder.Entity<Strike>()
			.Property(strike => strike.Id)
			.ValueGeneratedOnAdd();

			_ = modelBuilder.Entity<Tag>()
			.Property(tag => tag.TagId)
			.ValueGeneratedOnAdd();
		}
	}
}
