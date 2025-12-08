using Microsoft.AspNetCore.Mvc;
using WordMaster.Application.Attributes;
using WordMaster.Application.Requests.Permission;
using WordMaster.Application.Responses.Permission;
using WordMaster.Application.Services.Abstract;
using WordMaster.Domain.Results;

namespace WordMaster.API.Controllers.Admin;

/// <summary>
/// Controller for managing permissions and role assignments.
/// </summary>
[Route("api/admin/permissions")]
public class PermissionsController(IPermissionService permissionService) : BaseController
{
    /// <summary>
    /// Retrieves all permissions.
    /// </summary>
    /// <returns>A list of permissions.</returns>
    [HttpGet]
    [RequirePermission("Admin", "Permissions", "GetAll", "GET", "Tüm yetkileri görüntüle")]
    public async Task<IActionResult> GetAll()
    {
        ServiceResult<List<PermissionResponse>> result = await permissionService.GetAllPermissionsAsync();
        return CreateResult(result);
    }

    /// <summary>
    /// Retrieves permissions for a specific role.
    /// </summary>
    /// <param name="roleId">The ID of the role.</param>
    /// <returns>A list of permissions for the role.</returns>
    [HttpGet("role/{roleId}")]
    [RequirePermission("Admin", "Permissions", "GetByRole", "GET", "Role göre yetkileri görüntüle")]
    public async Task<IActionResult> GetByRole(Guid roleId)
    {
        ServiceResult<List<PermissionResponse>> result = await permissionService.GetPermissionsByRoleIdAsync(roleId);
        return CreateResult(result);
    }

    /// <summary>
    /// Updates permissions for a specific role.
    /// </summary>
    /// <param name="request">The update request.</param>
    /// <returns>Result of the update operation.</returns>
    [HttpPut("role")]
    [RequirePermission("Admin", "Permissions", "UpdateRolePermissions", "PUT", "Rol yetkilerini güncelle")]
    public async Task<IActionResult> UpdateRolePermissions([FromBody] UpdateRolePermissionsRequest request)
    {
        string? userIdString = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        Guid userId = string.IsNullOrEmpty(userIdString) ? Guid.Empty : Guid.Parse(userIdString);

        ServiceResult result = await permissionService.UpdateRolePermissionsAsync(request, userId);
        return CreateResult(result);
    }

    /// <summary>
    /// Scans the application for permissions and updates the database.
    /// </summary>
    /// <returns>Result of the scan operation.</returns>
    [HttpPost("scan")]
    [RequirePermission("Admin", "Permissions", "Scan", "POST", "Yeni yetkileri tara")]
    public async Task<IActionResult> Scan()
    {
        ServiceResult<PermissionScanResponse> result = await permissionService.ScanAndSavePermissionsAsync();
        return CreateResult(result);
    }
}
