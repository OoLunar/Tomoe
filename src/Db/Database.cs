namespace Tomoe.Db
{
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.ChangeTracking;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;

    public class Database : DbContext
    {
        public Database(DbContextOptions<Database> options) : base(options) { }

        public DbSet<MenuRole> MenuRoles { get; set; }
        public DbSet<AutoReaction> AutoReactions { get; set; }
        public DbSet<ModLog> ModLogs { get; set; }
        public DbSet<Strike> Strikes { get; set; }
        public DbSet<GuildMember> GuildMembers { get; set; }
        public DbSet<GuildConfig> GuildConfigs { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<Lock> Locks { get; set; }
        public DbSet<LogSetting> LogSettings { get; set; }
        public DbSet<Reminder> Reminders { get; set; }
        public DbSet<PermanentButton> PermanentButtons { get; set; }

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
                    ignoredChannels => ignoredChannels.ConvertAll(s => s.ToString(CultureInfo.InvariantCulture)),
                    ignoredChannels => ignoredChannels.ConvertAll(ulong.Parse)
                );

            modelBuilder.Entity<GuildConfig>()
                .Property(guild => guild.AdminRoles)
                .HasConversion(
                    adminRoles => adminRoles.ConvertAll(s => s.ToString(CultureInfo.InvariantCulture)),
                    adminRoles => adminRoles.ConvertAll(ulong.Parse)
                ).Metadata.SetValueComparer(valueComparerUlong);

            modelBuilder.Entity<GuildMember>()
                .Property(guildUser => guildUser.Roles)
                .HasConversion(
                    guildUser => guildUser.ConvertAll(s => s.ToString(CultureInfo.InvariantCulture)),
                    guildUser => guildUser.ConvertAll(ulong.Parse)
                ).Metadata.SetValueComparer(valueComparerUlong);
        }
    }
}