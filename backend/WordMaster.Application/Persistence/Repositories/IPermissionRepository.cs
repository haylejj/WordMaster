using WordMaster.Domain.Entities;

namespace WordMaster.Application.Persistence.Repositories;

public interface IPermissionRepository : IGenericRepository<Permission>
{
    Task<bool> HasPermissionAsync(Guid userId, string permissionKey);
    Task<List<Permission>> GetPermissionsByRoleIdAsync(Guid roleId);
    Task<bool> RoleExistsAsync(Guid roleId);
    Task<List<RolePermission>> GetRolePermissionsAsync(Guid roleId);
    void RemoveRolePermissions(IEnumerable<RolePermission> rolePermissions);
    Task AddRolePermissionsAsync(IEnumerable<RolePermission> rolePermissions);
    Task AddPermissionsAsync(IEnumerable<Permission> permissions);
}
