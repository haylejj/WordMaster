using System.Net;
using Microsoft.AspNetCore.Identity;
using WordMaster.Application.Requests.Auth;
using WordMaster.Application.Services.Abstract;
using WordMaster.Domain.Entities;
using WordMaster.Domain.Results;

namespace WordMaster.Infrastructure.Services;

public class RegisterService(UserManager<AppUser> userManager) : IRegisterService
{
    public async Task<ServiceResult> RegisterAsync(RegisterRequest request)
    {
        IdentityResult result = await userManager.CreateAsync(new AppUser() { UserName = request.UserName, Email = request.Email, PhoneNumber = request.Phone }, request.Password!);

        if (!result.Succeeded)
        {
            var errors = result.Errors.Select(e => e.Description).ToList();
            return ServiceResult.Failure(errors, HttpStatusCode.BadRequest);
        }
        else
        {
            AppUser? user = await userManager.FindByNameAsync(request.UserName!);
            if (user == null)
            {
                return ServiceResult.Failure("Kullanıcı bulunamadı.", HttpStatusCode.NotFound);
            }
            await userManager.AddToRoleAsync(user, "user");
            return ServiceResult.SuccessAsCreated();
        }
    }
}
