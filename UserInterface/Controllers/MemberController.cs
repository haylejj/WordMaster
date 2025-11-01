using Core.Service;
using Core.Requests;
using Core.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserInterface.Extensions;

namespace UserInterface.Controllers
{
    [Authorize]
    public class MemberController : Controller
    {
        private readonly IMemberService _memberService;

        public MemberController(IMemberService memberService)
        {
            _memberService = memberService;
        }

        public async Task<IActionResult> LogOut()
        {
            await _memberService.LogOutAsync();
            return RedirectToAction("LogIn", "Login");
        }
        public async Task<IActionResult> UserEdit()
        {
            ViewBag.genderList = _memberService.GetGenderSelectList();
            return View(await _memberService.GetUserEditViewModelAsync(User.Identity!.Name!));
        }
        [HttpPost]
        public async Task<IActionResult> UserEdit(UserEditRequest request)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }
            var (isSuccess, error) = await _memberService.EditUserAsync(request, User.Identity!.Name!);
            if (!isSuccess)
            {
                ModelState.AddModelErrorList(error!.Select(x => x.Description).ToList());
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
            if (!await _memberService.CheckPasswordAsync(User.Identity!.Name!, request.PasswordOld!))
            {
                ModelState.AddModelError(string.Empty, "Mevcut şifrenizi yanlış girdiniz.");
                return View();
            }
            var (isSuccess, error) = await _memberService.ChangePasswordAsync(request, User.Identity!.Name!);
            if (!isSuccess)
            {
                ModelState.AddModelErrorList(error!.Select(x => x.Description).ToList());
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
}
