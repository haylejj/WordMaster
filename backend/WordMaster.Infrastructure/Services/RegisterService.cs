using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using WordMaster.Application.Helpers;
using WordMaster.Application.Requests.Auth;
using WordMaster.Application.Services.Abstract;
using WordMaster.Application.Constants;
using WordMaster.Domain.Configuration;
using WordMaster.Domain.Entities;
using WordMaster.Domain.Results;

namespace WordMaster.Infrastructure.Services;

public class RegisterService(
    UserManager<AppUser> userManager,
    ILogger<RegisterService> logger,
    IEmailService emailService,
    IOptions<UrlsSettings> urlSettings,
    IDataProtectionHelper dataProtectionHelper,
    IUsernameHelper usernameHelper) : IRegisterService
{
    public async Task<ServiceResult> RegisterAsync(RegisterRequest request)
    {
        // FirstName + LastName'den benzersiz username oluştur
        string username = await usernameHelper.GenerateUniqueUsernameAsync(request.FirstName, request.LastName);

        AppUser newUser = new()
        {
            UserName = username,
            Email = request.Email,
            PhoneNumber = request.Phone,
            FirstName = request.FirstName,
            LastName = request.LastName
        };

        IdentityResult result = await userManager.CreateAsync(newUser, request.Password!);

        if (!result.Succeeded)
        {
            List<string> errors = result.Errors.Select(e => e.Description).ToList();
            logger.LogWarning("Registration failed for Email {Email}. Errors: {Errors}", request.Email, string.Join(", ", errors));
            return ServiceResult.Failure(errors, HttpStatusCode.BadRequest);
        }

        AppUser? user = await userManager.FindByNameAsync(username);
        if (user == null)
        {
            logger.LogError("Email {Email} created but not found immediately after creation.", request.Email);
            return ServiceResult.Failure("Kullanıcı bulunamadı.", HttpStatusCode.NotFound);
        }

        await userManager.AddToRoleAsync(user, "user");

        //Metric: Increment users registered counter
        OpenTelemetryMetric.UsersRegistered.Add(1);

        // Email Confirmation Logic
        string token = await userManager.GenerateEmailConfirmationTokenAsync(user);

        string encryptedUserId = dataProtectionHelper.Encrypt(user.Id.ToString());
        string encodedEncryptedUserId = WebUtility.UrlEncode(encryptedUserId);
        string encodedToken = WebUtility.UrlEncode(token);

        string baseUrl = urlSettings.Value.Client;
        string confirmLink = $"{baseUrl}/confirm-email?userId={encodedEncryptedUserId}&token={encodedToken}";

        await emailService.SendEmailConfirmationLinkAsync(confirmLink, user.Email!);

        logger.LogInformation("Email {Email} registered successfully. Confirmation email sent.", request.Email);
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
