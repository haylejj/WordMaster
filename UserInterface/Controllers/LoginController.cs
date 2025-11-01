using Core.Entity;
using Core.Service;
using Core.Requests;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using UserInterface.Extensions;

namespace UserInterface.Controllers
{
    public class LoginController : Controller
    {
        private readonly ILoginService _loginService;
        private readonly UserManager<AppUser> _userManager;
        private readonly IEmailService _emailService;
        public LoginController(ILoginService loginService, UserManager<AppUser> userManager, IEmailService emailService)
        {
            _loginService = loginService;
            _userManager = userManager;
            _emailService = emailService;
        }

        public IActionResult LogIn()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> LogIn(LoginRequest request, string? returnUrl = null)
        {
            if (!ModelState.IsValid) // bir hata var ise validate de
            {
                return View();
            }
            returnUrl = returnUrl ?? Url.Action("Index", "Word");

            var user = await _loginService.FindByEmailAsync(request.Email!);
            if (user == null)
            {
                ModelState.AddModelErrorList(new List<string>() { "Email veya şifre yanlış" });
            }

            var result = await _loginService.LoginAsync(request, user!);

            if (result)
            {
                return Redirect(returnUrl!);
            }

            ModelState.AddModelErrorList(new List<string> { "Email veya şifre yanlış" });
            return View();
        }
        public IActionResult ForgetPassword()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> ForgetPassword(ForgetPasswordRequest request)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }
            var user = await _loginService.FindByEmailAsync(request.Email!);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Bu email adresine sahip kullanıcı bulunamamıştır.");
                return View();
            }

            var passwordResetToken = await _loginService.GeneratePasswordResetTokenAsync(user.Id);// şimdi biz özel token ürettik. şifre değiştirmede kullanılacak 

            var passwordResetLink = Url.Action("ResetPassword", "Login", new { userId = user.Id, token = passwordResetToken }, HttpContext.Request.Scheme); // bu linkin ömrünü program.cs de belirliycez.
            //örnek link
            // https://localhost:7289?userId=12213&token=aasdfasdfsdf

            // email e link gönderme metodu.
            await _emailService.SendResetPasswordLinkToEmailAsync(passwordResetLink!, user.Email);
            //
            TempData["success"] = "Şifre yenileme linki e-posta adresinize gönderilmiştir.";

            return RedirectToAction(nameof(ForgetPassword));
        }
        public IActionResult ResetPassword(string userId, string token)
        {
            TempData["userId"] = userId;
            TempData["token"] = token;
            return View();
        }
        [HttpPost]
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
            var hasUser = await _userManager.FindByIdAsync(userId.ToString()!);
            if (hasUser == null)
            {
                ModelState.AddModelErrorList(new List<string>() { "Kullanıcı bulunamamıştır." });
                return View();
            }
            var result = await _userManager.ResetPasswordAsync(hasUser, token!.ToString()!, request.Password!);
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
}
