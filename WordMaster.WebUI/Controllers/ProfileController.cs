using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WordMaster.Application.Requests.Auth;
using WordMaster.Application.Requests.User;
using WordMaster.Application.Responses.User;
using WordMaster.Application.Services.Abstract;
using WordMaster.Domain.Results;
using WordMaster.WebUI.Extensions;

namespace WordMaster.WebUI.Controllers;

[Authorize]
public class ProfileController(IUserService userService) : Controller
{
    [Route("Logout")]
    public async Task<IActionResult> LogOut()
    {
        await userService.LogOutAsync();
        return RedirectToAction("Login", "Login");
    }

    [Route("Profile/Update")]
    public async Task<IActionResult> UpdateProfile()
    {
        ViewBag.genderList = userService.GetGenderSelectList();
        ServiceResult<UserEditResponse> vm = await userService.GetUserEditViewModelAsync(User.Identity!.Name!);
        return View(vm.Data);
    }

    [HttpPost]
    [Route("Profile/Update")]
    public async Task<IActionResult> UpdateProfile(UserEditRequest request)
    {
        ViewBag.genderList = userService.GetGenderSelectList();
        if (!ModelState.IsValid)
        {
            return View();
        }
        ServiceResult edit = await userService.EditUserAsync(request, User.Identity!.Name!);
        if (!edit.IsSuccess)
        {
            ModelState.AddModelErrorList(edit.ErrorList ?? ["Bir hata oluştu."]);
        }
        TempData["SuccessMessage"] = "Güncelleme işlemi başarılı.";
        return RedirectToAction(nameof(UpdateProfile));
    }
    public IActionResult PasswordChange()
    {
        return View();
    }
    [HttpPost]
    public async Task<IActionResult> PasswordChange(ChangePasswordRequest request)
    {
        if (!ModelState.IsValid)
        {
            return View();
        }
        ServiceResult<bool> check = await userService.CheckPasswordAsync(User.Identity!.Name!, request.PasswordOld!);
        if (!check.Data)
        {
            ModelState.AddModelError(string.Empty, "Mevcut Şifrenizi yanlış girdiniz.");
            return View();
        }
        ServiceResult change = await userService.ChangePasswordAsync(request, User.Identity!.Name!);
        if (!change.IsSuccess)
        {
            ModelState.AddModelErrorList(change.ErrorList ?? ["Bir hata oluştu."]);
            return View();
        }
        TempData["SuccessMessage"] = "Şifreniz başarıyla değiştirilmiştir.";

        return View();
    }
    public IActionResult AccessDenied()
    {
        string message = string.Empty;

        message = "Bu sayfayı görmeye yetkiniz yoktur.Yetki almak için yöneticinizle görüşebilirsiniz.";
        ViewBag.message = message;
        return View();
    }
}
