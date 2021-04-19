using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using DSharpPlus.Interactivity.EventHandling;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Tomoe.Db
{
	public class Database : DbContext
	{
		public DbSet<ReactionRole> ReactionRoles { get; set; }

		public Database(DbContextOptions<Database> options) : base(options) { }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			_ = modelBuilder
			.Entity<ReactionRole>()
			.Property(e => e.GuildId)
			.ValueGeneratedNever();
		}
	}
}
