using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;
using System.Reflection;
using Microsoft.Extensions.Logging;
using WordMaster.Application.Attributes;
using WordMaster.Application.Persistence;
using WordMaster.Application.Persistence.Repositories;
using WordMaster.Application.Requests.Permission;
using WordMaster.Application.Responses.Permission;
using WordMaster.Application.Services.Abstract;
using WordMaster.Domain.Entities;
using WordMaster.Domain.Results;

namespace WordMaster.Infrastructure.Services;

public class PermissionService(IPermissionRepository permissionRepository, IUnitOfWork unitOfWork, ILogger<PermissionService> logger) : IPermissionService
{
    /// <summary>
    /// Checks if a user has a specific permission.
    /// </summary>
    /// <param name="userId">The ID of the user.</param>
    /// <param name="permissionKey">The unique key of the permission.</param>
    /// <returns>True if the user has the permission, otherwise false.</returns>
    public async Task<ServiceResult<bool>> HasPermissionAsync(Guid userId, string permissionKey)
    {
        bool hasPermission = await permissionRepository.HasPermissionAsync(userId, permissionKey);

        return ServiceResult<bool>.Success(hasPermission, HttpStatusCode.OK);
    }

    /// <summary>
    /// Retrieves all permissions defined in the system.
    /// </summary>
    /// <returns>A list of all permissions.</returns>
    public async Task<ServiceResult<List<PermissionResponse>>> GetAllPermissionsAsync()
    {
        List<Permission> permissions = await permissionRepository.GetAll().ToListAsync();

        var response = permissions.Select(p => new PermissionResponse
        {
            Id = p.Id,
            Key = p.Key,
            Description = p.Description ?? string.Empty,
            AreaName = p.AreaName,
            ControllerName = p.ControllerName,
            ActionName = p.ActionName,
            HttpMethod = p.HttpMethod
        }).ToList();

        return ServiceResult<List<PermissionResponse>>.Success(response, HttpStatusCode.OK);
    }

    /// <summary>
    /// Retrieves permissions assigned to a specific role.
    /// </summary>
    /// <param name="roleId">The ID of the role.</param>
    /// <returns>A list of permissions assigned to the role.</returns>
    public async Task<ServiceResult<List<PermissionResponse>>> GetPermissionsByRoleIdAsync(Guid roleId)
    {
        List<Permission> permissions = await permissionRepository.GetPermissionsByRoleIdAsync(roleId);

        var response = permissions.Select(p => new PermissionResponse
        {
            Id = p.Id,
            Key = p.Key,
            Description = p.Description ?? string.Empty,
            AreaName = p.AreaName,
            ControllerName = p.ControllerName,
            ActionName = p.ActionName,
            HttpMethod = p.HttpMethod
        }).ToList();

        return ServiceResult<List<PermissionResponse>>.Success(response, HttpStatusCode.OK);
    }

    /// <summary>
    /// Updates the permissions assigned to a role.
    /// </summary>
    /// <param name="request">The request containing the role ID and the list of permission IDs.</param>
    /// <param name="operatorId">The ID of the user performing the update.</param>
    /// <returns>A result indicating success or failure.</returns>
    public async Task<ServiceResult> UpdateRolePermissionsAsync(UpdateRolePermissionsRequest request, Guid operatorId)
    {
        bool roleExists = await permissionRepository.RoleExistsAsync(request.RoleId);
        if (!roleExists)
        {
            logger.LogWarning("Role {RoleId} not found during permission update by User {OperatorId}.", request.RoleId, operatorId);
            return ServiceResult.Failure("Role not found", HttpStatusCode.NotFound);
        }

        List<RolePermission> existingPermissions = await permissionRepository.GetRolePermissionsAsync(request.RoleId);

        permissionRepository.RemoveRolePermissions(existingPermissions);

        IEnumerable<RolePermission> newPermissions = request.PermissionIds.Select(pId => new RolePermission
        {
            RoleId = request.RoleId,
            PermissionId = pId
        });

        await permissionRepository.AddRolePermissionsAsync(newPermissions);
        await unitOfWork.CommitAsync();

        logger.LogInformation("Permissions updated successfully. Operator: {OperatorId}, Role: {RoleId}, New Permission IDs: {PermissionIds}",
            operatorId, request.RoleId, string.Join(", ", request.PermissionIds));

        return ServiceResult.Success(HttpStatusCode.OK);
    }

    /// <summary>
    /// Scans the application for permissions and synchronizes them with the database.
    /// Adds new permissions found in code and removes permissions that no longer exist in code.
    /// </summary>
    /// <returns>A result indicating success or failure, including counts of added and deleted permissions.</returns>
    public async Task<ServiceResult<PermissionScanResponse>> ScanAndSavePermissionsAsync()
    {
        logger.LogInformation("Starting permission scan and synchronization...");

        var assembly = Assembly.GetEntryAssembly(); // Get the API assembly
        if (assembly == null)
        {
            logger.LogError("Entry assembly not found during permission scan.");
            return ServiceResult<PermissionScanResponse>.Failure("Entry assembly not found", HttpStatusCode.InternalServerError);
        }

        IEnumerable<Type> controllers = assembly.GetTypes()
            .Where(t => typeof(ControllerBase).IsAssignableFrom(t) && !t.IsAbstract);

        var codePermissions = new List<Permission>();

        foreach (Type? controller in controllers)
        {
            MethodInfo[] methods = controller.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
            foreach (MethodInfo method in methods)
            {
                RequirePermissionAttribute? attribute = method.GetCustomAttribute<RequirePermissionAttribute>();
                if (attribute != null)
                {
                    string key = $"{attribute.AreaName}_{attribute.ControllerName}_{attribute.ActionName}_{attribute.HttpMethod}";

                    if (!codePermissions.Any(p => p.Key == key))
                    {
                        codePermissions.Add(new Permission
                        {
                            Id = Guid.NewGuid(),
                            Key = key,
                            Description = attribute.Description,
                            AreaName = attribute.AreaName,
                            ControllerName = attribute.ControllerName,
                            ActionName = attribute.ActionName,
                            HttpMethod = attribute.HttpMethod
                        });
                    }
                }
            }
        }

        var dbPermissions = await permissionRepository.GetAll().ToListAsync();

        // 1. Permissions to ADD (In Code but not in DB)
        var permissionsToAdd = codePermissions
            .Where(cp => !dbPermissions.Any(dp => dp.Key == cp.Key))
            .ToList();

        // 2. Permissions to DELETE (In DB but not in Code)
        var permissionsToDelete = dbPermissions
            .Where(dp => !codePermissions.Any(cp => cp.Key == dp.Key))
            .ToList();

        if (permissionsToAdd.Count != 0)
        {
            await permissionRepository.AddPermissionsAsync(permissionsToAdd);
        }

        if (permissionsToDelete.Count != 0)
        {
            permissionRepository.RemoveRange(permissionsToDelete);
        }

        if (permissionsToAdd.Count != 0 || permissionsToDelete.Count != 0)
        {
            await unitOfWork.CommitAsync();
        }

        logger.LogInformation("Permission scan completed. Added: {AddedCount}, Deleted: {DeletedCount}", permissionsToAdd.Count, permissionsToDelete.Count);

        var response = new PermissionScanResponse
        {
            AddedCount = permissionsToAdd.Count,
            DeletedCount = permissionsToDelete.Count
        };

        return ServiceResult<PermissionScanResponse>.Success(response, HttpStatusCode.OK);
    }
}
