
using Microsoft.AspNetCore.Identity;
using WordMaster.Application.Requests.Auth;
using WordMaster.Application.Services.Abstract;
using WordMaster.Domain.Entities;
using WordMaster.Domain.Results;

namespace WordMaster.Infrastructure.Services;

public class RegisterService(UserManager<AppUser> userManager) : IRegisterService
{
    public async Task<ServiceResult<IEnumerable<IdentityError>>> RegisterAsync(RegisterRequest request)
    {
        IdentityResult result = await userManager.CreateAsync(new AppUser() { UserName = request.UserName, Email = request.Email, PhoneNumber = request.Phone }, request.Password!);

        if (!result.Succeeded)
        {
            return new ServiceResult<IEnumerable<IdentityError>> { IsSuccess = false, ErrorMessage = "Kayıt başarısız.", Data = result.Errors };
        }
        else
        {
            AppUser? user = await userManager.FindByNameAsync(request.UserName!);
            if (user == null)
            {
                return new ServiceResult<IEnumerable<IdentityError>> { IsSuccess = false, ErrorMessage = "Kullanıcı bulunamadı.", Data = null };
            }
            await userManager.AddToRoleAsync(user, "user");
            return ServiceResult<IEnumerable<IdentityError>>.Success(null);
        }
    }
}
