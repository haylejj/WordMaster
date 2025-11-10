using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WordMaster.Application.Requests;
using WordMaster.Application.Services.Abstract;
using WordMaster.Application.ViewModels;
using WordMaster.Domain.Entities;
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
        if (!ModelState.IsValid) // bir hata var ise validate de
        {
            return View(viewModel);
        }
        returnUrl = returnUrl ?? Url.Action("Index", "Word");

        var request = new LoginRequest
        {
            Email = viewModel.Email,
            Password = viewModel.Password,
            RememberMe = viewModel.RememberMe
        };

        var userResult = await loginService.FindByEmailAsync(request.Email!);
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

        if (!userResult.IsSuccess || userResult.Data == null)
        {
            ModelState.AddModelErrorList(new List<string>() { "Email veya şifre yanlış" });
            await logHistoryService.RecordAsync(null, request.Email, ipAddress, false, "UserLogin");
            return View(viewModel);
        }

        var login = await loginService.LoginAsync(request, userResult.Data);

        await logHistoryService.RecordAsync(userResult.Data.Id, request.Email, ipAddress, login.IsSuccess, "UserLogin");

        if (login.IsSuccess)
        {
            return Redirect(returnUrl!);
        }

        ModelState.AddModelErrorList(new List<string> { "Email veya şifre yanlış" });
        return View(viewModel);
    }
    [HttpGet("ForgetPassword")]
    public IActionResult ForgetPassword()
    {
        return View();
    }

    [HttpPost("ForgetPassword")]
    public async Task<IActionResult> ForgetPassword(ForgetPasswordViewModel viewModel)
    {
        if (!ModelState.IsValid)
        {
            return View();
        }
        var request = new ForgetPasswordRequest
        {
            Email = viewModel.Email
        };
        var user = await loginService.FindByEmailAsync(request.Email!);
        if (!user.IsSuccess || user.Data == null)
        {
            ModelState.AddModelError(string.Empty, "Bu email adresine sahip kullanıcı bulunamamıştır.");
            return View();
        }

        var passwordResetToken = await loginService.GeneratePasswordResetTokenAsync(user.Data.Id);// Şimdi biz özel token ürettik. Şifre değiştirmede kullanılacak 

        var passwordResetLink = Url.Action("ResetPassword", null, new { userId = user.Data.Id, token = passwordResetToken.Data }, HttpContext.Request.Scheme); // bu linkin ömrünü program.cs de belirliycez.
        //Örnek link
        // https://localhost:7289?userId=12213&token=aasdfasdfsdf

        // email e link gönderme metodu.
        await emailService.SendResetPasswordLinkToEmailAsync(passwordResetLink!, user.Data.Email!);
        //
        TempData["success"] = "Şifre yenileme linki e-posta adresinize gönderilmiştir.";

        return RedirectToAction(nameof(ForgetPassword));
    }
    [HttpGet("ResetPassword")]
    public IActionResult ResetPassword(string userId, string token)
    {
        TempData["userId"] = userId;
        TempData["token"] = token;
        return View();
    }

    [HttpPost("ResetPassword")]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel viewModel)
    {
        if (!ModelState.IsValid)
        {
            return View();
        }
        var userId = TempData["userId"];
        var token = TempData["token"];
        if (userId == null || token == null)
        {
            throw new Exception("Bir hata meydana geldi");
        }
        var hasUser = await userManager.FindByIdAsync(userId.ToString()!);
        if (hasUser == null)
        {
            ModelState.AddModelErrorList(new List<string>() { "Kullanıcı bulunamamıştır." });
            return View();
        }
        var request = new ResetPasswordRequest
        {
            Password = viewModel.Password,
            PasswordConfirm = viewModel.PasswordConfirm
        };
        var result = await userManager.ResetPasswordAsync(hasUser, token!.ToString()!, request.Password!);
        if (result.Succeeded)
        {
            TempData["SuccessMessage"] = "Şifreniz başarıyla yenilenmiştir.";
        }
        else
        {
            ModelState.AddModelErrorList(result.Errors.Select(x => x.Description).ToList());
            return View();
        }
        return RedirectToAction(nameof(ResetPassword));
    }

}
