using System;
using System.Linq;

using Microsoft.EntityFrameworkCore;

namespace Tomoe.Db
{
	public class Database : DbContext
	{
		public DbSet<Guild> Guilds { get; set; }
		public DbSet<Strike> Strikes { get; set; }
		public DbSet<Assignment> Assignments { get; set; }

		protected override void OnConfiguring(DbContextOptionsBuilder options)
		{
			_ = options.UseSqlite($"Data Source={Utils.Config.DatabaseFilePath}");
			_ = options.EnableSensitiveDataLogging(true);
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
		}
	}
}
