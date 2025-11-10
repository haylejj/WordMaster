using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WordMaster.Application.Requests;
using WordMaster.Application.Services.Abstract;
using WordMaster.Application.ViewModels;
using WordMaster.Domain.Entities;
using WordMaster.WebUI.Extensions;

namespace WordMaster.WebUI.Areas.Admin.Controllers;

[Area("Admin")]
public class AuthController(ILoginService loginService, UserManager<AppUser> userManager) : Controller
{
    [HttpGet]
    [AllowAnonymous]
    [Route("AdminArea/Login")]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    [Route("AdminArea/Login")]
    public async Task<IActionResult> Login(LoginViewModel viewModel)
    {
        if (!ModelState.IsValid)
        {
            return View(viewModel);
        }

        var userResult = await loginService.FindByEmailAsync(viewModel.Email!);
        if (!userResult.IsSuccess || userResult.Data == null)
        {
            ModelState.AddModelErrorList(new List<string> { "Email veya şifre yanlış" });
            return View(viewModel);
        }

        // Admin rol kontrolü
        var isAdmin = await userManager.IsInRoleAsync(userResult.Data, "admin");
        if (!isAdmin)
        {
            ModelState.AddModelErrorList(new List<string> { "Bu panele erişim yetkiniz yok." });
            return View(viewModel);
        }

        var request = new LoginRequest
        {
            Email = viewModel.Email,
            Password = viewModel.Password,
            RememberMe = viewModel.RememberMe
        };

        var login = await loginService.LoginAsync(request, userResult.Data);
        if (!login.IsSuccess)
        {
            ModelState.AddModelErrorList(new List<string> { "Email veya şifre yanlış" });
            return View(viewModel);
        }

        TempData["AdminLoginSuccess"] = "Başarıyla giriş yaptınız.";

        return RedirectToAction(nameof(Login));
    }
}

