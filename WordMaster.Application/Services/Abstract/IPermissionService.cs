using WordMaster.Application.Requests.Permission;
using WordMaster.Application.Responses.Permission;
using WordMaster.Domain.Results;

namespace WordMaster.Application.Services.Abstract;

public interface IPermissionService
{
    /// <summary>
    /// Checks if a user has a specific permission.
    /// </summary>
    /// <param name="userId">The ID of the user.</param>
    /// <param name="permissionKey">The unique key of the permission.</param>
    /// <returns>True if the user has the permission, otherwise false.</returns>
    Task<ServiceResult<bool>> HasPermissionAsync(Guid userId, string permissionKey);

    /// <summary>
    /// Retrieves all permissions defined in the system.
    /// </summary>
    /// <returns>A list of all permissions.</returns>
    Task<ServiceResult<List<PermissionResponse>>> GetAllPermissionsAsync();

    /// <summary>
    /// Retrieves permissions assigned to a specific role.
    /// </summary>
    /// <param name="roleId">The ID of the role.</param>
    /// <returns>A list of permissions assigned to the role.</returns>
    Task<ServiceResult<List<PermissionResponse>>> GetPermissionsByRoleIdAsync(Guid roleId);

    /// <summary>
    /// Updates the permissions assigned to a role.
    /// </summary>
    /// <param name="request">The request containing the role ID and the list of permission IDs.</param>
    /// <returns>A result indicating success or failure.</returns>
    Task<ServiceResult> UpdateRolePermissionsAsync(UpdateRolePermissionsRequest request);

    /// <summary>
    /// Scans the application for permissions and saves them to the database.
    /// </summary>
    /// <returns>A result indicating success or failure.</returns>
    Task<ServiceResult> ScanAndSavePermissionsAsync();
}
