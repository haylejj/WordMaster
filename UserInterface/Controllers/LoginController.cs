using Core.Entity;
using Core.Requests;
using Core.Service;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using UserInterface.Extensions;

namespace UserInterface.Controllers;

[Route("/Login")]
public class LoginController(ILoginService loginService, UserManager<AppUser> userManager, IEmailService emailService) : Controller
{
    [HttpGet("")]
    public IActionResult Login()
    {
        // Eğer kullanıcı zaten login olmuşsa Word sayfasına yönlendir
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Word");
        }
        return View();
    }

    [HttpPost("")]
    public async Task<IActionResult> Login(LoginRequest request, string? returnUrl = null)
    {
        if (!ModelState.IsValid) // bir hata var ise validate de
        {
            return View();
        }
        returnUrl = returnUrl ?? Url.Action("Index", "Word");

        var userResult = await loginService.FindByEmailAsync(request.Email!);
        if (!userResult.IsSuccess || userResult.Data == null)
        {
            ModelState.AddModelErrorList(new List<string>() { "Email veya şifre yanlış" });
        }

        var result = await loginService.LoginAsync(request, userResult.Data!);

        if (result.IsSuccess)
        {
            return Redirect(returnUrl!);
        }

        ModelState.AddModelErrorList(new List<string> { "Email veya şifre yanlış" });
        return View();
    }
    [HttpGet("ForgetPassword")]
    public IActionResult ForgetPassword()
    {
        return View();
    }

    [HttpPost("ForgetPassword")]
    public async Task<IActionResult> ForgetPassword(ForgetPasswordRequest request)
    {
        if (!ModelState.IsValid)
        {
            return View();
        }
        var user = await loginService.FindByEmailAsync(request.Email!);
        if (!user.IsSuccess || user.Data == null)
        {
            ModelState.AddModelError(string.Empty, "Bu email adresine sahip kullanıcı bulunamamıştır.");
            return View();
        }

        var passwordResetToken = await loginService.GeneratePasswordResetTokenAsync(user.Data.Id);// şimdi biz özel token ürettik. şifre değiştirmede kullanılacak 

        var passwordResetLink = Url.Action("ResetPassword", null, new { userId = user.Data.Id, token = passwordResetToken.Data }, HttpContext.Request.Scheme); // bu linkin ömrünü program.cs de belirliycez.
        //örnek link
        // https://localhost:7289?userId=12213&token=aasdfasdfsdf

        // email e link gönderme metodu.
        await emailService.SendResetPasswordLinkToEmailAsync(passwordResetLink!, user.Data.Email);
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
    public async Task<IActionResult> ResetPassword(ResetPasswordRequest request)
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
        return RedirectToAction(nameof(LoginController.ResetPassword));
    }

}
