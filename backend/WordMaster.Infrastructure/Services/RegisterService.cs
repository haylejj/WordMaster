using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System.Net;
using WordMaster.Application.Requests.Auth;
using WordMaster.Application.Services.Abstract;
using WordMaster.Domain.Entities;
using WordMaster.Domain.Results;
using Microsoft.Extensions.Options;
using WordMaster.Domain.Configuration;
using WordMaster.Infrastructure.Helpers;

namespace WordMaster.Infrastructure.Services;

public class RegisterService(
    UserManager<AppUser> userManager,
    ILogger<RegisterService> logger,
    IEmailService emailService,
    IOptions<UrlsSettings> urlSettings,
    IDataProtectionHelper dataProtectionHelper) : IRegisterService
{
    public async Task<ServiceResult> RegisterAsync(RegisterRequest request)
    {
        logger.LogInformation("Attempting to register new user with username: {UserName}, email: {Email}", request.UserName, request.Email);

        AppUser newUser = new()
        {
            UserName = request.UserName,
            Email = request.Email,
            PhoneNumber = request.Phone,
            Gender = request.Gender
        };

        IdentityResult result = await userManager.CreateAsync(newUser, request.Password!);

        if (!result.Succeeded)
        {
            List<string> errors = result.Errors.Select(e => e.Description).ToList();
            logger.LogWarning("Registration failed for user {UserName}. Errors: {Errors}", request.UserName, string.Join(", ", errors));
            return ServiceResult.Failure(errors, HttpStatusCode.BadRequest);
        }

        AppUser? user = await userManager.FindByNameAsync(request.UserName!);
        if (user == null)
        {
            logger.LogError("User {UserName} created but not found immediately after creation.", request.UserName);
            return ServiceResult.Failure("Kullanıcı bulunamadı.", HttpStatusCode.NotFound);
        }

        await userManager.AddToRoleAsync(user, "user");

        // Email Confirmation Logic
        string token = await userManager.GenerateEmailConfirmationTokenAsync(user);

        string encryptedUserId = dataProtectionHelper.Encrypt(user.Id.ToString());
        string encodedEncryptedUserId = WebUtility.UrlEncode(encryptedUserId);
        string encodedToken = WebUtility.UrlEncode(token);

        string baseUrl = urlSettings.Value.Client;
        string confirmLink = $"{baseUrl}/confirm-email?userId={encodedEncryptedUserId}&token={encodedToken}";

        await emailService.SendEmailConfirmationLinkAsync(confirmLink, user.Email!);

        logger.LogInformation("User {UserName} registered successfully. Confirmation email sent.", request.UserName);
        return ServiceResult.SuccessAsCreated();
    }

    public async Task<ServiceResult> ConfirmEmailAsync(string userId, string token)
    {
        string decryptedUserId = dataProtectionHelper.Decrypt(userId);
        if (string.IsNullOrEmpty(decryptedUserId))
        {
            return ServiceResult.Failure("Geçersiz kullanıcı bilgisi.", HttpStatusCode.BadRequest);
        }

        AppUser? user = await userManager.FindByIdAsync(decryptedUserId);
        if (user == null)
        {
            return ServiceResult.Failure("Kullanıcı bulunamadı.", HttpStatusCode.NotFound);
        }

        IdentityResult result = await userManager.ConfirmEmailAsync(user, token);
        if (!result.Succeeded)
        {
            List<string> errors = result.Errors.Select(e => e.Description).ToList();
            return ServiceResult.Failure(errors, HttpStatusCode.BadRequest);
        }

        logger.LogInformation("User {UserId} email confirmed successfully.", user.Id);
        return ServiceResult.Success(HttpStatusCode.OK);
    }
}
