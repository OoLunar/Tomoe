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
        //[EdgeDBTypeConverter(typeof(UlongTypeConverter))]
        public ulong GuildId { get; private set; }

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
        //[EdgeDBTypeConverter(typeof(IEnumerableTypeConverter<GuildMemberModel, GuildMemberModel>))]
        private ConcurrentHashSet<GuildPrefixModel> _prefixes { get; set; } = new();

        /// <summary>
        /// A read-only list of the members in the guild.
        /// </summary>
        [EdgeDBIgnore]
        public IReadOnlyList<GuildMemberModel> Members => _members.ToArray();
        //[EdgeDBTypeConverter(typeof(IEnumerableTypeConverter<GuildMemberModel, GuildMemberModel>))]
        private ConcurrentHashSet<GuildMemberModel> _members { get; set; } = new();

        private ILogger<GuildModel> Logger { get; init; } = null!;

        public GuildModel() { }
        internal GuildModel(ILogger<GuildModel> logger, IConfiguration configuration, ulong id)
        {
            ArgumentNullException.ThrowIfNull(logger, nameof(logger));
            ArgumentNullException.ThrowIfNull(configuration, nameof(configuration));

            Logger = logger;
            GuildId = id;

            // Store the default prefixes in case they were to ever change, the user's would not need to guess the new prefixes.
            _prefixes = new(configuration.GetSection("discord:prefixes")
                .Get<IEnumerable<string>>()
                .Select(prefix => new GuildPrefixModel(prefix, id)));
        }

        /// <summary>
        /// Change the bot's prefixes on a guild.
        /// </summary>
        /// <param name="prefixes">The new prefixes that the bot should watch for.</param>
        internal void SetPrefixes(IEnumerable<GuildPrefixModel> prefixes)
        {
            ArgumentNullException.ThrowIfNull(prefixes, nameof(prefixes));

            Logger.LogDebug("Setting prefixes for guild {GuildId} to {Prefixes}", GuildId, prefixes);
            _prefixes = new(prefixes);
        }

        /// <summary>
        /// Adds new members to the guild. Ignores members who do not belong in the guild.
        /// </summary>
        /// <param name="newMembers">The new members who have joined the guild.</param>
        internal void AddMembers(IEnumerable<GuildMemberModel> newMembers)
        {
            ArgumentNullException.ThrowIfNull(newMembers, nameof(newMembers));

            int totalMemberCount = 0;
            foreach (GuildMemberModel member in newMembers)
            {
                // Skip so that the rest of the members may be added. We don't need only half of the list added because of one member.
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
                // Log when this happens, there should be logic that prevents this from happening.
                Logger.LogWarning("AddMembers: No members were provided.");
            }
            else
            {
                Logger.LogDebug("Added {MemberCount:N0} members to guild {GuildId}.", totalMemberCount, GuildId);
            }
        }

        /// <summary>
        /// Remove members from the guild. Ignores members who do not belong in the guild.
        /// </summary>
        /// <param name="membersToRemove">The members to remove.</param>
        internal void RemoveMembers(IEnumerable<GuildMemberModel> membersToRemove)
        {
            ArgumentNullException.ThrowIfNull(membersToRemove, nameof(membersToRemove));

            int totalMemberCount = 0;
            foreach (GuildMemberModel member in membersToRemove)
            {
                // Skip so that the rest of the members may be removed. We don't need only half of the list removed because of one member.
                if (member.GuildModel.GuildId != GuildId)
                {
                    Logger.LogWarning("RemoveMembersAsync: Member {MemberId} from guild {MemberGuild} was attempted to be removed from guild {GuildId}.", member.UserId, member.GuildModel.GuildId, GuildId);
                    continue;
                }
                // Try removing the member, if it fails, ignore.
                else if (_members.TryRemove(member))
                {
                    totalMemberCount++;
                }
            }

            Logger.LogDebug("Added {MemberCount:N0} members to guild {GuildId}.", totalMemberCount, GuildId);
        }
    }
}
