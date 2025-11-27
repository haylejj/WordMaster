using System.Net;
using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WordMaster.Application.Attributes;
using WordMaster.Application.Persistence;
using WordMaster.Application.Persistence.Repositories;
using WordMaster.Application.Requests.Permission;
using WordMaster.Application.Responses.Permission;
using WordMaster.Application.Services.Abstract;
using WordMaster.Domain.Entities;
using WordMaster.Domain.Results;

namespace WordMaster.Infrastructure.Services;

public class PermissionService(IPermissionRepository permissionRepository, IUnitOfWork unitOfWork) : IPermissionService
{
    /// <summary>
    /// Checks if a user has a specific permission.
    /// </summary>
    /// <param name="userId">The ID of the user.</param>
    /// <param name="permissionKey">The unique key of the permission.</param>
    /// <returns>True if the user has the permission, otherwise false.</returns>
    public async Task<ServiceResult<bool>> HasPermissionAsync(Guid userId, string permissionKey)
    {
        var hasPermission = await permissionRepository.HasPermissionAsync(userId, permissionKey);

        return ServiceResult<bool>.Success(hasPermission, HttpStatusCode.OK);
    }

    /// <summary>
    /// Retrieves all permissions defined in the system.
    /// </summary>
    /// <returns>A list of all permissions.</returns>
    public async Task<ServiceResult<List<PermissionResponse>>> GetAllPermissionsAsync()
    {
        var permissions = await permissionRepository.GetAll().ToListAsync();

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
        var permissions = await permissionRepository.GetPermissionsByRoleIdAsync(roleId);

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
    /// <returns>A result indicating success or failure.</returns>
    public async Task<ServiceResult> UpdateRolePermissionsAsync(UpdateRolePermissionsRequest request)
    {
        var roleExists = await permissionRepository.RoleExistsAsync(request.RoleId);
        if (!roleExists)
        {
            return ServiceResult.Failure("Role not found", HttpStatusCode.NotFound);
        }

        var existingPermissions = await permissionRepository.GetRolePermissionsAsync(request.RoleId);

        permissionRepository.RemoveRolePermissions(existingPermissions);

        IEnumerable<RolePermission> newPermissions = request.PermissionIds.Select(pId => new RolePermission
        {
            RoleId = request.RoleId,
            PermissionId = pId
        });

        await permissionRepository.AddRolePermissionsAsync(newPermissions);
        await unitOfWork.CommitAsync();

        return ServiceResult.Success(HttpStatusCode.OK);
    }

    /// <summary>
    /// Scans the application for permissions and saves them to the database.
    /// </summary>
    /// <returns>A result indicating success or failure.</returns>
    public async Task<ServiceResult> ScanAndSavePermissionsAsync()
    {
        var assembly = Assembly.GetEntryAssembly(); // Get the API assembly
        if (assembly == null)
        {
            return ServiceResult.Failure("Entry assembly not found", HttpStatusCode.InternalServerError);
        }

        var controllers = assembly.GetTypes()
            .Where(t => typeof(ControllerBase).IsAssignableFrom(t) && !t.IsAbstract);

        var permissionsToAdd = new List<Permission>();

        foreach (var controller in controllers)
        {
            var methods = controller.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
            foreach (var method in methods)
            {
                var attribute = method.GetCustomAttribute<RequirePermissionAttribute>();
                if (attribute != null)
                {
                    var key = $"{attribute.AreaName}_{attribute.ControllerName}_{attribute.ActionName}_{attribute.HttpMethod}";

                    // Check if permission already exists in DB
                    var exists = await permissionRepository.PermissionExistsAsync(key);
                    if (!exists && !permissionsToAdd.Any(p => p.Key == key))
                    {
                        permissionsToAdd.Add(new Permission
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

        if (permissionsToAdd.Any())
        {
            await permissionRepository.AddPermissionsAsync(permissionsToAdd);
            await unitOfWork.CommitAsync();
        }

        return ServiceResult.Success(HttpStatusCode.OK);
    }
}