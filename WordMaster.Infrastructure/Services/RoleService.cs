using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WordMaster.Application.Requests;
using WordMaster.Application.Services.Abstract;
using WordMaster.Application.ViewModels;
using WordMaster.Domain.Entities;
using WordMaster.Domain.Results;

namespace WordMaster.Infrastructure.Services;

public class RoleService(RoleManager<AppRole> roleManager, UserManager<AppUser> userManager) : IRoleService
{
    public async Task<List<RoleViewModel>> GetRoleListAsync()
    {
        var roles = await roleManager.Roles.AsNoTracking().ToListAsync();
        var roleViewModel = roles.Select(x => new RoleViewModel() { Id = x.Id, Name = x.Name! }).ToList();
        return roleViewModel;
    }
    public async Task<Result<IEnumerable<IdentityError>>> CreateRoleAsync(RoleCreateRequest request)
    {
        var result = await roleManager.CreateAsync(new AppRole() { Name = request.Name });

        return !result.Succeeded
            ? new Result<IEnumerable<IdentityError>> { IsSuccess = false, ErrorMessage = "Rol olu�turulamad�.", Data = result.Errors }
            : Result<IEnumerable<IdentityError>>.Success(null);
    }
    public async Task<Result<RoleUpdateViewModel>> FindByIdReturnRoleUpdateViewModelAsync(string id)
    {
        var role = await roleManager.FindByIdAsync(id);
        if (role == null)
        {
            return Result<RoleUpdateViewModel>.Failure("Rol bulunamad�.");
        }
        var roleUpdateViewModel = new RoleUpdateViewModel() { Id = role.Id, Name = role.Name! };
        return Result<RoleUpdateViewModel>.Success(roleUpdateViewModel);
    }

    public async Task<Result<IEnumerable<IdentityError>>> UpdateRoleAsync(RoleUpdateRequest request)
    {
        var role = await roleManager.FindByIdAsync(request.Id);
        if (role == null)
        {
            return new Result<IEnumerable<IdentityError>> { IsSuccess = false, ErrorMessage = "Rol bulunamad�.", Data = null };
        }
        role.Name = request.Name;
        var result = await roleManager.UpdateAsync(role);
        return !result.Succeeded
            ? new Result<IEnumerable<IdentityError>> { IsSuccess = false, ErrorMessage = "Rol g�ncellenemedi.", Data = result.Errors }
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

        foreach (var role in request)
        {
            if (role.Exist)
            {
                await userManager.AddToRoleAsync(user, role.Name);
            }
            else await userManager.RemoveFromRoleAsync(user, role.Name);
        }
    }
}
