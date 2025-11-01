using Core.Entity;
using Core.Service;
using Core.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Service.Service;

public class AdminService : IAdminService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly RoleManager<AppRole> _roleManager;
    private readonly SignInManager<AppUser> _signInManager;

    public AdminService(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager, SignInManager<AppUser> signInManager)
    {
        _userManager=userManager;
        _roleManager=roleManager;
        _signInManager=signInManager;
    }

    public async Task<List<UserViewModel>> GetUsersAsync()
    {
        var users = await _userManager.Users.AsNoTracking().ToListAsync();

        var userViewModel = users.Select(x => new UserViewModel { Id=x.Id, UserName=x.UserName, Email=x.Email }).ToList();
        return userViewModel;
    }
}
