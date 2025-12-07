using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Net;
using WordMaster.Application.Requests.Role;
using WordMaster.Application.Responses.Role;
using WordMaster.Application.Services.Abstract;
using WordMaster.Domain.Entities;
using WordMaster.Domain.Results;

namespace WordMaster.Infrastructure.Services;

public class RoleService(RoleManager<AppRole> roleManager, UserManager<AppUser> userManager, ICacheService cacheService) : IRoleService
{
    private const string RolesCacheKey = "roles:list";
    private static readonly TimeSpan RolesCacheExpiration = TimeSpan.FromMinutes(10);

    public async Task<ServiceResult<List<RoleResponse>>> GetRoleListAsync()
    {
        List<RoleResponse>? cachedRoles = await cacheService.GetAsync<List<RoleResponse>>(RolesCacheKey);
        if (cachedRoles != null)
        {
            return ServiceResult<List<RoleResponse>>.Success(cachedRoles, HttpStatusCode.OK);
        }

        List<AppRole> roles = await roleManager.Roles.AsNoTracking().ToListAsync();
        List<RoleResponse> roleViewModel = roles.Select(x => new RoleResponse() { Id = x.Id.ToString(), Name = x.Name! }).ToList();

        await cacheService.SetAsync(RolesCacheKey, roleViewModel, RolesCacheExpiration);
        return ServiceResult<List<RoleResponse>>.Success(roleViewModel, HttpStatusCode.OK);
    }
    public async Task<ServiceResult> CreateRoleAsync(RoleCreateRequest request)
    {
        IdentityResult result = await roleManager.CreateAsync(new AppRole() { Name = request.Name });

        if (result.Succeeded)
        {
            await cacheService.RemoveAsync(RolesCacheKey);
            return ServiceResult.SuccessAsCreated();
        }

        List<string> errors = result.Errors.Select(e => e.Description).ToList();
        return ServiceResult.Failure(errors, HttpStatusCode.BadRequest);
    }
    public async Task<ServiceResult<RoleUpdateResponse>> FindByIdReturnRoleUpdateViewModelAsync(string id)
    {
        AppRole? role = await roleManager.FindByIdAsync(id);
        if (role == null)
        {
            return ServiceResult<RoleUpdateResponse>.Failure("Rol bulunamadı.", HttpStatusCode.NotFound);
        }
        RoleUpdateResponse roleUpdateViewModel = new() { Id = role.Id.ToString(), Name = role.Name! };
        return ServiceResult<RoleUpdateResponse>.Success(roleUpdateViewModel, HttpStatusCode.OK);
    }
    public async Task<ServiceResult> UpdateRoleAsync(RoleUpdateRequest request)
    {
        AppRole? role = await roleManager.FindByIdAsync(request.Id);
        if (role == null)
        {
            return ServiceResult.Failure("Rol bulunamadı.", HttpStatusCode.NotFound);
        }
        role.Name = request.Name;
        IdentityResult result = await roleManager.UpdateAsync(role);

        if (result.Succeeded)
        {
            await cacheService.RemoveAsync(RolesCacheKey);
            return ServiceResult.Success(HttpStatusCode.NoContent);
        }

        List<string> errors = result.Errors.Select(e => e.Description).ToList();
        return ServiceResult.Failure(errors, HttpStatusCode.BadRequest);
    }
    public async Task<ServiceResult> DeleteRoleAsync(string id)
    {
        AppRole? role = await roleManager.FindByIdAsync(id);
        if (role == null)
        {
            return ServiceResult.Failure("Rol bulunamadı.", HttpStatusCode.NotFound);
        }

        IdentityResult result = await roleManager.DeleteAsync(role);

        if (result.Succeeded)
        {
            await cacheService.RemoveAsync(RolesCacheKey);
            return ServiceResult.Success(HttpStatusCode.NoContent);
        }

        List<string> errors = result.Errors.Select(e => e.Description).ToList();
        return ServiceResult.Failure(errors, HttpStatusCode.BadRequest);
    }
    public async Task<ServiceResult<List<AssignToRoleResponse>>> GetRoleByIdReturnAssignToRoleAsync(string id)
    {
        AppUser? user = await userManager.FindByIdAsync(id);
        if (user == null)
        {
            return ServiceResult<List<AssignToRoleResponse>>.Failure("Kullanıcı bulunamadı.", HttpStatusCode.NotFound);
        }

        List<AppRole> roles = await roleManager.Roles.ToListAsync();

        List<AssignToRoleResponse> roleViewModel = new();

        IList<string> userRoles = await userManager.GetRolesAsync(user);

        foreach (AppRole? role in roles)
        {
            AssignToRoleResponse assignToRoleViewModel = new() { Id = role.Id.ToString(), Name = role.Name! };
            if (userRoles.Contains(role.Name!))
            {
                assignToRoleViewModel.Exist = true;
            }
            roleViewModel.Add(assignToRoleViewModel);
        }
        return ServiceResult<List<AssignToRoleResponse>>.Success(roleViewModel, HttpStatusCode.OK);

    }
    public async Task<ServiceResult> AssignRoleAsync(AssignRolesRequest request)
    {
        AppUser? user = await userManager.FindByIdAsync(request.UserId);
        if (user == null)
        {
            return ServiceResult.Failure("Kullanıcı bulunamadı.", HttpStatusCode.NotFound);
        }

        IList<string> userRoles = await userManager.GetRolesAsync(user);

        foreach (AssignToRoleResponse role in request.Roles)
        {
            bool isInRole = userRoles.Contains(role.Name);

            if (role.Exist && !isInRole)
            {
                await userManager.AddToRoleAsync(user, role.Name);
            }
            else if (!role.Exist && isInRole)
            {
                await userManager.RemoveFromRoleAsync(user, role.Name);
            }
        }
        return ServiceResult.Success(HttpStatusCode.NoContent);
    }
}
