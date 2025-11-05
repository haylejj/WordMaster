using Core.Entity;
using Core.Service;
using Core.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Service.Service;

public class AdminService(UserManager<AppUser> userManager) : IAdminService
{
    public async Task<List<UserViewModel>> GetUsersAsync()
    {
        var users = await userManager.Users.AsNoTracking().ToListAsync();

        var userViewModel = users.Select(x => new UserViewModel { Id=x.Id, UserName=x.UserName, Email=x.Email }).ToList();
        return userViewModel;
    }
}
