using Microsoft.EntityFrameworkCore;

namespace Tomoe.Db
{
	public class Database : DbContext
	{
		public DbSet<ReactionRole> ReactionRoles { get; set; }
		public DbSet<AutoReaction> AutoReactions { get; set; }

		public Database(DbContextOptions<Database> options) : base(options) { }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			_ = modelBuilder
			.Entity<ReactionRole>()
			.Property(e => e.Id)
			.ValueGeneratedOnAdd();

			_ = modelBuilder
			.Entity<AutoReaction>()
			.Property(e => e.Id)
			.ValueGeneratedOnAdd();

		}
	}
}
