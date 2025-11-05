using Core.Entity;
using Core.Requests;
using Core.Service;
using Microsoft.AspNetCore.Identity;

namespace Service.Service;

public class RegisterService(UserManager<AppUser> userManager) : IRegisterService
{
    public async Task<(bool, IEnumerable<IdentityError>?)> RegisterAsync(RegisterRequest request)
    {
        var result = await userManager.CreateAsync(new AppUser() { UserName = request.UserName, Email = request.Email, PhoneNumber = request.Phone }, request.Password);

        if (!result.Succeeded)
        {
            return (false, result.Errors);
        }
        else
        {
            var user = await userManager.FindByNameAsync(request.UserName);
            await userManager.AddToRoleAsync(user, "user");
            return (true, null);
        }
    }
}
