using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Tomoe.Db
{
	public class Database : DbContext
	{
		public DbSet<ReactionRole> ReactionRoles { get; set; }
		public DbSet<AutoReaction> AutoReactions { get; set; }
		public DbSet<ModLog> ModLogs { get; set; }
		public DbSet<CommandUsage> CommandUsages { get; set; }
		public DbSet<Strike> Strikes { get; set; }
		public DbSet<GuildUser> GuildUsers { get; set; }
		public DbSet<GuildConfig> GuildConfigs { get; set; }
		public DbSet<Tag> Tags { get; set; }
		public DbSet<Lock> Locks { get; set; }

		public Database(DbContextOptions<Database> options) : base(options) { }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			// Convert List<ulong> to List<string> due to weird behaviour of EFCore + Postgres
			ValueComparer valueComparerUlong = new ValueComparer<List<ulong>>(
				(c1, c2) => c1.SequenceEqual(c2),
				c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
				c => c.ToList()
			);

			modelBuilder.Entity<GuildConfig>()
			.Property(guild => guild.IgnoredChannels)
			.HasConversion(
				ignoredChannels => ignoredChannels.ConvertAll(s => s.ToString()),
				ignoredChannels => ignoredChannels.ConvertAll(s => ulong.Parse(s))
			);

			modelBuilder.Entity<GuildConfig>()
			.Property(guild => guild.AdminRoles)
			.HasConversion(
				adminRoles => adminRoles.ConvertAll(s => s.ToString()),
				adminRoles => adminRoles.ConvertAll(s => ulong.Parse(s))
			).Metadata.SetValueComparer(valueComparerUlong);

			modelBuilder.Entity<GuildUser>()
			.Property(guildUser => guildUser.Roles)
			.HasConversion(
				guildUser => guildUser.ConvertAll(s => s.ToString()),
				guildUser => guildUser.ConvertAll(s => ulong.Parse(s))
			).Metadata.SetValueComparer(valueComparerUlong);
		}
	}
}
