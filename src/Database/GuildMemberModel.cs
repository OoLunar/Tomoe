using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EdgeDB;
using Microsoft.Extensions.Logging;
using OoLunar.Tomoe.Interfaces;

namespace OoLunar.Tomoe.Database
{
    /// <summary>
    /// A database representation of a Discord member inside of a Discord guild.
    /// </summary>
    [EdgeDBType("GuildMember")]
    public sealed class GuildMemberModel : ICopyable<GuildMemberModel>
    {
        /// <summary>
        /// The guild that the member is linked to.
        /// </summary>
        public GuildModel GuildModel { get; private init; } = null!;

        /// <summary>
        /// The user's id.
        /// </summary>
        public ulong UserId { get; private init; }

        /// <summary>
        /// When the member first joined the guild.
        /// </summary>
        public DateTimeOffset JoinedAt { get; private init; }

        /// <summary>
        /// Whether the member is still in the guild.
        /// </summary>
        public bool Disabled { get; private set; }

        /// <summary>
        /// A list of role ids the user currently has.
        /// </summary>
        public IReadOnlyList<ulong> Roles => new ReadOnlyCollection<ulong>(_roles);
        private List<ulong> _roles { get; init; } = new();

        private EdgeDBClient EdgeDBClient { get; init; } = null!;
        private ILogger<GuildMemberModel> Logger { get; init; } = null!;

        [EdgeDBDeserializer]
        private GuildMemberModel(IDictionary<string, object?> raw)
        {
            GuildModel = (GuildModel)raw["guild"]!;
            UserId = (ulong)raw["user_id"]!;
            Disabled = (bool)raw["disabled"]!;
            _roles = ((List<object>)raw["roles"]!).Cast<ulong>().ToList();
        }

        internal GuildMemberModel(EdgeDBClient edgeDBClient, ILogger<GuildMemberModel> logger, GuildModel guildModel, ulong userId, IEnumerable<ulong> roles)
        {
            ArgumentNullException.ThrowIfNull(edgeDBClient);
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentNullException.ThrowIfNull(guildModel);
            ArgumentNullException.ThrowIfNull(roles, nameof(roles));

            EdgeDBClient = edgeDBClient;
            Logger = logger;

            GuildModel = guildModel;
            UserId = userId;
            _roles = roles.ToList();
        }

        internal async Task<GuildMemberModel> SetRolesAsync(IEnumerable<ulong> roleIds, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(roleIds, nameof(roleIds));

            return Copy((await EdgeDBClient.QueryAsync<GuildMemberModel>(@"
                UPDATE GuildMember
                FILTER .guild = (SELECT Guild FILTER .id = <bigint>$guildId) AND .user_id = <bigint>$userId
                SET {
                    roles := <array<bigint>>$roleIds
                }",
                new Dictionary<string, object?>()
                {
                    ["guildId"] = GuildModel.Id,
                    ["userId"] = UserId,
                    ["roleIds"] = roleIds.Select(roleId => roleId)
                }, Capabilities.Modifications, cancellationToken))
                .FirstOrDefault() ?? throw new InvalidOperationException($"User {UserId} not found in guild {GuildModel.Id}")
            );
        }

        internal async Task<GuildMemberModel> DisableAsync(bool isDisabled, CancellationToken cancellationToken = default) => Copy((await EdgeDBClient.QueryAsync<GuildMemberModel>(@"
            UPDATE GuildMember
            FILTER .guild = (SELECT Guild FILTER .id = <bigint>$guildId) AND .user_id = <bigint>$userId
            SET {
                disabled := <bool>$disabled
            }",
            new Dictionary<string, object?>
            {
                ["guildId"] = GuildModel.Id,
                ["userId"] = UserId,
                ["disabled"] = isDisabled,
            }, Capabilities.Modifications, cancellationToken))
            .FirstOrDefault() ?? throw new InvalidOperationException($"User {UserId} not found in guild {GuildModel.Id}")
        );

        public GuildMemberModel Copy(GuildMemberModel old)
        {
            ArgumentNullException.ThrowIfNull(old);

            Disabled = old.Disabled;
            _roles.Clear();
            _roles.AddRange(old._roles);

            return this;
        }
    }
}
