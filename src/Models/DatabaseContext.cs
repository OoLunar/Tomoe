using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;

namespace Tomoe.Models
{
    public class DatabaseContext : DbContext
    {
        public DbSet<TagModel> Tags { get; init; } = null!;
        public DbSet<GuildConfigModel> GuildConfigs { get; init; } = null!;
        public DbSet<SnowflakePermissionsModel> SnowflakePerms { get; init; } = null!;
        public DbSet<AutoReactionModel> AutoReactions { get; init; } = null!;
        public DbSet<AutoMentionModel> AutoMentions { get; init; } = null!;

        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder) => modelBuilder.Entity<AutoReactionModel>().Property(autoReaction => autoReaction.Regex).HasConversion(regex => regex == null ? "" : regex.ToString(), regex => new Regex(regex));
    }
}