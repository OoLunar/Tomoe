using System;
using System.Collections.Generic;
using System.Linq;
using ConcurrentCollections;
using EdgeDB;
using Microsoft.Extensions.Logging;

namespace OoLunar.Tomoe.Database.Models
{
    /// <summary>
    /// A database representation of a Discord member inside of a Discord guild.
    /// </summary>
    [EdgeDBType("GuildMember")]
    public sealed class GuildMemberModel : DatabaseTrackable<GuildMemberModel>
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
        public IReadOnlyList<ulong> Roles => _roles.ToArray();
        private ConcurrentHashSet<ulong> _roles { get; set; } = new();

        private ILogger<GuildMemberModel> Logger { get; init; } = null!;

        public GuildMemberModel() { }
        internal GuildMemberModel(ILogger<GuildMemberModel> logger, GuildModel guildModel, ulong userId, IEnumerable<ulong> roles)
        {
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentNullException.ThrowIfNull(guildModel);
            ArgumentNullException.ThrowIfNull(roles, nameof(roles));

            Logger = logger;
            GuildModel = guildModel;
            UserId = userId;
            _roles = new ConcurrentHashSet<ulong>(roles);
        }

        /// <summary>
        /// Sets the roles for the member.
        /// </summary>
        /// <param name="roleIds">The role ids that the member contains.</param>
        internal void SetRoles(IEnumerable<ulong> roleIds)
        {
            ArgumentNullException.ThrowIfNull(roleIds, nameof(roleIds));

            Logger.LogTrace("Setting roles for Member {MemberId} (Guild {GuildId}) from {OldRoles} to {NewRoles}", UserId, GuildModel.GuildId, _roles, roleIds);
            _roles = new(roleIds);
        }

        /// <summary>
        /// Used when a member has left or rejoined the guild.
        /// </summary>
        /// <param name="isDisabled">Whether the member has left or rejoined the guild.</param>
        internal void Disable(bool isDisabled)
        {
            Logger.LogTrace("Setting disabled status for Member {MemberId} (Guild {GuildId}) from {OldStatus} to {NewStatus}", UserId, GuildModel.GuildId, Disabled, isDisabled);
            Disabled = isDisabled;
        }
    }
}
