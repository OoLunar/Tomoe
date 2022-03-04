using System;
using System.ComponentModel.DataAnnotations;
using Tomoe.Interfaces;

namespace Tomoe.Models
{
    public class TempRoleModel : IExpires<Guid>
    {
        [Key]
        public Guid Id { get; init; }
        public ulong GuildId { get; init; }
        public ulong RoleId { get; init; }
        public ulong Assignee { get; init; }
        public ulong Assigner { get; init; }
        public DateTime ExpiresAt { get; internal set; }
    }
}