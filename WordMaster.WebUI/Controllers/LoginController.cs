using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WordMaster.Application.Requests;
using WordMaster.Application.Services.Abstract;
using WordMaster.Application.ViewModels;
using WordMaster.Domain.Entities;
using WordMaster.WebUI.Extensions;

namespace WordMaster.WebUI.Controllers;

[Route("/Login")]
public class LoginController(ILoginService loginService, UserManager<AppUser> userManager, IEmailService emailService) : Controller
{
    [HttpGet("")]
    public IActionResult Login()
    {
        // E�er kullan�c� zaten login olmu�sa Word sayfas�na y�nlendir
        return User.Identity?.IsAuthenticated == true ? RedirectToAction("Index", "Word") : View();
    }

    [HttpPost("")]
    public async Task<IActionResult> Login(LoginViewModel viewModel, string? returnUrl = null)
    {
        if (!ModelState.IsValid) // bir hata var ise validate de
        {
            return View();
        }
        returnUrl = returnUrl ?? Url.Action("Index", "Word");

        var request = new LoginRequest
        {
            Email = viewModel.Email,
            Password = viewModel.Password,
            RememberMe = viewModel.RememberMe
        };

        var userResult = await loginService.FindByEmailAsync(request.Email!);
        if (!userResult.IsSuccess || userResult.Data == null)
        {
            ModelState.AddModelErrorList(new List<string>() { "Email veya �ifre yanl��" });
        }

        var result = await loginService.LoginAsync(request, userResult.Data!);

        if (result.IsSuccess)
        {
            return Redirect(returnUrl!);
        }

        ModelState.AddModelErrorList(new List<string> { "Email veya �ifre yanl��" });
        return View();
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
            ModelState.AddModelError(string.Empty, "Bu email adresine sahip kullan�c� bulunamam��t�r.");
            return View();
        }

        var passwordResetToken = await loginService.GeneratePasswordResetTokenAsync(user.Data.Id);// �imdi biz �zel token �rettik. �ifre de�i�tirmede kullan�lacak 

        var passwordResetLink = Url.Action("ResetPassword", null, new { userId = user.Data.Id, token = passwordResetToken.Data }, HttpContext.Request.Scheme); // bu linkin �mr�n� program.cs de belirliycez.
        //�rnek link
        // https://localhost:7289?userId=12213&token=aasdfasdfsdf

        // email e link g�nderme metodu.
        await emailService.SendResetPasswordLinkToEmailAsync(passwordResetLink!, user.Data.Email!);
        //
        TempData["success"] = "�ifre yenileme linki e-posta adresinize g�nderilmi�tir.";

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
            ModelState.AddModelErrorList(new List<string>() { "Kullan�c� bulunamam��t�r." });
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
            TempData["SuccessMessage"] = "�ifreniz ba�ar�yla yenilenmi�tir.";
        }
        else
        {
            ModelState.AddModelErrorList(result.Errors.Select(x => x.Description).ToList());
            return View();
        }
        return RedirectToAction(nameof(ResetPassword));
    }

}
