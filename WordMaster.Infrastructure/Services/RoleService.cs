using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WordMaster.Application.Requests;
using WordMaster.Application.Services.Abstract;
using WordMaster.Application.ViewModels;
using WordMaster.Domain.Entities;
using WordMaster.Domain.Results;

namespace WordMaster.Infrastructure.Services;

public class RoleService(RoleManager<AppRole> roleManager, UserManager<AppUser> userManager, ICacheService cacheService) : IRoleService
{
    private const string RolesCacheKey = "roles:list";
    private static readonly TimeSpan RolesCacheExpiration = TimeSpan.FromMinutes(10);

    public async Task<List<RoleViewModel>> GetRoleListAsync()
    {
        var cachedRoles = await cacheService.GetAsync<List<RoleViewModel>>(RolesCacheKey);
        if (cachedRoles != null)
        {
            return cachedRoles;
        }

        var roles = await roleManager.Roles.AsNoTracking().ToListAsync();
        var roleViewModel = roles.Select(x => new RoleViewModel() { Id = x.Id, Name = x.Name! }).ToList();

        await cacheService.SetAsync(RolesCacheKey, roleViewModel, RolesCacheExpiration);
        return roleViewModel;
    }
    public async Task<Result<IEnumerable<IdentityError>>> CreateRoleAsync(RoleCreateRequest request)
    {
        var result = await roleManager.CreateAsync(new AppRole() { Name = request.Name });

        if (result.Succeeded)
        {
            await cacheService.RemoveAsync(RolesCacheKey);
        }

        return !result.Succeeded
            ? new Result<IEnumerable<IdentityError>> { IsSuccess = false, ErrorMessage = "Rol oluşturulamadı.", Data = result.Errors }
            : Result<IEnumerable<IdentityError>>.Success(null);
    }
    public async Task<Result<RoleUpdateViewModel>> FindByIdReturnRoleUpdateViewModelAsync(string id)
    {
        var role = await roleManager.FindByIdAsync(id);
        if (role == null)
        {
            return Result<RoleUpdateViewModel>.Failure("Rol bulunamadı.");
        }
        var roleUpdateViewModel = new RoleUpdateViewModel() { Id = role.Id, Name = role.Name! };
        return Result<RoleUpdateViewModel>.Success(roleUpdateViewModel);
    }

    public async Task<Result<IEnumerable<IdentityError>>> UpdateRoleAsync(RoleUpdateRequest request)
    {
        var role = await roleManager.FindByIdAsync(request.Id);
        if (role == null)
        {
            return new Result<IEnumerable<IdentityError>> { IsSuccess = false, ErrorMessage = "Rol bulunamadı.", Data = null };
        }
        role.Name = request.Name;
        var result = await roleManager.UpdateAsync(role);

        if (result.Succeeded)
        {
            await cacheService.RemoveAsync(RolesCacheKey);
        }

        return !result.Succeeded
            ? new Result<IEnumerable<IdentityError>> { IsSuccess = false, ErrorMessage = "Rol güncellenemedi.", Data = result.Errors }
            : Result<IEnumerable<IdentityError>>.Success(null);
    }
    public async Task<Result<IEnumerable<IdentityError>>> DeleteRoleAsync(string id)
    {
        var role = await roleManager.FindByIdAsync(id);
        if (role == null)
        {
            return new Result<IEnumerable<IdentityError>> { IsSuccess = false, ErrorMessage = "Rol bulunamadı.", Data = null };
        }

        var result = await roleManager.DeleteAsync(role);

        if (result.Succeeded)
        {
            await cacheService.RemoveAsync(RolesCacheKey);
        }

        return !result.Succeeded
            ? new Result<IEnumerable<IdentityError>> { IsSuccess = false, ErrorMessage = "Rol silinemedi.", Data = result.Errors }
            : Result<IEnumerable<IdentityError>>.Success(null);
    }

    public async Task<List<AssignToRoleViewModel>> GetRoleByIdReturnAssignToRoleAsync(string id)
    {
        var user = await userManager.FindByIdAsync(id);
        if (user == null)
        {
            return [];
        }

        var roles = await roleManager.Roles.ToListAsync();

        var roleViewModel = new List<AssignToRoleViewModel>();

        var userRoles = await userManager.GetRolesAsync(user);

        foreach (var role in roles)
        {
            var assignToRoleViewModel = new AssignToRoleViewModel() { Id = role.Id, Name = role.Name! };
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
        var user = await userManager.FindByIdAsync(id);
        if (user == null)
        {
            return;
        }

        var userRoles = await userManager.GetRolesAsync(user);

        foreach (var role in request)
        {
            var isInRole = userRoles.Contains(role.Name);

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
