using Microsoft.EntityFrameworkCore;
using WordMaster.Application.Persistence.Repositories;
using WordMaster.Domain.Entities;

namespace WordMaster.Infrastructure.EfCore.Repositories;

public class PermissionRepository(AppDbContext context) : GenericRepository<Permission>(context), IPermissionRepository
{
    public async Task<bool> HasPermissionAsync(Guid userId, string permissionKey)
    {
        return await (from ur in _context.UserRoles
                      join rp in _context.RolePermissions on ur.RoleId equals rp.RoleId
                      join p in _context.Permissions on rp.PermissionId equals p.Id
                      where ur.UserId == userId && p.Key == permissionKey
                      select 1).AnyAsync();
    }

    public async Task<List<Permission>> GetPermissionsByRoleIdAsync(Guid roleId)
    {
        return await (from rp in _context.RolePermissions
                      join p in _context.Permissions on rp.PermissionId equals p.Id
                      where rp.RoleId == roleId
                      select p).ToListAsync();
    }

    public async Task<bool> RoleExistsAsync(Guid roleId)
    {
        return await _context.Roles.AnyAsync(r => r.Id == roleId);
    }

    public async Task<List<RolePermission>> GetRolePermissionsAsync(Guid roleId)
    {
        return await _context.RolePermissions
            .Where(rp => rp.RoleId == roleId)
            .ToListAsync();
    }

    public void RemoveRolePermissions(IEnumerable<RolePermission> rolePermissions)
    {
        _context.RolePermissions.RemoveRange(rolePermissions);
    }

    public async Task AddRolePermissionsAsync(IEnumerable<RolePermission> rolePermissions)
    {
        await _context.RolePermissions.AddRangeAsync(rolePermissions);
    }

    public async Task<bool> PermissionExistsAsync(string key)
    {
        return await _context.Permissions.AnyAsync(p => p.Key == key);
    }

    public async Task AddPermissionsAsync(IEnumerable<Permission> permissions)
    {
        await _context.Permissions.AddRangeAsync(permissions);
    }
}
