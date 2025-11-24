using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WordMaster.Application.Requests.Auth;
using WordMaster.Application.Services.Abstract;
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
    public async Task<IActionResult> Login(LoginRequest request)
    {
        if (!ModelState.IsValid)
        {
            return View(request);
        }

        var userResult = await loginService.FindByEmailAsync(request.Email!);
        string? ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

        // IP adresi kontrolü
        if (!string.IsNullOrWhiteSpace(ipAddress))
        {
            ServiceResult<bool> isIpAllowed = await allowedIpAddressService.IsIpAllowedAsync(ipAddress);
            if (!isIpAllowed.Data)
            {
                ModelState.AddModelErrorList(new List<string> { "Bu IP adresinden admin paneline giriş yapma yetkiniz yok." });
                await logHistoryService.RecordAsync(null, request.Email, ipAddress, false, "AdminLogin");
                return View(request);
            }
        }

        if (!userResult.IsSuccess || userResult.Data == null)
        {
            ModelState.AddModelErrorList(new List<string> { "Email veya şifre yanlış" });
            await logHistoryService.RecordAsync(null, request.Email, ipAddress, false, "AdminLogin");
            return View(request);
        }

        var user = await userManager.FindByEmailAsync(request.Email!);
        if (user == null)
        {
            ModelState.AddModelErrorList(new List<string> { "Kullanıcı bulunamadı." });
            return View(request);
        }

        // Admin rol kontrolü
        bool isAdmin = await userManager.IsInRoleAsync(user, "admin");
        if (!isAdmin)
        {
            ModelState.AddModelErrorList(new List<string> { "Bu panele erişim yetkiniz yok." });
            await logHistoryService.RecordAsync(user.Id.ToString(), request.Email, ipAddress, false, "AdminLogin");
            return View(request);
        }

        ServiceResult login = await loginService.AdminLoginAsync(request);
        if (!login.IsSuccess)
        {
            ModelState.AddModelErrorList(new List<string> { "Email veya şifre yanlış" });
            await logHistoryService.RecordAsync(user.Id.ToString(), request.Email, ipAddress, false, "AdminLogin");
            return View(request);
        }

        TempData["AdminLoginSuccess"] = "Başarıyla giriş yaptınız.";
        await logHistoryService.RecordAsync(user.Id.ToString(), request.Email, ipAddress, true, "AdminLogin");

        return Redirect("/Admin/Dashboard");
    }
}

