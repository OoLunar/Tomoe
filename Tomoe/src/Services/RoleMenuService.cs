using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using OoLunar.Tomoe.Database;
using OoLunar.Tomoe.Database.Models;

namespace OoLunar.Tomoe.Services
{
    public sealed class RoleMenuService
    {
        private readonly MemoryCache _roleMenuCache;
        private readonly DatabaseContext _databaseContext;
        private readonly ILogger<RoleMenuService> _logger;

        public RoleMenuService(DatabaseContext databaseContext, ILogger<RoleMenuService> logger)
        {
            _logger = logger;
            _databaseContext = databaseContext;
            _roleMenuCache = new(new MemoryCacheOptions() { ExpirationScanFrequency = TimeSpan.FromMinutes(2) });
        }

        public RoleMenuModel AddRoleMenu(ulong guildId, params ulong[] roleIds)
        {
            RoleMenuModel roleMenuModel = new(guildId, Guid.NewGuid(), roleIds);
            _databaseContext.RoleMenus.Add(roleMenuModel);
            _databaseContext.SaveChanges();
            _roleMenuCache.Set(roleMenuModel.Id, roleMenuModel);
            _logger.LogDebug("Added role menu {MenuId} to guild {GuildId}.", roleMenuModel.Id, guildId);
            return roleMenuModel;
        }

        public RoleMenuModel? GetRoleMenu(Guid menuId)
        {
            if (_roleMenuCache.TryGetValue(menuId, out RoleMenuModel? roleMenuModel))
            {
                return roleMenuModel;
            }

            roleMenuModel = _databaseContext.RoleMenus.Find(menuId);
            if (roleMenuModel != null)
            {
                _roleMenuCache.Set(menuId, roleMenuModel);
            }

            return roleMenuModel;
        }

        public RoleMenuModel UpdateRoleMenu(Guid menuId, IEnumerable<ulong> roles)
        {
            RoleMenuModel? roleMenu = GetRoleMenu(menuId);
            if (roleMenu == null)
            {
                _logger.LogError("Could not find role menu {MenuId} to update.", menuId);
                throw new ArgumentException($"Role menu {menuId} does not exist.", nameof(menuId));
            }

            roleMenu.RoleIds.Clear();
            roleMenu.RoleIds.AddRange(roles);
            if (roleMenu.RoleIds.Count > 24)
            {
                _logger.LogError("Could not update role menu {MenuId} because it has more than 24 roles.", menuId);
                throw new ArgumentException($"Role menu {menuId} has more than 24 roles.", nameof(menuId));
            }

            _databaseContext.SaveChanges();
            _roleMenuCache.Set(menuId, roleMenu);
            _logger.LogDebug("Updated role menu {MenuId}.", menuId);
            return roleMenu;
        }

        public bool TryRemoveRoleMenu(Guid menuId, [NotNullWhen(true)] out RoleMenuModel? removedRoleMenuModel)
        {
            removedRoleMenuModel = GetRoleMenu(menuId);
            if (removedRoleMenuModel is null)
            {
                _logger.LogWarning("Failed to remove role menu {MenuId} because it does not exist.", menuId);
                return false;
            }

            _databaseContext.RoleMenus.Remove(removedRoleMenuModel);
            _databaseContext.SaveChanges();
            _roleMenuCache.Remove(menuId);
            _logger.LogDebug("Removed role menu {MenuId} from guild {GuildId}.", menuId, removedRoleMenuModel.GuildId);
            return true;
        }
    }
}
