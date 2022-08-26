using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DSharpPlus.Entities;
using EdgeDB;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OoLunar.Tomoe.Interfaces;

namespace OoLunar.Tomoe.Database
{
    /// <summary>
    /// A database representation of a Discord guild.
    /// </summary>
    [EdgeDBType("Guild")]
    public sealed class GuildModel : ICopy<GuildModel>
    {
        /// <summary>
        /// The guild id.
        /// </summary>
        [EdgeDBProperty("guild_id")]
        public ulong Id { get; private init; }

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
        public IReadOnlyList<GuildPrefixModel> Prefixes => new ReadOnlyCollection<GuildPrefixModel>(_prefixes);
        private List<GuildPrefixModel> _prefixes { get; init; } = new();

        /// <summary>
        /// A read-only list of the members in the guild.
        /// </summary>
        [EdgeDBIgnore]
        public IReadOnlyList<GuildMemberModel> Members => new ReadOnlyCollection<GuildMemberModel>(_members);
        private List<GuildMemberModel> _members { get; init; } = new();

        private EdgeDBClient EdgeDBClient { get; init; } = null!;
        private ILogger<GuildModel> Logger { get; init; } = null!;

        //[EdgeDBDeserializer]
        //private GuildModel(IDictionary<string, object?> raw)
        //{
        //    Id = (ulong)raw["id"]!;
        //    Disabled = (bool)raw["disabled"]!;
        //    KeepRoles = (bool)raw["keep_roles"]!;
        //    TryDmReminders = (bool)raw["try_dm_reminders"]!;
        //    _prefixes = ((List<object>)raw["prefixes"]!).Cast<GuildPrefixModel>().ToList();
        //    _members = ((List<object>)raw["members"]!).Cast<GuildMemberModel>().ToList();
        //}

        internal GuildModel(EdgeDBClient edgeDBClient, ILogger<GuildModel> logger, IConfiguration configuration, ulong id)
        {
            ArgumentNullException.ThrowIfNull(edgeDBClient, nameof(edgeDBClient));
            ArgumentNullException.ThrowIfNull(logger, nameof(logger));
            ArgumentNullException.ThrowIfNull(configuration, nameof(configuration));

            EdgeDBClient = edgeDBClient;
            Logger = logger;

            Id = id;
            _prefixes = configuration.GetSection("discord:prefixes")
                .Get<IEnumerable<string>>()
                .Select(prefix => new GuildPrefixModel(prefix, id))
                .ToList();
        }

        internal async Task<GuildModel> SetPrefixesAsync(IEnumerable<GuildPrefixModel> prefixes, CancellationToken cancellationToken = default) => Copy((await EdgeDBClient.QueryAsync<GuildModel>(
            "UPDATE GuildModel SET { prefixes := <array<GuildPrefixModel>>$prefixes } FILTER .id = <bigint>$id",
            new Dictionary<string, object?>
            {
                { "id", Id },
                { "prefixes", prefixes }
            }, Capabilities.Modifications, cancellationToken))
            .FirstOrDefault() ?? throw new InvalidOperationException($"Guild {Id} not found in the database.")
        );

        internal async Task<GuildModel> AddMembersAsync(IEnumerable<DiscordMember> newMembers, CancellationToken cancellationToken = default)
        {
            if (newMembers == null)
            {
                throw new ArgumentNullException(nameof(newMembers));
            }
            else if (!newMembers.Any())
            {
                Logger.LogWarning("AddMembersAsync: No members were provided.");
                return this;
            }
            else if (newMembers.Any(member => member.Guild.Id != Id))
            {
                throw new ArgumentException("All members must be in the same guild.", nameof(newMembers));
            }

            return Copy((await EdgeDBClient.QueryAsync<GuildModel>(@"
                WITH
                    member_guild := SELECT Guild FILTER .id = $guildId,
                    memberData := <json>$memberData
                FOR member in json_array_unpack(memberData) union (
                    INSERT GuildMember {
                        guild := member_guild,
                        user_id := <bigint>member['user_id'],
                        roles := <array<bigint>>member['role_ids']
                    }
                    UNLESS CONFLICT ON .user_id
                    ELSE (
                        UPDATE GuildMember
                        FILTER .user_id = <bigint>member['user_id'] AND .guild = member_guild
                        SET {
                            roles := <array<bigint>>member['role_ids'],
                        }
                    )
                )",
                new Dictionary<string, object?>()
                {
                    ["guildId"] = Id,
                    ["memberData"] = newMembers.Select(member => new Dictionary<string, object>()
                    {
                        ["user_id"] = member.Id,
                        ["role_ids"] = member.Roles.Select(role => role.Id)
                    })
                }, Capabilities.Modifications, cancellationToken))
                .FirstOrDefault() ?? throw new InvalidOperationException($"Guild {Id} not found in the database.")
            );
        }

        internal async Task<GuildModel> RemoveMembersAsync(IEnumerable<ulong> memberIds, CancellationToken cancellationToken = default)
        {
            if (memberIds == null)
            {
                throw new ArgumentNullException(nameof(memberIds));
            }
            else if (!memberIds.Any())
            {
                Logger.LogWarning("RemoveMembersAsync: No members were provided.");
                return this;
            }

            return Copy((await EdgeDBClient.QueryAsync<GuildModel>(@"
                WITH
                    member_guild := SELECT Guild FILTER .id = $guildId,
                    member_ids := <array<bigint>>$memberIds
                FOR member_id in member_ids union (
                    DELETE GuildMember
                    FILTER .user_id = member_id AND .guild = member_guild
                )",
                new Dictionary<string, object?>()
                {
                    ["guildId"] = Id,
                    ["memberIds"] = memberIds
                }, Capabilities.Modifications, cancellationToken))
                .FirstOrDefault() ?? throw new InvalidOperationException($"Guild {Id} not found in the database.")
            );
        }

        public GuildModel Copy(GuildModel old)
        {
            ArgumentNullException.ThrowIfNull(old, nameof(old));

            Disabled = old.Disabled;
            KeepRoles = old.KeepRoles;
            TryDmReminders = old.TryDmReminders;
            _prefixes.Clear();
            _prefixes.AddRange(old._prefixes);
            _members.Clear();
            _members.AddRange(old._members);

            return this;
        }
    }
}
