using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WordMaster.Application.Requests.Auth;
using WordMaster.Application.Services.Abstract;
using WordMaster.Application.ViewModels.Auth;
using WordMaster.Domain.Entities;
using WordMaster.Domain.Results;
using WordMaster.WebUI.Extensions;

namespace WordMaster.WebUI.Areas.Admin.Controllers;

[Area("Admin")]
public class AuthController(ILoginService loginService, UserManager<AppUser> userManager, ILogHistoryService logHistoryService, IAllowedIpAddressService allowedIpAddressService) : Controller
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

        Result<AppUser> userResult = await loginService.FindByEmailAsync(viewModel.Email!);
        string? ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

        // IP adresi kontrolü
        if (!string.IsNullOrWhiteSpace(ipAddress))
        {
            bool isIpAllowed = await allowedIpAddressService.IsIpAllowedAsync(ipAddress);
            if (!isIpAllowed)
            {
                ModelState.AddModelErrorList(new List<string> { "Bu IP adresinden admin paneline giriş yapma yetkiniz yok." });
                await logHistoryService.RecordAsync(null, viewModel.Email, ipAddress, false, "AdminLogin");
                return View(viewModel);
            }
        }

        if (!userResult.IsSuccess || userResult.Data == null)
        {
            ModelState.AddModelErrorList(new List<string> { "Email veya şifre yanlış" });
            await logHistoryService.RecordAsync(null, viewModel.Email, ipAddress, false, "AdminLogin");
            return View(viewModel);
        }

        // Admin rol kontrolü
        bool isAdmin = await userManager.IsInRoleAsync(userResult.Data, "admin");
        if (!isAdmin)
        {
            ModelState.AddModelErrorList(new List<string> { "Bu panele erişim yetkiniz yok." });
            await logHistoryService.RecordAsync(userResult.Data.Id, viewModel.Email, ipAddress, false, "AdminLogin");
            return View(viewModel);
        }

        LoginRequest request = new()
        {
            Email = viewModel.Email,
            Password = viewModel.Password,
            RememberMe = viewModel.RememberMe
        };

        Result login = await loginService.LoginAsync(request, userResult.Data);
        if (!login.IsSuccess)
        {
            ModelState.AddModelErrorList(new List<string> { "Email veya şifre yanlış" });
            await logHistoryService.RecordAsync(userResult.Data.Id, viewModel.Email, ipAddress, false, "AdminLogin");
            return View(viewModel);
        }

        TempData["AdminLoginSuccess"] = "Başarıyla giriş yaptınız.";
        await logHistoryService.RecordAsync(userResult.Data.Id, viewModel.Email, ipAddress, true, "AdminLogin");

        return Redirect("/Admin/Dashboard");
    }
}

