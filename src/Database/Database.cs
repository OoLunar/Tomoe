using Microsoft.EntityFrameworkCore;

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

		public Database(DbContextOptions<Database> options) : base(options) { }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			_ = modelBuilder.Entity<GuildUser>()
		   	.Property(b => b.Roles)
		   	.HasColumnType("bigint[]");

			_ = modelBuilder.Entity<GuildConfig>()
		   	.Property(b => b.AdminRoles)
		   	.HasColumnType("bigint[]");

			_ = modelBuilder.Entity<GuildConfig>()
		   	.Property(b => b.IgnoredChannels)
		   	.HasColumnType("bigint[]");
		}
	}
}
