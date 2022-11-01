using System;
using System.ComponentModel.DataAnnotations;

namespace Tomoe.Db
{
    public class TempRoleModel
    {
        [Key]
        public Guid Id { get; init; }
        public ulong GuildId { get; init; }
        public ulong RoleId { get; init; }
        public ulong Assignee { get; init; }
        public ulong Assigner { get; init; }
        public DateTime ExpiresAt { get; init; }

        public TempRoleModel() { }

        public TempRoleModel(ulong guildId, ulong roleId, ulong assignee, ulong assigner, DateTime expiresAt)
        {
            GuildId = guildId;
            RoleId = roleId;
            Assignee = assignee;
            Assigner = assigner;
            ExpiresAt = expiresAt;
        }
    }
}
