using System;
using System.Collections.Generic;
using System.Linq;
using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;

namespace Tomoe.Db
{
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
        public DbSet<TempRoleModel> TemporaryRoles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder) { }

        public bool AddGuildMember(DiscordMember discordMember)
        {
            if (discordMember == null)
            {
                throw new ArgumentNullException(nameof(discordMember));
            }

            bool added = false;
            GuildMember guildMember = GuildMembers.FirstOrDefault(databaseGuildMember => databaseGuildMember.GuildId == discordMember.Guild.Id && databaseGuildMember.UserId == discordMember.Id);
            if (guildMember == null)
            {
                added = true;
                guildMember = new GuildMember()
                {
                    GuildId = discordMember.Guild.Id,
                    UserId = discordMember.Id,
                    Roles = discordMember.Roles.Except(new[] { discordMember.Guild.EveryoneRole }).Select(discordRole => discordRole.Id).ToList(),
                    JoinedAt = discordMember.JoinedAt.UtcDateTime
                };

                GuildMembers.Add(guildMember);
            }

            return added;
        }

        public List<ulong> AddGuildMembers(IEnumerable<DiscordMember> discordMembers)
        {
            if (discordMembers == null)
            {
                throw new ArgumentNullException(nameof(discordMembers));
            }

            List<ulong> added = new();
            List<GuildMember> guildMembers = new();
            foreach (DiscordMember discordMember in discordMembers)
            {
                GuildMember guildMember = GuildMembers.FirstOrDefault(databaseGuildMember => databaseGuildMember.GuildId == discordMember.Guild.Id && databaseGuildMember.UserId == discordMember.Id);
                if (guildMember == null)
                {
                    guildMember = new GuildMember()
                    {
                        GuildId = discordMember.Guild.Id,
                        UserId = discordMember.Id,
                        Roles = discordMember.Roles.Except(new[] { discordMember.Guild.EveryoneRole }).Select(discordRole => discordRole.Id).ToList(),
                        JoinedAt = discordMember.JoinedAt.UtcDateTime
                    };
                }
                guildMembers.Add(guildMember);
                added.Add(guildMember.UserId);
            }

            GuildMembers.AddRange(guildMembers);
            return added;
        }
    }
}
