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

		protected override void OnConfiguring(DbContextOptionsBuilder options) => options.UseSqlite($"Data Source={Utils.Config.DatabaseFilePath}");
		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			_ = modelBuilder.Entity<Guild>()
			.Property(e => e.AllowedInvites)
			.HasConversion(
				v => string.Join('\v', v).Replace("\\v", "\v"),
				v => v.Replace("\v", "\\v").Split('\v', StringSplitOptions.RemoveEmptyEntries).ToList()
			);

			_ = modelBuilder.Entity<Guild>()
			.Property(e => e.IgnoredChannels)
			.HasConversion(
				v => string.Join(';', v),
				v => v.Split(';', StringSplitOptions.RemoveEmptyEntries).Cast<ulong>().ToList()
			);

			_ = modelBuilder.Entity<Guild>()
			.Property(e => e.AdminRoles)
			.HasConversion(
				v => string.Join(';', v),
				v => v.Split(';', StringSplitOptions.RemoveEmptyEntries).Cast<ulong>().ToList()
			);

			_ = modelBuilder.Entity<GuildUser>()
			.Property(e => e.Roles)
			.HasConversion(
				v => string.Join(';', v),
				v => v.Split(';', StringSplitOptions.RemoveEmptyEntries).Cast<ulong>().ToList()
			);

			_ = modelBuilder.Entity<Strike>()
			.Property(e => e.Reason)
			.HasConversion(
				v => string.Join('\v', v).Replace("\\v", "\v"),
				v => v.Replace("\v", "\\v").Split('\v', StringSplitOptions.RemoveEmptyEntries).ToList()
			);
		}
	}
}
