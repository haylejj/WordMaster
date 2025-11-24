using Microsoft.AspNetCore.Identity;
using WordMaster.Application.Responses.User;
using Microsoft.AspNetCore.Mvc;
using WordMaster.Application.Requests.Auth;
using WordMaster.Application.Services.Abstract;
using WordMaster.Application.Responses.Auth;
using WordMaster.Domain.Entities;
using WordMaster.Domain.Results;
using WordMaster.WebUI.Extensions;
using WordMaster.Infrastructure.Helpers;

namespace WordMaster.WebUI.Controllers;

[Route("/Login")]
public class LoginController(ILoginService loginService, UserManager<AppUser> userManager, IEmailService emailService, ILogHistoryService logHistoryService, IDataProtectionHelper dataProtectionHelper) : Controller
{
    [HttpGet("")]
    public IActionResult Login()
    {
        // Eğer kullanıcı zaten login olmuşsa Word sayfasına yönlendir
        return User.Identity?.IsAuthenticated == true ? RedirectToAction("Index", "Word") : View();
    }

    [HttpPost("")]
    public async Task<IActionResult> Login(LoginRequest request, string? returnUrl = null)
    {
        // 6 karakterden kısa şifre girilirse DB kontrolüne gitmeden direk hata dön
        if (request.Password?.Length < 6)
        {
            ModelState.Remove("Password");
            ModelState.AddModelErrorList(new List<string>() { "Email veya şifre yanlış" });
            return View(request);
        }

        if (!ModelState.IsValid)
        {
            return View(request);
        }
        returnUrl ??= Url.Action("Index", "Word");


        ServiceResult<UserResponse> userResult = await loginService.FindByEmailAsync(request.Email!);
        string? ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

        if (!userResult.IsSuccess || userResult.Data == null)
        {
            ModelState.AddModelErrorList(new List<string>() { "Email veya şifre yanlış" });
            await logHistoryService.RecordAsync(null, request.Email, ipAddress, false, "UserLogin");
            return View(request);
        }

        ServiceResult<LoginResponse> login = await loginService.LoginAsync(request);

        await logHistoryService.RecordAsync(userResult.Data.Id, request.Email, ipAddress, login.IsSuccess, "UserLogin");

        if (login.IsSuccess)
        {
            return !string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl) ? Redirect(returnUrl) : RedirectToAction("Index", "Word");
        }

        ModelState.AddModelErrorList(new List<string> { "Email veya şifre yanlış" });
        return View(request);
    }
    [HttpGet("/ForgetPassword")]
    public IActionResult ForgetPassword()
    {
        return View();
    }

    [HttpPost("/ForgetPassword")]
    public async Task<IActionResult> ForgetPassword(ForgetPasswordRequest request)
    {
        if (!ModelState.IsValid)
        {
            return View();
        }

        ServiceResult<UserResponse> user = await loginService.FindByEmailAsync(request.Email!);
        if (!user.IsSuccess || user.Data == null)
        {
            // Güvenlik gereği, kullanıcı bulunamasa bile sanki işlem başarılıymış gibi mesaj dönüyoruz.
            // Böylece kötü niyetli kişiler sistemde hangi emailin kayıtlı olduğunu anlayamaz.
            TempData["success"] = "Eğer böyle bir kullanıcı varsa, şifre yenileme linki e-posta adresinize gönderilmiştir.";
            return RedirectToAction(nameof(ForgetPassword));
        }

        ServiceResult<string> passwordResetToken = await loginService.GeneratePasswordResetTokenAsync(user.Data.Id);// Şimdi biz özel token ürettik. Şifre değiştirmede kullanılacak 

        string passwordResetLink = Url.Action("ResetPassword", null, new { userId = user.Data.Id, token = passwordResetToken.Data }, HttpContext.Request.Scheme)!; // bu linkin ömrünü program.cs de belirliycez.
        //Örnek link
        // https://localhost:7289?userId=12213&token=aasdfasdfsdf

        // email e link gönderme metodu.
        ServiceResult emailResult = await emailService.SendResetPasswordLinkToEmailAsync(passwordResetLink!, user.Data.Email!);

        if (!emailResult.IsSuccess)
        {
            ModelState.AddModelErrorList(new List<string> { "Email gönderilirken bir hata oluştu. Lütfen daha sonra tekrar deneyiniz." });
            return View();
        }
        //
        TempData["success"] = "Şifre yenileme linki e-posta adresinize gönderilmiştir.";

        return RedirectToAction(nameof(ForgetPassword));
    }
    [HttpGet("/ResetPassword")]
    public IActionResult ResetPassword(string userId, string token)
    {
        TempData["userId"] = userId;
        TempData["token"] = token;
        return View();
    }

    [HttpPost("/ResetPassword")]
    public async Task<IActionResult> ResetPassword(ResetPasswordRequest request)
    {
        if (!ModelState.IsValid)
        {
            return View();
        }
        object? userId = TempData["userId"];
        object? token = TempData["token"];
        if (userId == null || token == null)
        {
            throw new Exception("Bir hata meydana geldi");
        }

        // UserId'yi çöz
        string decryptedUserId = dataProtectionHelper.Decrypt(userId.ToString()!);
        if (string.IsNullOrEmpty(decryptedUserId))
        {
            ModelState.AddModelErrorList(new List<string>() { "Geçersiz bağlantı." });
            return View();
        }

        AppUser? hasUser = await userManager.FindByIdAsync(decryptedUserId);
        if (hasUser == null)
        {
            ModelState.AddModelErrorList(new List<string>() { "Kullanıcı bulunamamıştır." });
            return View();
        }

        IdentityResult result = await userManager.ResetPasswordAsync(hasUser, token!.ToString()!, request.Password!);
        if (result.Succeeded)
        {
            TempData["SuccessMessage"] = "Şifreniz başarıyla yenilenmiştir.";
        }
        else
        {
            ModelState.AddModelErrorList([.. result.Errors.Select(x => x.Description)]);
            return View();
        }
        return RedirectToAction(nameof(ResetPassword));
    }

}
