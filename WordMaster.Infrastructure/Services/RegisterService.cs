using Microsoft.AspNetCore.Identity;
using System.Net;
using WordMaster.Application.Requests.Auth;
using WordMaster.Application.Services.Abstract;
using WordMaster.Domain.Entities;
using WordMaster.Domain.Results;

using Microsoft.Extensions.Logging;

namespace WordMaster.Infrastructure.Services;

public class RegisterService(UserManager<AppUser> userManager, ILogger<RegisterService> logger) : IRegisterService
{
    public async Task<ServiceResult> RegisterAsync(RegisterRequest request)
    {
        logger.LogInformation("Attempting to register new user with username: {UserName}, email: {Email}", request.UserName, request.Email);

        IdentityResult result = await userManager.CreateAsync(new AppUser() { UserName = request.UserName, Email = request.Email, PhoneNumber = request.Phone, Gender = request.Gender }, request.Password!);

        if (!result.Succeeded)
        {
            List<string> errors = result.Errors.Select(e => e.Description).ToList();
            logger.LogWarning("Registration failed for user {UserName}. Errors: {Errors}", request.UserName, string.Join(", ", errors));
            return ServiceResult.Failure(errors, HttpStatusCode.BadRequest);
        }
        else
        {
            AppUser? user = await userManager.FindByNameAsync(request.UserName!);
            if (user == null)
            {
                logger.LogError("User {UserName} created but not found immediately after creation.", request.UserName);
                return ServiceResult.Failure("Kullanıcı bulunamadı.", HttpStatusCode.NotFound);
            }
            await userManager.AddToRoleAsync(user, "user");

            logger.LogInformation("User {UserName} registered successfully.", request.UserName);
            return ServiceResult.SuccessAsCreated();
        }
    }
}
