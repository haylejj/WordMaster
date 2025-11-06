using Core.Requests;
using Core.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserInterface.Extensions;

namespace UserInterface.Controllers;

[Authorize]
public class MemberController(IMemberService memberService) : Controller
{
    public async Task<IActionResult> LogOut()
    {
        await memberService.LogOutAsync();
        return RedirectToAction("Login", "Login");
    }
    public async Task<IActionResult> UserEdit()
    {
        ViewBag.genderList = memberService.GetGenderSelectList();
        var vm = await memberService.GetUserEditViewModelAsync(User.Identity!.Name!);
        return View(vm.Data);
    }
    [HttpPost]
    public async Task<IActionResult> UserEdit(UserEditRequest request)
    {
        if (!ModelState.IsValid)
        {
            return View();
        }
        var edit = await memberService.EditUserAsync(request, User.Identity!.Name!);
        if (!edit.IsSuccess)
        {
            ModelState.AddModelErrorList(edit.Data!.Select(x => x.Description).ToList());
        }
        TempData["SuccessMessage"] = "Güncelleme işlemi başarılı.";
        return RedirectToAction(nameof(UserEdit));
    }
    public IActionResult PasswordChange()
    {
        return View();
    }
    [HttpPost]
    public async Task<IActionResult> PasswordChange(PasswordChangeRequest request)
    {
        if (!ModelState.IsValid)
        {
            return View();
        }
        var check = await memberService.CheckPasswordAsync(User.Identity!.Name!, request.PasswordOld!);
        if (!check.Data)
        {
            ModelState.AddModelError(string.Empty, "Mevcut şifrenizi yanlış girdiniz.");
            return View();
        }
        var change = await memberService.ChangePasswordAsync(request, User.Identity!.Name!);
        if (!change.IsSuccess)
        {
            ModelState.AddModelErrorList(change.Data!.Select(x => x.Description).ToList());
            return View();
        }
        TempData["SuccessMessage"] = "Şifreniz başarıyla değiştirilmiştir.";

        return View();
    }
    public IActionResult AccessDenied(string ReturnUrl)
    {
        string message = string.Empty;

        message = "Bu sayfayı görmeye yetkiniz yoktur.Yetki almak için yöneticinizle görüşebilirsiniz.";
        ViewBag.message = message;
        return View();
    }
}
