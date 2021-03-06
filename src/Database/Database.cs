using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
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

			// Tried to use dynamic and T, neither worked. D:
			// Create List<T> for each type.
			ValueComparer valueComparerString = new ValueComparer<List<string>>(
				(c1, c2) => c1.SequenceEqual(c2),
				c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
				c => c.ToList()
			);

			ValueComparer valueComparerUlong = new ValueComparer<List<ulong>>(
				(c1, c2) => c1.SequenceEqual(c2),
				c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
				c => c.ToList()
			);

			modelBuilder.Entity<Guild>()
			.Property(guild => guild.AllowedInvites)
			.HasConversion(
				allowedInvites => JsonConvert.SerializeObject(allowedInvites),
				allowedInvites => JsonConvert.DeserializeObject<List<string>>(allowedInvites)
			).Metadata.SetValueComparer(valueComparerString);

			modelBuilder.Entity<Guild>()
			.Property(guild => guild.IgnoredChannels)
			.HasConversion(
				ignoredChannels => JsonConvert.SerializeObject(ignoredChannels),
				ignoredChannels => JsonConvert.DeserializeObject<List<ulong>>(ignoredChannels)
			).Metadata.SetValueComparer(valueComparerUlong);

			modelBuilder.Entity<Guild>()
			.Property(guild => guild.AdminRoles)
			.HasConversion(
				adminRoles => JsonConvert.SerializeObject(adminRoles),
				adminRoles => JsonConvert.DeserializeObject<List<ulong>>(adminRoles)
			).Metadata.SetValueComparer(valueComparerUlong);

			modelBuilder.Entity<GuildUser>()
			.Property(guildUser => guildUser.Roles)
			.HasConversion(
				roles => JsonConvert.SerializeObject(roles),
				roles => JsonConvert.DeserializeObject<List<ulong>>(roles)
			).Metadata.SetValueComparer(valueComparerUlong);

			modelBuilder.Entity<Strike>()
			.Property(strike => strike.Reasons)
			.HasConversion(
				reasons => JsonConvert.SerializeObject(reasons),
				reasons => JsonConvert.DeserializeObject<List<string>>(reasons)
			).Metadata.SetValueComparer(valueComparerString);

			_ = modelBuilder.Entity<Strike>()
			.Property(strike => strike.Id)
			.ValueGeneratedOnAdd();

			_ = modelBuilder.Entity<Tag>()
			.Property(tag => tag.TagId)
			.ValueGeneratedOnAdd();
		}
	}
}
