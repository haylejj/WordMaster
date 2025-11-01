using Core.Entity;
using Core.Requests;
using Core.Service;
using Core.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Service.Service;

public class RoleService : IRoleService
{
    private readonly RoleManager<AppRole> _roleManager;
    private readonly UserManager<AppUser> _userManager;

    public RoleService(RoleManager<AppRole> roleManager, UserManager<AppUser> userManager)
    {
        _roleManager = roleManager;
        _userManager = userManager;
    }

    public async Task<List<RoleViewModel>> GetRoleListAsync()
    {
        var roles = await _roleManager.Roles.AsNoTracking().ToListAsync();
        var roleViewModel = roles.Select(x => new RoleViewModel() { Id = x.Id, Name = x.Name }).ToList();
        return roleViewModel;
    }
    public async Task<(bool, IEnumerable<IdentityError>?)> CreateRoleAsync(RoleCreateRequest request)
    {
        var result = await _roleManager.CreateAsync(new AppRole() { Name = request.Name });

        if (!result.Succeeded)
        {
            return (false, result.Errors);
        }
        else { return (true, null); }
    }
    public async Task<(bool, RoleUpdateViewModel?)> FindByIdReturnRoleUpdateViewModelAsync(string id)
    {
        var role = await _roleManager.FindByIdAsync(id);
        if (role == null)
        {
            return (false, null);
        }
        var roleUpdateViewModel = new RoleUpdateViewModel() { Id = role.Id, Name = role.Name };
        return (true, roleUpdateViewModel);
    }

    public async Task<(bool, IEnumerable<IdentityError>?)> UpdateRoleAsync(RoleUpdateRequest request)
    {
        var role = await _roleManager.FindByIdAsync(request.Id);
        if (role == null)
        {
            return (false, null);
        }
        role.Name = request.Name;
        var result = await _roleManager.UpdateAsync(role);
        if (!result.Succeeded) { return (false, result.Errors); }

        else { return (true, null); }
    }
    public async Task<(bool, IEnumerable<IdentityError>?)> DeleteRoleAsync(string id)
    {
        var role = await _roleManager.FindByIdAsync(id);

        var result = await _roleManager.DeleteAsync(role);
        if (!result.Succeeded)
        {
            return (false, result.Errors);

        }
        else { return (true, null); }
    }

    public async Task<List<AssignToRoleViewModel>> GetRoleByIdReturnAssignToRoleAsync(string id)
    {
        var user = await _userManager.FindByIdAsync(id);

        var roles = await _roleManager.Roles.ToListAsync();

        var roleViewModel = new List<AssignToRoleViewModel>();

        var userRoles = await _userManager.GetRolesAsync(user);

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
        var user = await _userManager.FindByIdAsync(id);

        foreach (var role in request)
        {
            if (role.Exist)
            {
                await _userManager.AddToRoleAsync(user, role.Name);
            }
            else await _userManager.RemoveFromRoleAsync(user, role.Name);
        }
    }
}
