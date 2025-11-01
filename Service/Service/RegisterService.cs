using Core.Entity;
using Core.Requests;
using Core.Service;
using Microsoft.AspNetCore.Identity;

namespace Service.Service;

public class RegisterService : IRegisterService
{

    private readonly UserManager<AppUser> _userManager;
    private readonly RoleManager<AppRole> _roleManager;
    public RegisterService(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public async Task<(bool, IEnumerable<IdentityError>?)> RegisterAsync(RegisterRequest request)
    {
        var result = await _userManager.CreateAsync(new AppUser() { UserName = request.UserName, Email = request.Email, PhoneNumber = request.Phone }, request.Password);

        if (!result.Succeeded)
        {
            return (false, result.Errors);
        }
        else
        {
            var user = await _userManager.FindByNameAsync(request.UserName);
            await _userManager.AddToRoleAsync(user, "member");
            return (true, null);
        }
    }
}
