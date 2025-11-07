using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WordMaster.Application.Requests;
using WordMaster.Application.Services.Abstract;
using WordMaster.Application.ViewModels;
using WordMaster.WebUI.Extensions;

namespace WordMaster.WebUI.Controllers;

[Authorize]
public class MemberController(IUserService userService) : Controller
{
    public async Task<IActionResult> LogOut()
    {
        await userService.LogOutAsync();
        return RedirectToAction("Login", "Login");
    }
    public async Task<IActionResult> UserEdit()
    {
        ViewBag.genderList = userService.GetGenderSelectList();
        var vm = await userService.GetUserEditViewModelAsync(User.Identity!.Name!);
        return View(vm.Data);
    }
    [HttpPost]
    public async Task<IActionResult> UserEdit(UserEditRequest request)
    {
        if (!ModelState.IsValid)
        {
            return View();
        }
        var edit = await userService.EditUserAsync(request, User.Identity!.Name!);
        if (!edit.IsSuccess)
        {
            ModelState.AddModelErrorList(edit.Data!.Select(x => x.Description).ToList());
        }
        TempData["SuccessMessage"] = "G�ncelleme i�lemi ba�ar�l�.";
        return RedirectToAction(nameof(UserEdit));
    }
    public IActionResult PasswordChange()
    {
        return View();
    }
    [HttpPost]
    public async Task<IActionResult> PasswordChange(PasswordChangeViewModel viewModel)
    {
        if (!ModelState.IsValid)
        {
            return View();
        }
        var request = new PasswordChangeRequest
        {
            PasswordOld = viewModel.PasswordOld,
            PasswordNew = viewModel.PasswordNew,
            PasswordConfirm = viewModel.PasswordConfirm
        };
        var check = await userService.CheckPasswordAsync(User.Identity!.Name!, request.PasswordOld!);
        if (!check.Data)
        {
            ModelState.AddModelError(string.Empty, "Mevcut �ifrenizi yanl�� girdiniz.");
            return View();
        }
        var change = await userService.ChangePasswordAsync(request, User.Identity!.Name!);
        if (!change.IsSuccess)
        {
            ModelState.AddModelErrorList(change.Data!.Select(x => x.Description).ToList());
            return View();
        }
        TempData["SuccessMessage"] = "�ifreniz ba�ar�yla de�i�tirilmi�tir.";

        return View();
    }
    public IActionResult AccessDenied()
    {
        string message = string.Empty;

        message = "Bu sayfay� g�rmeye yetkiniz yoktur.Yetki almak i�in y�neticinizle g�r��ebilirsiniz.";
        ViewBag.message = message;
        return View();
    }
}
