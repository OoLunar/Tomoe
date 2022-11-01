using System;
using System.Collections.Generic;
using System.Linq;
using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;

namespace Tomoe.Db
{
    public sealed class Database : DbContext
    {
        public DbSet<MenuRole> MenuRoles { get; init; } = null!;
        public DbSet<AutoReaction> AutoReactions { get; init; } = null!;
        public DbSet<ModLog> ModLogs { get; init; } = null!;
        public DbSet<Strike> Strikes { get; init; } = null!;
        public DbSet<GuildMember> GuildMembers { get; init; } = null!;
        public DbSet<GuildConfig> GuildConfigs { get; init; } = null!;
        public DbSet<Tag> Tags { get; init; } = null!;
        public DbSet<Lock> Locks { get; init; } = null!;
        public DbSet<LogSetting> LogSettings { get; init; } = null!;
        public DbSet<Reminder> Reminders { get; init; } = null!;
        public DbSet<PermanentButton> PermanentButtons { get; init; } = null!;
        public DbSet<TempRoleModel> TemporaryRoles { get; init; } = null!;

        public Database(DbContextOptions<Database> options) : base(options) { }
        protected override void OnModelCreating(ModelBuilder modelBuilder) { }

        public bool AddGuildMember(DiscordMember discordMember)
        {
            if (discordMember == null)
            {
                throw new ArgumentNullException(nameof(discordMember));
            }

            bool added = false;
            GuildMember? guildMember = GuildMembers.FirstOrDefault(databaseGuildMember => databaseGuildMember.GuildId == discordMember.Guild.Id && databaseGuildMember.UserId == discordMember.Id);
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
                GuildMember? guildMember = GuildMembers.FirstOrDefault(databaseGuildMember => databaseGuildMember.GuildId == discordMember.Guild.Id && databaseGuildMember.UserId == discordMember.Id);
                guildMember ??= new GuildMember()
                {
                    GuildId = discordMember.Guild.Id,
                    UserId = discordMember.Id,
                    Roles = discordMember.Roles.Except(new[] { discordMember.Guild.EveryoneRole }).Select(discordRole => discordRole.Id).ToList(),
                    JoinedAt = discordMember.JoinedAt.UtcDateTime
                };
                guildMembers.Add(guildMember);
                added.Add(guildMember.UserId);
            }

            GuildMembers.AddRange(guildMembers);
            return added;
        }
    }
}
