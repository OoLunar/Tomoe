using System.Collections.Generic;
using System.Linq;
using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;

namespace Tomoe.Models
{
    public class DatabaseContext : DbContext
    {
        public DbSet<PollModel> Polls { get; init; } = null!;
        public DbSet<AutoModel<IMention>> AutoMentions { get; init; } = null!;
        public DbSet<TempRoleModel> TempRoles { get; init; } = null!;

        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options) { }

        public List<IAutoModel> GetAutoModels(ulong guildId)
        {
            // TODO: Should use reflection to get all AutoModel<T> properties and add it to this list.
            List<IAutoModel> autoModels = new();
            autoModels.AddRange(AutoMentions.Where(x => x.GuildId == guildId).AsEnumerable());
            return autoModels;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder) => modelBuilder.Entity<AutoModel<IMention>>().Property(autoMention => autoMention.Values).HasPostgresArrayConversion(x => x.ToString(), x => x!.ToMention());
    }
}