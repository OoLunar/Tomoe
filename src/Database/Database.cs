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
	}
}
