using Microsoft.EntityFrameworkCore;

namespace Tomoe.Db
{
    public class Database : DbContext
    {
        public Database(DbContextOptions<Database> options) : base(options) { }

        public DbSet<ReactionRole> ReactionRoles { get; set; }
        public DbSet<AutoReaction> AutoReactions { get; set; }
        public DbSet<ModLog> ModLogs { get; set; }
        public DbSet<Strike> Strikes { get; set; }
        public DbSet<GuildUser> GuildUsers { get; set; }
        public DbSet<GuildConfig> GuildConfigs { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<Lock> Locks { get; set; }
        public DbSet<LogSetting> LogSettings { get; set; }
        public DbSet<Reminder> Reminders { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder) { }
    }
}