using System;
using System.Collections.Generic;
using System.Linq;
using ConcurrentCollections;
using EdgeDB;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace OoLunar.Tomoe.Database.Models
{
    /// <summary>
    /// A database representation of a Discord guild.
    /// </summary>
    [EdgeDBType("Guild")]
    public sealed class GuildModel : DatabaseTrackable<GuildModel>
    {
        /// <summary>
        /// The guild id.
        /// </summary>
        public ulong GuildId { get; private init; }

        /// <summary>
        /// Whether the bot is in the guild.
        /// </summary>
        public bool Disabled { get; private set; }

        /// <summary>
        /// Whether to restore a previous member's roles.
        /// </summary>
        public bool KeepRoles { get; private set; }

        /// <summary>
        /// Whether to attempt to DM reminders before sending them in the channel the reminder was set in. This is only a strong suggestion, not a requirement.
        /// </summary>
        public bool TryDmReminders { get; private set; }

        /// <summary>
        /// A read-only list of the prefixes the bot will respond to. They contain additonal information such as who set the prefix and when the prefix was created.
        /// </summary>
        [EdgeDBIgnore]
        public IReadOnlyList<GuildPrefixModel> Prefixes => _prefixes.ToArray();
        private ConcurrentHashSet<GuildPrefixModel> _prefixes { get; set; } = new();

        /// <summary>
        /// A read-only list of the members in the guild.
        /// </summary>
        [EdgeDBIgnore]
        public IReadOnlyList<GuildMemberModel> Members => _members.ToArray();
        private ConcurrentHashSet<GuildMemberModel> _members { get; set; } = new();

        private ILogger<GuildModel> Logger { get; init; } = null!;

        public GuildModel() { }
        internal GuildModel(ILogger<GuildModel> logger, IConfiguration configuration, ulong id)
        {
            ArgumentNullException.ThrowIfNull(logger, nameof(logger));
            ArgumentNullException.ThrowIfNull(configuration, nameof(configuration));

            Logger = logger;
            GuildId = id;
            _prefixes = new(configuration.GetSection("discord:prefixes")
                .Get<IEnumerable<string>>()
                .Select(prefix => new GuildPrefixModel(prefix, id)));
        }

        internal void SetPrefixes(IEnumerable<GuildPrefixModel> prefixes)
        {
            ArgumentNullException.ThrowIfNull(prefixes, nameof(prefixes));

            Logger.LogDebug("Setting prefixes for guild {GuildId} to {Prefixes}", GuildId, prefixes);
            _prefixes = new(prefixes);
        }

        internal void AddMembers(IEnumerable<GuildMemberModel> newMembers)
        {
            ArgumentNullException.ThrowIfNull(newMembers, nameof(newMembers));

            int totalMemberCount = 0;
            foreach (GuildMemberModel member in newMembers)
            {
                if (member.GuildModel.GuildId != GuildId)
                {
                    Logger.LogWarning("AddMembersAsync: Member {MemberId} from guild {MemberGuild} was attempted to be added to guild {GuildId}.", member.UserId, member.GuildModel.GuildId, GuildId);
                    continue;
                }
                _members.Add(member);
                totalMemberCount++;
            }

            if (totalMemberCount == 0)
            {
                Logger.LogWarning("AddMembers: No members were provided.");
            }
            else
            {
                Logger.LogDebug("Added {MemberCount:N0} members to guild {GuildId}.", totalMemberCount, GuildId);
            }
        }

        internal void RemoveMembers(IEnumerable<GuildMemberModel> membersToRemove)
        {
            ArgumentNullException.ThrowIfNull(membersToRemove, nameof(membersToRemove));

            int totalMemberCount = 0;
            foreach (GuildMemberModel member in membersToRemove)
            {
                if (member.GuildModel.GuildId != GuildId)
                {
                    Logger.LogWarning("RemoveMembersAsync: Member {MemberId} from guild {MemberGuild} was attempted to be removed from guild {GuildId}.", member.UserId, member.GuildModel.GuildId, GuildId);
                    continue;
                }

                if (_members.TryRemove(member))
                {
                    totalMemberCount++;
                }
            }

            Logger.LogDebug("Added {MemberCount:N0} members to guild {GuildId}.", totalMemberCount, GuildId);
        }
    }
}
