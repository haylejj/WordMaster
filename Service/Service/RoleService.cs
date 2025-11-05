using Core.Entity;
using Core.Requests;
using Core.Service;
using Core.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Service.Service;

public class RoleService(RoleManager<AppRole> roleManager, UserManager<AppUser> userManager) : IRoleService
{
    public async Task<List<RoleViewModel>> GetRoleListAsync()
    {
        var roles = await roleManager.Roles.AsNoTracking().ToListAsync();
        var roleViewModel = roles.Select(x => new RoleViewModel() { Id = x.Id, Name = x.Name }).ToList();
        return roleViewModel;
    }
    public async Task<(bool, IEnumerable<IdentityError>?)> CreateRoleAsync(RoleCreateRequest request)
    {
        var result = await roleManager.CreateAsync(new AppRole() { Name = request.Name });

        if (!result.Succeeded)
        {
            return (false, result.Errors);
        }
        else { return (true, null); }
    }
    public async Task<(bool, RoleUpdateViewModel?)> FindByIdReturnRoleUpdateViewModelAsync(string id)
    {
        var role = await roleManager.FindByIdAsync(id);
        if (role == null)
        {
            return (false, null);
        }
        var roleUpdateViewModel = new RoleUpdateViewModel() { Id = role.Id, Name = role.Name };
        return (true, roleUpdateViewModel);
    }

    public async Task<(bool, IEnumerable<IdentityError>?)> UpdateRoleAsync(RoleUpdateRequest request)
    {
        var role = await roleManager.FindByIdAsync(request.Id);
        if (role == null)
        {
            return (false, null);
        }
        role.Name = request.Name;
        var result = await roleManager.UpdateAsync(role);
        if (!result.Succeeded) { return (false, result.Errors); }

        else { return (true, null); }
    }
    public async Task<(bool, IEnumerable<IdentityError>?)> DeleteRoleAsync(string id)
    {
        var role = await roleManager.FindByIdAsync(id);

        var result = await roleManager.DeleteAsync(role);
        if (!result.Succeeded)
        {
            return (false, result.Errors);

        }
        else { return (true, null); }
    }

    public async Task<List<AssignToRoleViewModel>> GetRoleByIdReturnAssignToRoleAsync(string id)
    {
        var user = await userManager.FindByIdAsync(id);

        var roles = await roleManager.Roles.ToListAsync();

        var roleViewModel = new List<AssignToRoleViewModel>();

        var userRoles = await userManager.GetRolesAsync(user);

        foreach (var role in roles)
        {
            var assignToRoleViewModel = new AssignToRoleViewModel() { Id = role.Id, Name = role.Name };
            if (userRoles.Contains(role.Name))
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
