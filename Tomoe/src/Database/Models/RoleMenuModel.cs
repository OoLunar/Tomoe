using System;
using System.Collections.Generic;
using System.Linq;

namespace OoLunar.Tomoe.Database.Models
{
    public sealed class RoleMenuModel
    {
        public Guid Id { get; init; }
        public ulong GuildId { get; init; }
        public List<ulong> RoleIds { get; init; }

        public RoleMenuModel() { }
        public RoleMenuModel(ulong guildId, Guid menuId, params ulong[] roleIds)
        {
            GuildId = guildId;
            Id = menuId;
            RoleIds = roleIds.ToList();
        }
    }
}
