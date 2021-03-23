using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Tomoe.Commands.Moderation;

namespace Tomoe.Db
{
	public class Database : DbContext
	{
		public DbSet<Guild> Guilds { get; set; }
		public DbSet<Strike> Strikes { get; set; }
		public DbSet<Assignment> Assignments { get; set; }
		public DbSet<ModLog> ModLogs { get; set; }

		public Database(DbContextOptions<Database> options) : base(options) { }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			// Tried to use dynamic and T, neither worked. D:
			// Create List<T> for each type.
			ValueComparer valueComparerUlong = new ValueComparer<List<ulong>>(
				(c1, c2) => c1.SequenceEqual(c2),
				c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
				c => c.ToList()
			);

			ValueComparer valueComparerUri = new ValueComparer<List<Uri>>(
				(c1, c2) => c1.SequenceEqual(c2),
				c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
				c => c.ToList()
			);

			ValueComparer valueComparerGuildUser = new ValueComparer<List<GuildUser>>(
				(c1, c2) => c1.SequenceEqual(c2),
				c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
				c => c.ToList()
			);

			ValueComparer valueComparerDictionary = new ValueComparer<Dictionary<int, ProgressiveStrike>>(
				(c1, c2) => c1.SequenceEqual(c2),
				c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
				c => c.ToDictionary(m => m.Key, m => m.Value)
			);

			ValueComparer valueComparerReactionRole = new ValueComparer<List<ReactionRole>>(
				(c1, c2) => c1.SequenceEqual(c2),
				c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
				c => c.ToList()
			);

			modelBuilder.Entity<Guild>()
			.Property(guild => guild.IgnoredChannels)
			.HasConversion(
				ignoredChannels => JsonSerializer.Serialize(ignoredChannels, default),
				ignoredChannels => JsonSerializer.Deserialize<List<ulong>>(ignoredChannels, default)
			).Metadata.SetValueComparer(valueComparerUlong);

			modelBuilder.Entity<Guild>()
			.Property(guild => guild.AdminRoles)
			.HasConversion(
				adminRoles => JsonSerializer.Serialize(adminRoles, default),
				adminRoles => JsonSerializer.Deserialize<List<ulong>>(adminRoles, default)
			).Metadata.SetValueComparer(valueComparerUlong);

			modelBuilder.Entity<Guild>()
			.Property(guild => guild.Users)
			.HasConversion(
				guildUsers => JsonSerializer.Serialize(guildUsers, default),
				guildUsers => JsonSerializer.Deserialize<List<GuildUser>>(guildUsers, default)
			).Metadata.SetValueComparer(valueComparerGuildUser);

			modelBuilder.Entity<GuildUser>()
			.Property(guildUser => guildUser.Roles)
			.HasConversion(
				roles => JsonSerializer.Serialize(roles, default),
				roles => JsonSerializer.Deserialize<List<ulong>>(roles, default)
			).Metadata.SetValueComparer(valueComparerUlong);

			_ = modelBuilder.Entity<Strike>()
			.Property(strike => strike.Id)
			.ValueGeneratedOnAdd();

			modelBuilder.Entity<Strike>()
			.Property(strike => strike.JumpLinks)
			.HasConversion(
				jumplinks => JsonSerializer.Serialize(jumplinks, default),
				jumplinks => JsonSerializer.Deserialize<List<Uri>>(jumplinks, default)
			).Metadata.SetValueComparer(valueComparerUri);

			_ = modelBuilder.Entity<Tag>()
			.Property(tag => tag.TagId)
			.ValueGeneratedOnAdd();

			_ = modelBuilder.Entity<Assignment>()
			.Property(assignment => assignment.Id)
			.ValueGeneratedOnAdd();

			_ = modelBuilder.Entity<Guild>()
			.Property(guild => guild.ReactionRoles)
			.ValueGeneratedNever();

			modelBuilder.Entity<Guild>()
			.Property(guild => guild.Punishments)
			.HasConversion(
				progressivePunishments => JsonSerializer.Serialize(progressivePunishments, default),
				progressivePunishments => JsonSerializer.Deserialize<Dictionary<int, ProgressiveStrike>>(progressivePunishments, default)
			).Metadata.SetValueComparer(valueComparerDictionary);

			modelBuilder.Entity<Guild>()
			.Property(guild => guild.ReactionRoles)
			.HasConversion(
				reactionRoles => JsonSerializer.Serialize(reactionRoles, default),
				reactionRoles => JsonSerializer.Deserialize<List<ReactionRole>>(reactionRoles, default)
			).Metadata.SetValueComparer(valueComparerReactionRole);

			base.OnModelCreating(modelBuilder);
		}
	}
}
