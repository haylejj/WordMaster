using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WordMaster.Application.Requests.Auth;
using WordMaster.Application.Services.Abstract;
using WordMaster.Application.ViewModels.Auth;
using WordMaster.Domain.Entities;
using WordMaster.Domain.Results;
using WordMaster.WebUI.Extensions;

namespace WordMaster.WebUI.Controllers;

[Route("/Login")]
public class LoginController(ILoginService loginService, UserManager<AppUser> userManager, IEmailService emailService, ILogHistoryService logHistoryService) : Controller
{
    [HttpGet("")]
    public IActionResult Login()
    {
        // Eğer kullanıcı zaten login olmuşsa Word sayfasına yönlendir
        return User.Identity?.IsAuthenticated == true ? RedirectToAction("Index", "Word") : View();
    }

    [HttpPost("")]
    public async Task<IActionResult> Login(LoginViewModel viewModel, string? returnUrl = null)
    {
        // 6 karakterden kısa şifre girilirse DB kontrolüne gitmeden direk hata dön
        if (viewModel.Password?.Length < 6)
        {
            ModelState.Remove("Password");
            ModelState.AddModelErrorList(new List<string>() { "Email veya şifre yanlış" });
            return View(viewModel);
        }

        if (!ModelState.IsValid)
        {
            return View(viewModel);
        }
        returnUrl ??= Url.Action("Index", "Word");

        LoginRequest request = new()
        {
            Email = viewModel.Email,
            Password = viewModel.Password,
            RememberMe = viewModel.RememberMe
        };

        Result<AppUser> userResult = await loginService.FindByEmailAsync(request.Email!);
        string? ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

        if (!userResult.IsSuccess || userResult.Data == null)
        {
            ModelState.AddModelErrorList(new List<string>() { "Email veya şifre yanlış" });
            await logHistoryService.RecordAsync(null, request.Email, ipAddress, false, "UserLogin");
            return View(viewModel);
        }

        Result login = await loginService.LoginAsync(request, userResult.Data);

        await logHistoryService.RecordAsync(userResult.Data.Id.ToString(), request.Email, ipAddress, login.IsSuccess, "UserLogin");

        if (login.IsSuccess)
        {
            return !string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl) ? Redirect(returnUrl) : RedirectToAction("Index", "Word");
        }

        ModelState.AddModelErrorList(new List<string> { "Email veya şifre yanlış" });
        return View(viewModel);
    }
    [HttpGet("/ForgetPassword")]
    public IActionResult ForgetPassword()
    {
        return View();
    }

    [HttpPost("/ForgetPassword")]
    public async Task<IActionResult> ForgetPassword(ForgetPasswordViewModel viewModel)
    {
        if (!ModelState.IsValid)
        {
            return View();
        }

        ForgetPasswordRequest request = new()
        {
            Email = viewModel.Email
        };
        Result<AppUser> user = await loginService.FindByEmailAsync(request.Email!);
        if (!user.IsSuccess || user.Data == null)
        {
            // Güvenlik gereği, kullanıcı bulunamasa bile sanki işlem başarılıymış gibi mesaj dönüyoruz.
            // Böylece kötü niyetli kişiler sistemde hangi emailin kayıtlı olduğunu anlayamaz.
            TempData["success"] = "Eğer böyle bir kullanıcı varsa, şifre yenileme linki e-posta adresinize gönderilmiştir.";
            return RedirectToAction(nameof(ForgetPassword));
        }

        Result<string> passwordResetToken = await loginService.GeneratePasswordResetTokenAsync(user.Data.Id.ToString());// Şimdi biz özel token ürettik. Şifre değiştirmede kullanılacak 

        string passwordResetLink = Url.Action("ResetPassword", null, new { userId = user.Data.Id.ToString(), token = passwordResetToken.Data }, HttpContext.Request.Scheme)!; // bu linkin ömrünü program.cs de belirliycez.
        //Örnek link
        // https://localhost:7289?userId=12213&token=aasdfasdfsdf

        // email e link gönderme metodu.
        await emailService.SendResetPasswordLinkToEmailAsync(passwordResetLink!, user.Data.Email!);
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
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel viewModel)
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
        AppUser? hasUser = await userManager.FindByIdAsync(userId.ToString()!);
        if (hasUser == null)
        {
            ModelState.AddModelErrorList(new List<string>() { "Kullanıcı bulunamamıştır." });
            return View();
        }
        ResetPasswordRequest request = new()
        {
            Password = viewModel.Password,
            PasswordConfirm = viewModel.PasswordConfirm
        };
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
