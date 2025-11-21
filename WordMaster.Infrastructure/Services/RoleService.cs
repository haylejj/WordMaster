using System.Net;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WordMaster.Application.Requests.Role;
using WordMaster.Application.Services.Abstract;
using WordMaster.Application.ViewModels.Role;
using WordMaster.Domain.Entities;
using WordMaster.Domain.Results;

namespace WordMaster.Infrastructure.Services;

public class RoleService(RoleManager<AppRole> roleManager, UserManager<AppUser> userManager, ICacheService cacheService) : IRoleService
{
    private const string RolesCacheKey = "roles:list";
    private static readonly TimeSpan RolesCacheExpiration = TimeSpan.FromMinutes(10);

    public async Task<List<RoleViewModel>> GetRoleListAsync()
    {
        List<RoleViewModel>? cachedRoles = await cacheService.GetAsync<List<RoleViewModel>>(RolesCacheKey);
        if (cachedRoles != null)
        {
            return cachedRoles;
        }

        List<AppRole> roles = await roleManager.Roles.AsNoTracking().ToListAsync();
        List<RoleViewModel> roleViewModel = roles.Select(x => new RoleViewModel() { Id = x.Id.ToString(), Name = x.Name! }).ToList();

        await cacheService.SetAsync(RolesCacheKey, roleViewModel, RolesCacheExpiration);
        return roleViewModel;
    }

    public async Task<ServiceResult> CreateRoleAsync(RoleCreateRequest request)
    {
        IdentityResult result = await roleManager.CreateAsync(new AppRole() { Name = request.Name });

        if (result.Succeeded)
        {
            await cacheService.RemoveAsync(RolesCacheKey);
            return ServiceResult.SuccessAsCreated();
        }

        var errors = result.Errors.Select(e => e.Description).ToList();
        return ServiceResult.Failure(errors, HttpStatusCode.BadRequest);
    }

    public async Task<ServiceResult<RoleUpdateViewModel>> FindByIdReturnRoleUpdateViewModelAsync(string id)
    {
        AppRole? role = await roleManager.FindByIdAsync(id);
        if (role == null)
        {
            return ServiceResult<RoleUpdateViewModel>.Failure("Rol bulunamadı.", HttpStatusCode.NotFound);
        }
        RoleUpdateViewModel roleUpdateViewModel = new() { Id = role.Id.ToString(), Name = role.Name! };
        return ServiceResult<RoleUpdateViewModel>.Success(roleUpdateViewModel, HttpStatusCode.OK);
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

        var errors = result.Errors.Select(e => e.Description).ToList();
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

        var errors = result.Errors.Select(e => e.Description).ToList();
        return ServiceResult.Failure(errors, HttpStatusCode.BadRequest);
    }

    public async Task<List<AssignToRoleViewModel>> GetRoleByIdReturnAssignToRoleAsync(string id)
    {
        AppUser? user = await userManager.FindByIdAsync(id);
        if (user == null)
        {
            return [];
        }

        List<AppRole> roles = await roleManager.Roles.ToListAsync();

        List<AssignToRoleViewModel> roleViewModel = new();

        IList<string> userRoles = await userManager.GetRolesAsync(user);

        foreach (AppRole? role in roles)
        {
            AssignToRoleViewModel assignToRoleViewModel = new() { Id = role.Id.ToString(), Name = role.Name! };
            if (userRoles.Contains(role.Name!))
            {
                assignToRoleViewModel.Exist = true;
            }
            roleViewModel.Add(assignToRoleViewModel);
        }
        return roleViewModel;

    }
    public async Task AssignRoleAsync(string id, List<AssignToRoleViewModel> request)
    {
        AppUser? user = await userManager.FindByIdAsync(id);
        if (user == null)
        {
            return;
        }

        IList<string> userRoles = await userManager.GetRolesAsync(user);

        foreach (AssignToRoleViewModel role in request)
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
    }
}
