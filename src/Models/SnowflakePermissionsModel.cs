using System;
using System.ComponentModel.DataAnnotations;
using Tomoe.Enums;

namespace Tomoe.Models
{
    /// <summary>
    /// Tomoe's custom permissions system that applies to both role and user ids.
    /// </summary>
    public class SnowflakePermissionsModel
    {
        [Key]
        public ulong SnowflakeId { get; init; }
        public ulong GuildId { get; init; }
        public GuildPermissions Permissions { get; set; }

        public override bool Equals(object? obj) => obj is SnowflakePermissionsModel perms && SnowflakeId == perms.SnowflakeId && Permissions == perms.Permissions;
        public override int GetHashCode() => HashCode.Combine(SnowflakeId, Permissions);
    }
}