using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using WordMaster.Application.Requests.Auth;
using WordMaster.Application.Services.Abstract;
using WordMaster.Application.Responses.Auth;
using WordMaster.Domain.Results;
using WordMaster.WebUI.Extensions;

namespace WordMaster.WebUI.Controllers;

[Route("/Register")]
public class RegisterController(IRegisterService registerService) : Controller
{
    [HttpGet("")]
    public IActionResult Register()
    {
        return View();
    }

    [HttpPost("")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        if (!ModelState.IsValid)
        {
            return View();
        }

        ServiceResult result = await registerService.RegisterAsync(request);

        if (!result.IsSuccess)
        {
            if (result.ErrorList != null)
            {
                ModelState.AddModelErrorList(result.ErrorList);
            }
            return View();
        }

        TempData["SuccessMessage"] = "Kayıt olma işlemi başarıyla tamamlanmıştır";

        return View(nameof(Register));
    }
}
